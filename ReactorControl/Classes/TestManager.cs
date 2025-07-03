using System.Diagnostics;
using System.IO;
using System.Timers;
using Microsoft.Extensions.Logging;
using ReactorControl.Types;
using static System.Windows.Forms.AxHost;
using Timer = System.Timers.Timer;

namespace ReactorControl.Classes;

public class TestManager
{
    private readonly ILogger<TestManager> _logger;
    private readonly Stopwatch _setpointTimer = new();
    private readonly TestState _currentState = TestState.Idle;
    private readonly StateStore _store;
    private readonly object _storeLock = new();
    private readonly Stopwatch _testTimer = new();

    //private readonly Stopwatch _connectionTimer = new();
    //private readonly Stopwatch _watchdogTimer = new();

    private bool _powerMode, _testMode, _testFinished, _errorIr, _errorMag, _errorPower;
    private decimal _targetTemp, _deltaTemp, _targetHoldTime, _emissivity;
    private decimal _pidKp, _pidKi, _pidKd;
    private PowerStep _powerModeStep = PowerStep.Power100;
    private List<DataPoint> _dataPoints = [];


    public Timer WatchDogTimer = new() {
        Interval = 3000,
        AutoReset = false,
        Enabled = false
    };
    
    public Timer ConnectionTimer = new() {
        Interval = 1000,
        AutoReset = false,
        Enabled = false
    };


    public TestManager(StateStore store, ILogger<TestManager> logger)
    {
        
        _store = store;
        _logger = logger;
        WatchDogTimer.Elapsed += OnWatchDogElapsed;
        ConnectionTimer.Elapsed += OnConnectionElapsed;
    }

    public void OnWatchDogElapsed(object? sender, ElapsedEventArgs e) {
        
    }
    public void OnConnectionElapsed(object? sender, ElapsedEventArgs e) {
        //todo implement a better reset mechanism
        var deviceState = new DeviceState {
            IsAck = true,
            GoodAck = false,
            LastCommand = CommandLastSent.Command,
        };

        var deviceData = new DeviceData();
        _store.Update(deviceState, deviceData);
    }

    private AppCommand CommandLastSent { get; set; }

    public decimal TargetTemp
    {
        get => _targetTemp;
        set
        {
            if (value == TargetTemp) return;
            _targetTemp = value;
        }
    }

    public decimal DeltaTemp
    {
        get => _deltaTemp;
        set
        {
            if (value == DeltaTemp) return;
            _deltaTemp = value;
        }
    }

    public decimal TargetHoldTime
    {
        get => _targetHoldTime;
        set
        {
            if (value == TargetHoldTime) return;
            _targetHoldTime = value;
        }
    }

    public decimal Emissivity
    {
        get => _emissivity;
        set
        {
            if (value == Emissivity) return;
            _emissivity = value;
        }
    }

    public decimal PidKp
    {
        get => _pidKp;
        set
        {
            if (value == PidKp) return;
            _pidKp = value;
        }
    }
    public decimal PidKi
    {
        get => _pidKi;
        set
        {
            if (value == PidKi) return;
            _pidKi = value;
        }
    }

    public decimal PidKd {
        get => _pidKd;
        set {
            if (value == PidKd) return;
            _pidKd = value;
        }
    }
    public bool PowerMode
    {
        get => _powerMode;
        set
        {
            if (value == PowerMode) return;
            _powerMode = value;
        }
    }

    public bool TestMode
    {
        get => _testMode;
        set
        {
            if (value == TestMode) return;
            _testMode = value;
        }
    }

    public bool TestFinished
    {
        get => _testFinished;
        set
        {
            if (value == TestFinished) return;
            _testFinished = value;
        }
    }

    public PowerStep PowerModeStep
    {
        get => _powerModeStep;
        set
        {
            if (value == PowerModeStep) return;
            _powerModeStep = value;
        }
    }

    public event Action<AppCommand>? CommandRequested;


    #region public methods

    public void SendCheckStatusCommand() {
        var command = new AppCommand { Command = Commands.Init };
        PostCommand(command);
    }

