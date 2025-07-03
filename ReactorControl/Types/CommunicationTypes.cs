using System.Runtime.InteropServices;

namespace ReactorControl.Types;


#region Command
public class RawCommand
{
    public CommandHeader Header;
    public RawTestSpecs TestSpecsRaw;
}

public enum RawCommands : byte
{
    Nop = 0x00,
    Init = 0x01,
    Start = 0X02,
    Cooldown = 0x03,
    Stop = 0x04
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CommandHeader
{
    public RawCommands Type;
    public byte Length;
    public ushort SequenceId;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RawTestSpecs
{
    public float TargetTemp;
    public float DeltaTemp;
    public float TargetHoldTime;
    public float Emissivity;
    public float PidKp;
    public float PidKi;
    public float PidKd;
    public byte TestMode;
    public byte PowerMode;
    public RawPowerStep PowerStep;
}

#endregion Command

#region Message
public class RawMessage
{
    public MessageHeader Header;
    public RawAck AckPayload;
    public RawStatus StatusPayload;
}

public enum MessageType : byte
{
    MessageAck = 0x00,
    MessageStatus = 0x01
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MessageHeader
{
    public MessageType type;
    public byte payloadLen;
    public ushort sequenceId;
}

public enum RawTestState : byte
{
    TestIdle = 0x00,
    TestRunning = 0x01,
    TestCooldown = 0x02,
    TestError = 0x03
}

public enum RawSetpointState : byte
{
    SetpointReaching = 0x00,
    SetpointHit = 0x01,
    SetpointElapsed = 0x02,
    SetpointError = 0x03
}

public enum RawPowerStep : byte
{
    Percent0 = 0X00,
    Percent10 = 0X01,
    Percent20 = 0X02,
    Percent30 = 0X03,
    Percent40 = 0X04,
    Percent50 = 0X05,
    Percent60 = 0X06,
    Percent70 = 0X07,
    Percent80 = 0X08,
    Percent90 = 0X09,
    Percent100 = 0X10,
}

public enum RawSensorStatus : byte
{
    SensorOk = 0x00,
    SensorErrorRead = 0x01,
    SensorErrorData = 0x10,
    SensorErrorWrite = 0x11,
    SensorTimeoutRead = 0x02,
    SensorTimeoutWrite = 0x12,
    SensorBusyRead = 0x03,
    SensorBusyWrite = 0x13,
    SensorNotInit = 0x14
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RawSensorInfo
{
    public float data;
    public RawSensorStatus status;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RawAck
{
    public RawCommands ackedCommand;
    public ushort ackedSequenceId;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RawStatus
{
    public RawTestState testState;
    public RawSetpointState setpointState;
    public RawPowerStep powerStep;
    public RawSensorInfo IrSensorInfo;
    public RawSensorInfo PowerSensorInfo;
    public RawSensorInfo magnetronTemp;
}

    #endregion Message