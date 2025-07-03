using System.Text.Json.Serialization;

namespace ReactorControl.Types;


#region Commands

public class AppCommand
{
    public Commands Command { get; init; }
    public TestSpecs Specs { get; init; } 
}

public enum Commands
{
    Init,
    Start,
    Cooldown,
    Stop,
}

public struct TestSpecs
{
    public float TargetTemp { get; init; }
    public float DeltaTemp { get; init; }
    public float TargetHoldTime { get; init; }
    public float Emissivity { get; init; }
    public float PidKp { get; init; }
    public float PidKi { get; init; }
    public float PidKd { get; init; }
    public bool TestMode { get; init; } 
    public bool PowerMode { get; init; }
    public PowerStep PowerModeStep { get; init; }

}

#endregion Commands

#region Messages

public class AppMessage
{
    public TestState State { get; init; }
    public int SequenceId { get; init; }
    public bool IsAck { get; init; }
    public int AckSequenceId { get; init; }
    public Commands AckedCommand { get; init; }
    public SetpointState SetpointState { get; init; }
    public SensorInfo ThermometerSample { get; init; }
    public SensorInfo ThermometerMagnetron { get; init; }
    public SensorInfo PowerMeter { get; init; }
    public int PowerStep { get; init; }
}

public struct SensorInfo
{
    public SensorStatus Status { get; init; }
    public float Data { get; init; }
}

public enum SensorStatus
{
    Ok,
    Busy,
    Error,
}

public enum PowerStep
{
    Power10,
    Power20,
    Power30,
    Power40,
    Power50,
    Power60,
    Power70,
    Power80,
    Power90,
    Power100
}

public enum TestState
{
    Idle,
    Running,
    CoolingDown,
    Error,
}

public enum SetpointState
{
    Reaching, 
    Hit,
    Elapsed,
    Error
}

#endregion Messages

public class DeviceState
{
    public TestState State { get; init; }
    public SetpointState SetpointState { get; init; }
    public bool IsAck { get; init; }
    public bool GoodAck { get; init; }
    public bool SetpointHit { get; init; }
    public Commands LastCommand { get; init; }
    public double TimeStamp { get; init; }
    public double RemainingTime { get; init; }
    public float CurrentTemp { get; init; }
    public float HighestTemp { get; init; }
    public float RateTemp { get; init; }
    public float MagnetronTemp { get; init; }
    public float Power { get; init; }
    public int PowerStep { get; init; }
    public bool StateChanged { get; init; }
    public bool TestFinished { get; init;  }
    public bool HasError { get; init; }
    public string StatusMessage { get; set; }
}

public class DeviceData
{
    public DataPoint[] Data { get; init; }
}

public class DataPoint
{
    public double TimeStamp { get; set; }
    public float Temp { get; set; }
    public float Load { get; set; }
    public int Step { get; set; }
    public float MagTemp { get; set; }
}