    public void SendStartTestCommand()
    {

        TestFinished = false;
        //_setpointHit = false;
        _testTimer.Restart();
        _setpointTimer.Reset();
        _errorIr = false;
        _errorMag = false;
        _errorPower = false;
        ResetWatchDog(true);

        var command = new AppCommand
        {
            Command = Commands.Start,
            Specs = new TestSpecs
            {
                DeltaTemp = (float)DeltaTemp,
                TargetTemp = (float)TargetTemp,
                TargetHoldTime = (float)TargetHoldTime,
                Emissivity = (float)Emissivity,
                PidKp = (float)PidKp,
                PidKi = (float)PidKi,
                PidKd = (float)PidKd,
                PowerMode = PowerMode,
                PowerModeStep = PowerModeStep,
                TestMode = TestMode
            }
        };
        _logger.LogDebug(" ** ------------------------------------------ ** New Run Started ** ------------------------------------------ ** ");
        _logger.LogDebug("Starting test, target temp: {0}, delta temp: {1}, hold time: {2}, emissivity: {3}, power mode: {4}, test mode: {4}", DeltaTemp, TargetTemp, TargetHoldTime, Emissivity, PowerMode, TestMode);
        PostCommand(command);
    }

    public void SendCommand(Commands command)
    {
        var newCommand = new AppCommand { Command = command };
        if (command == Commands.Stop) AutoSaveRun();

        PostCommand(newCommand);
    }

    public void ExportDataToCSV() {
        if (_dataPoints.Count == 0) return;
        var directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        WriteData(directory);
        _logger.LogDebug("Data saved on desktop");
    }

    public void OnAckReceived(AppMessage message) {
        if (message.AckedCommand == CommandLastSent.Command)
        {
            ConnectionTimer.Stop();
        }

        var deviceState = new DeviceState {
            IsAck = true,
            GoodAck = message.AckedCommand == CommandLastSent.Command,
            LastCommand = CommandLastSent.Command,
        };

        var deviceData = new DeviceData();
        _store.Update(deviceState, deviceData);
        _logger.LogDebug("Ack received for {0} command", message.AckedCommand);
    }

    public void OnMessageReceived(AppMessage message)
    {
        ResetWatchDog(true);
        var testTime = _testTimer.Elapsed.TotalSeconds;
        
        var dataPoint = new DataPoint {
            TimeStamp = testTime,
            Temp = message.ThermometerSample.Data,
            Load = message.PowerMeter.Data,
            Step = message.PowerStep,
            MagTemp = message.ThermometerMagnetron.Data,
        };

        var testFinished = NotifyTestFinished(message.State);
        var remainingTime = UpdateBasedOnSetpoint(message.SetpointState);
        bool setpointHit = message.SetpointState == SetpointState.Hit;

        float highestTemp = message.ThermometerSample.Data;
        if (_dataPoints.Count > 1)
        {
            highestTemp = _dataPoints.Max(point => point.Temp);
        }

        float rateTemp = 0;
        if (_dataPoints.Count() > 10) {
            //var points = _dataPoints.Skip(_dataPoints.Count - 10).Take(10);
            //rateTemp = points.Average(p => p.Temp);
            rateTemp = _dataPoints[^1].Temp - _dataPoints[^10].Temp;
        }

        var newReport = CheckForErrorsOrMessages(message);

        var deviceState = new DeviceState {
            State = message.State,
            IsAck = false,
            GoodAck = false,
            SetpointHit = setpointHit,
            SetpointState = message.SetpointState,
            TimeStamp = testTime,
            RemainingTime = remainingTime,
            CurrentTemp = message.ThermometerSample.Data,
            HighestTemp = highestTemp,
            RateTemp = rateTemp,
            Power = message.PowerMeter.Data,
            PowerStep = message.PowerStep,
            MagnetronTemp = message.ThermometerMagnetron.Data,
            TestFinished = testFinished,
            HasError = newReport.Item2,
            StatusMessage = newReport.Item1
        };

        var deviceData = new DeviceData {
            Data = _dataPoints.ToArray()
        };

        _logger.LogDebug("Updating UI with data");
        _store.Update(deviceState, deviceData);
        _dataPoints.Add(dataPoint);
    }

    private double UpdateBasedOnSetpoint(SetpointState state)
    {
        var holdTime = (double)_targetHoldTime;
        switch (state)
        {
            case SetpointState.Error or SetpointState.Elapsed:
                _setpointTimer.Stop();
                _logger.LogDebug("Setpoint has elapsed");
                return 0;
            case SetpointState.Hit when !_setpointTimer.IsRunning:
                _setpointTimer.Start();
                return holdTime;
            case SetpointState.Hit:
                return holdTime - _setpointTimer.Elapsed.Seconds;
            default:
                return 0;
        }
    }

