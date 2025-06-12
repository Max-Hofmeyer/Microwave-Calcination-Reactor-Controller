using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ReactorControl.Classes;

public class TestManager
{
    private bool _powerMode, _testMode;
    private decimal _targetTemp, _deltaTemp, _targetHoldTime, _emissivity;
    private readonly List<Models.DataPacket> _testData = [];
    private readonly object _dataLock = new object();

    private bool _testFinished;
    private Models.TestState _testState = Models.TestState.Unknown;
    private readonly Stopwatch _testTimer = new();
    private Models.CommandPacket CommandLastSent { get; set; }

    public event Action<Models.CommandPacket>? CommandRequested;
    public event Action<Models.CommandPacket>? CommandReceived;
    public event Action<Models.TestState>? TestStateChanged;
    public Models.CommandPacket CommandLastReceived { get; set; }

    public Timer WatchDogTimer = new()
    {
        Interval = 3000,
        AutoReset = false,
        Enabled = false
    };

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
    public decimal Emissivity {
        get => _emissivity;
        set {
            if (value == Emissivity) return;
            _emissivity = value;
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
    public bool TestMode {
        get => _testMode;
        set {
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

    public Models.TestState CurrentTestState
    {
        get => _testState;
        set
        {
            if (value == _testState) return;
            _testState = value;
            TestStateChanged?.Invoke(_testState);
        }
    }

    #region public methods
    public void SendCheckStatusCommand()
    {
        var command = new Models.CommandPacket { Command = Models.ReactorCommandsEnum.Init };
        SendCommandPacket(command);
    }

    public void SendStartTestCommand()
    {
        _testFinished = false;
        var deltaTemp = (double)DeltaTemp;
        var targetTemp = (double)TargetTemp;
        var targetHoldTime = (double)TargetHoldTime;
        var emissivity = (double)Emissivity;
        if (_powerMode)
        {
            deltaTemp = 0.0;
            targetTemp = 0.0;
        }

        var testSpecs = new Models.TestSpecPacket
        {
            DeltaTemp = deltaTemp,
            TargetTemp = targetTemp,
            TargetHoldTime = targetHoldTime,
            Emissivity = emissivity,
            TestMode = _testMode
        };

        var command = new Models.CommandPacket
        {
            Command = Models.ReactorCommandsEnum.Start,
            TestSpecs = testSpecs
        };

        SendCommandPacket(command);
    }

    public void SubmitCommand(Models.ReactorCommandsEnum command)
    {
        var newCommand = new Models.CommandPacket { Command = command};
        SendCommandPacket(newCommand);
    }

    public void OnCommandReceived(Models.CommandPacket command)
    {
        CommandLastReceived = command;
        if (command is { Data: not null, Command: Models.ReactorCommandsEnum.Data or Models.ReactorCommandsEnum.Cooldown }) {
            var data = command.Data.Value;
            data.TimeStamp = _testTimer.Elapsed.TotalSeconds;
            _testData.Add(data);
        }

        UpdateTestState();
        CommandReceived?.Invoke(command);
    }

    public void OnWatchDogElapsed(object? sender, ElapsedEventArgs e)
    {
        if (CommandLastSent.Command == Models.ReactorCommandsEnum.Start) CurrentTestState = Models.TestState.Frozen;
    }

    public List<double> GetTemperatureValues()
    {
        return _testData
            .Where(packet => packet.TemperatureValue.HasValue) // 1. Filter packets that have a temp value
            .Select(packet => packet.TemperatureValue.Value)  // 2. Select the non-null value
            .ToList();
        //return _testData
        //    .Select(packet => packet.TemperatureValue)
        //    .Where(temp => temp.HasValue)
        //    .Select(temp => temp!.Value)
        //    .ToList();
    }
    public List<double> GetTimeValues() {
        return _testData.Select(packet => packet.TimeStamp).ToList();
    }

    public double GetLatestPowerValue() {
        if (_testData.Count == 0) return 0;
        return _testData.LastOrDefault().WallPowerValue ?? 0;
    }

    public List<double> GetTemperatureInterpolatedValues()
    {
        var tempVals = new List<double>();
        if (_testData.Count == 0) return tempVals;

        for (var i = 0; i < _testData.Count - 1; i++)
            if (_testData[i].TemperatureValue.HasValue && _testData[i + 1].TemperatureValue.HasValue)
            {
                var mid = (_testData[i].TemperatureValue!.Value + _testData[i + 1].TemperatureValue!.Value) / 2;
                tempVals.Add(mid);
            }

        tempVals.Add(_testData.Last().TemperatureValue ?? 0);
        return tempVals;
    }

    public List<double> GetTimeInterpolatedValues()
    {
        var tempVals = new List<double>();
        if (_testData.Count == 0) return tempVals;

        for (var i = 0; i < _testData.Count - 1; i++)
        {
            var mid = (_testData[i].TimeStamp + _testData[i + 1].TimeStamp) / 2;
            tempVals.Add(mid);
        }

        tempVals.Add(_testData.Last().TimeStamp);
        return tempVals;
    }

    public void ClearData()
    {
        _testData.Clear();
    }

    public void ExportDataToCSV(string? directory)
    {
        directory ??= Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        if (_testData.Count != GetTimeValues().Count)
            throw new InvalidOperationException("Mismatch count of data points and timestamps, cannot export");
        var file = "Microwave_Data_" + DateTime.Now.ToString("MM-dd-yyyy_hh-mmtt") + ".csv";
        var filePath = Path.Combine(directory, file);

        using var sw = new StreamWriter(filePath);
        sw.WriteLine("Timestamp(s), Sample Temperature(℃), Wall Power Draw (W)");

        foreach (var data in _testData)
            sw.WriteLine($"{data.TimeStamp},{data.TemperatureValue},{data.WallPowerValue}");
    }

    #endregion public methods

    #region private methods

    private void SendCommandPacket(Models.CommandPacket command)
    {
        CommandLastSent = command;
        CommandRequested?.Invoke(command);
    }

    private void UpdateTestState()
    {
        var newestCommand = CommandLastReceived.Command;

        if (newestCommand is Models.ReactorCommandsEnum.Data or Models.ReactorCommandsEnum.Debug && !TestFinished)
        {
            ResetWatchDog(true);
            return;
        }

        switch (newestCommand)
        {
            case Models.ReactorCommandsEnum.Init:
                HandleInitCommand();
                break;
            case Models.ReactorCommandsEnum.Start:
                HandleStartCommand();
                break;
            case Models.ReactorCommandsEnum.Cooldown:
                HandleCooldownCommand();
                break;
            case Models.ReactorCommandsEnum.Stop:
                HandleStopCommand();
                break;
            case Models.ReactorCommandsEnum.InternalError:
                HandleInternalErrorCommand();
                break;
            case Models.ReactorCommandsEnum.BadCommand:
            case Models.ReactorCommandsEnum.Data:
            case Models.ReactorCommandsEnum.Debug:
                break;
            default:
                return;
        }
    }

    private void HandleInitCommand()
    {
        CurrentTestState = Models.TestState.Idle;
    }

    private void HandleStartCommand()
    {
        if (CommandLastSent.Command != Models.ReactorCommandsEnum.Start)
        {
            HandleInternalErrorCommand();
            return;
        }

        _testTimer.Restart();
        ResetWatchDog(true);
        CurrentTestState = Models.TestState.Running;
        TestFinished = false;
    }

    private void HandleCooldownCommand()
    {
        //if app didn't send the cooldown command, test has finished
        if (CommandLastSent.Command != Models.ReactorCommandsEnum.Cooldown)
        {
            TestFinished = true;
        }

        ResetWatchDog();
        CurrentTestState = Models.TestState.CoolingDown;
    }

    private void HandleStopCommand()
    {
        ResetWatchDog();
        CurrentTestState = Models.TestState.Stopped;
        _testTimer.Stop();
    }

    private void HandleInternalErrorCommand()
    {
        ResetWatchDog();
        CurrentTestState = Models.TestState.Disconnected;
        _testTimer.Stop();
        TestFinished = true;
    }

    private void ResetWatchDog(bool enable = false)
    {
        //WatchDogTimer.Stop();
        //WatchDogTimer.Start();
        WatchDogTimer.Enabled = enable;
        WatchDogTimer.Stop();
        WatchDogTimer.Start();
        //WatchDogTimer.Enabled = false;
    }

    #endregion private methods
}