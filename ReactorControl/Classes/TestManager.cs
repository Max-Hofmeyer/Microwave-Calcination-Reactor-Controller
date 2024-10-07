using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace ReactorControl.Classes;

public class TestManager(Config config)
{
    private decimal _targetTemp, _deltaTemp, _targetHoldTime;
    private bool _disableLimits;

    private TestState _testState = TestState.Unknown;
    private Stopwatch _testTimer = new();
    private List<DataPacket> _testData = [];

    public event Action<CommandPacket>? CommandRequested;
    public event Action<CommandPacket>? CommandReceived;
    public event Action<TestState>? TestStateChanged;

    public bool TestFinished;

    public Timer WatchDogTimer = new()
    {
        Interval = 2000,
        AutoReset = false,
        Enabled = false,
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

    public decimal TargetHoldTime {
        get => _targetHoldTime;
        set {
            if (value == TargetHoldTime) return;
            _targetHoldTime = value;
        }
    }

    public bool DisableLimits
    {
        get => _disableLimits;
        set
        {
            if (value == DisableLimits) return;
            _disableLimits = value;
        }
    }

    public TestState CurrentTestState
    {
        get => _testState;
        set
        {
            _testState = value;
            TestStateChanged?.Invoke(_testState);
        }
    }

    private CommandPacket CommandLastSent { get; set; }

    private CommandPacket CommandLastReceived { get; set; }

    public void SendCheckStatusCommand()
    {
        var command = new CommandPacket { Command = ReactorCommandsEnum.Init };
        SendCommandPacket(command);
    }

    public void SendStartTestCommand()
    {
        var testSpecs = new TestSpecPacket
        {
            DeltaTemp = (double)DeltaTemp,
            TargetTemp = (double)TargetTemp,
            TargetHoldTime = (double)TargetHoldTime
        };

        var command = new CommandPacket
        {
            Command = ReactorCommandsEnum.Start,
            Checksum = (byte)ReactorCommandsEnum.Start ^ 0xFF,
            TestSpecs = testSpecs,
        };

        SendCommandPacket(command);
    }

    public void SendCooldownCommand() {
        var command = new CommandPacket { Command = ReactorCommandsEnum.Cooldown };
        SendCommandPacket(command);
    }

    public void SendStopTestCommand()
    {
        var command = new CommandPacket { Command = ReactorCommandsEnum.Stop };
        SendCommandPacket(command);
    }

    public void OnCommandReceived(CommandPacket command)
    {
        CommandLastReceived = command;
        if (command is { Command: ReactorCommandsEnum.Data, DataPacket: not null })
        {
            var data = command.DataPacket.Value;
            data.TimeStamp = _testTimer.Elapsed.TotalSeconds;
            _testData.Add(data);
        }

        UpdateTestState();
        CommandReceived?.Invoke(command);
    }

    public void OnWatchDogElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (CommandLastSent.Command == ReactorCommandsEnum.Start)
        {
            CurrentTestState = TestState.Frozen;
        }
    }

    public List<double> GetTemperatureValues()
    {
        return _testData.Select(packet => packet.TemperatureValue).ToList();
    }

    public List<double> GetTimeValues()
    {
        return _testData.Select(packet => packet.TimeStamp).ToList();
    }

    public double GetLatestPowerValue()
    {
        return _testData.LastOrDefault().WallPowerValue;
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
        sw.WriteLine(
            "Timestamp(s), Sample Temperature(℃), Wall Power Draw (W), Magnetron Power Draw (W), Reflected Power (W)");

        foreach (var data in _testData)
            sw.WriteLine(
                $"{data.TimeStamp},{data.TemperatureValue},{data.WallPowerValue}, {data.MagnetronPowerValue}, {data.ReflectedPowerValue}");
    }

    private void SendCommandPacket(CommandPacket command)
    {
        CommandLastSent = command;
        CommandRequested?.Invoke(command);
    }

    //private bool IsTestReady()
    //{
    //    return true;
    //    //if (DisableLimits) return true;
    //    //if (_targetTemp < config.MinTargetTemperature || _targetTemp > config.MaxTargetTemperature
    //    //                                              || _deltaTemp < config.MinDeltaTemperature ||
    //    //                                              _deltaTemp > config.MaxDeltaTemperature) return false;
    //    //return _targetHoldTime >= config.MinTargetHoldTime && _targetHoldTime <= config.MaxTargetHoldTime;
    //}


    //todo improve this 
    private void UpdateTestState()
    {
        var newestCommand = CommandLastReceived.Command;
        var lastSentCommand = CommandLastSent.Command;

        if (newestCommand is ReactorCommandsEnum.Data or ReactorCommandsEnum.Debug && !TestFinished)
        {
            ResetWatchDog(true);
            return;
        }

        switch (newestCommand)
        {
            case ReactorCommandsEnum.Init when
                lastSentCommand is ReactorCommandsEnum.Init:
                CurrentTestState = TestState.Idle;
                return;

            case ReactorCommandsEnum.Start when
                lastSentCommand is ReactorCommandsEnum.Start:
                _testTimer.Restart();
                ResetWatchDog(true);
                CurrentTestState = TestState.Running;
                TestFinished = false;
                return;

            case ReactorCommandsEnum.Cooldown when
                lastSentCommand is ReactorCommandsEnum.Cooldown:
                ResetWatchDog();
                CurrentTestState = TestState.CoolingDown;
                return;

            case ReactorCommandsEnum.Stop when
                lastSentCommand is ReactorCommandsEnum.Stop:
                ResetWatchDog();
                CurrentTestState = TestState.Stopped;
                _testTimer.Stop();
                return;

            case ReactorCommandsEnum.Stop when
                lastSentCommand is ReactorCommandsEnum.Start:
                ResetWatchDog();
                CurrentTestState = TestState.Stopped;
                _testTimer.Stop();
                TestFinished = true;
                return;

            case ReactorCommandsEnum.InternalError:
                ResetWatchDog();
                CurrentTestState = TestState.Unknown;
                break;

            case ReactorCommandsEnum.Data:
                break;
            case ReactorCommandsEnum.Debug:
                break;
            default:
                return;
        }
    }

    private void ResetWatchDog(bool enable = false)
    {
        WatchDogTimer.Stop();
        WatchDogTimer.Start();
        WatchDogTimer.Enabled = enable;
    }
}

public struct CommandPacket
{
    public ReactorCommandsEnum Command { get; init; }
    public byte Checksum { get; init; }
    public TestSpecPacket? TestSpecs { get; init; }
    public DataPacket? DataPacket { get; init; }
    public string? DebugMessage { get; init; }
}

public struct TestSpecPacket
{
    public double TargetTemp { get; init; }
    public double DeltaTemp { get; init; }
    public double TargetHoldTime { get; init; }
}

public struct DataPacket
{
    public double TemperatureValue { get; init; }
    public double WallPowerValue { get; init; }
    public double MagnetronPowerValue { get; init; }
    public double ReflectedPowerValue { get; init; }
    public double TimeStamp { get; set; }
}

public enum TestState
{
    Idle,
    Running,
    CoolingDown,
    Stopped,
    Unknown,
    Frozen,
}