    private (string statusMessage, bool hasError) CheckForErrorsOrMessages(AppMessage message) {
        var messages = new List<string>();
        bool overallError = false;

        var (irMsg, irErr) = ProcessSensorStatus("IR Sensor", message.ThermometerSample);
        if (!string.IsNullOrEmpty(irMsg) && !_errorIr)
        {
            _errorIr = true;
            messages.Add(irMsg);
        }
        overallError |= irErr;

        var (magMsg, magErr) = ProcessSensorStatus("Magnetron Thermocouple", message.ThermometerMagnetron);
        if (!string.IsNullOrEmpty(magMsg) && !_errorMag)
        {
            _errorMag = true;
            messages.Add(magMsg);
        }
        overallError |= magErr;

        var (powMsg, powErr) = ProcessSensorStatus("Load Sensor", message.PowerMeter);
        if (!string.IsNullOrEmpty(powMsg) && !_errorPower)
        {
            _errorPower = true;
            messages.Add(powMsg);
        }
        overallError |= powErr;

        string finalStatusMessage = string.Join(", ", messages);

        return (finalStatusMessage, overallError);
    }

    private (string message, bool isError) ProcessSensorStatus(string sensorName, SensorInfo info) {
        switch (info.Status) {
            case SensorStatus.Error:
                _logger.LogError("{0} is reporting an error", sensorName);
                return ($"{sensorName} is offline", true);

            case SensorStatus.Busy:
                _logger.LogWarning("{0} was busy", sensorName);
                return ($"{sensorName} is busy", false);

            case SensorStatus.Ok:
            default:
                return (string.Empty, false);
        }
    }

    private bool NotifyTestFinished(TestState state)
    {
        if (CommandLastSent.Command != Commands.Start || state != TestState.CoolingDown || TestFinished) return false;
        _logger.LogDebug("Test has finished");
        TestFinished = true;
        return true;
    }

    public void ClearData() {
        _logger.LogDebug("Data cleared in test manager");
        _dataPoints.Clear();


        var emptyState = new DeviceState {
            State = TestState.Idle,
            SetpointState = SetpointState.Reaching,
            TimeStamp = 0,
            RemainingTime = 0,
            CurrentTemp = 0,
            HighestTemp = 0,
            RateTemp = 0,
            MagnetronTemp = 0,
            Power = 0,
            PowerStep = 0,
            StateChanged = false,
            TestFinished = false,
            HasError = false,
            StatusMessage = string.Empty
        };

        var emptyData = new DeviceData {
            Data = []
        };

        _store.Update(emptyState, emptyData);
    }


    #endregion public methods

    #region private methods

    private void PostCommand(AppCommand command)
    {
        ConnectionTimer.Enabled = true;
        _logger.LogDebug("{0} command being posted", command.Command);
        CommandLastSent = command;
        CommandRequested?.Invoke(command);
    }

    private void ResetWatchDog(bool enable = false)
    {
        WatchDogTimer.Enabled = enable;
        WatchDogTimer.Stop();
        WatchDogTimer.Start();
        _logger.LogTrace("Watchdog successfully reset");
    }


    private void AutoSaveRun()
    {
        try {
            if (_dataPoints.Count == 0) return;

            var dir = AppDomain.CurrentDomain.BaseDirectory;
            var runsDir = Path.Combine(dir, "Runs");
            if (!Directory.Exists(runsDir))
            {
                Directory.CreateDirectory(runsDir);
                _logger.LogDebug("Runs directory created at {0}", runsDir);
            }

            WriteData(runsDir);
            _logger.LogDebug("Data autosaved to {0}", runsDir);
        }

        catch (Exception ex) {
            _logger.LogError("Failed to auto save data to runs directory {0}", ex);
        }
    }

    private void WriteData(string path)
    {
        try
        {
            var file = "Microwave_Data_" + DateTime.Now.ToString("MM-dd-yyyy_hh-mmtt") + ".csv";
            var filePath = Path.Combine(path, file);
            using var sw = new StreamWriter(filePath);

            sw.WriteLine(
                "Timestamp (s), Sample Temperature (C), Magnetron Temperature (C), Load (W), Heating (%), Proportional Gain, Integral Gain, Derivative Gain, Target Temp, Hold Time");
            sw.WriteLine($", , , , , {PidKp}, {PidKi}, {PidKd}, {TargetTemp}, {TargetHoldTime}");
            foreach (var data in _dataPoints)
                sw.WriteLine($"{data.TimeStamp}, {data.Temp}, {data.MagTemp}, {data.Load}, {data.Step}");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to write data to CSV {0}", ex);
        }
    }
    #endregion private methods
}