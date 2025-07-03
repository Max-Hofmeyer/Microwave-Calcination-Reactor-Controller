using System.IO;
using System.IO.Ports;
using System.Management;
using System.Runtime.InteropServices;
using System.Timers;
using Microsoft.Extensions.Logging;
using ReactorControl.Types;
using Timer = System.Timers.Timer;


namespace ReactorControl.Classes;

public class ComPortManager
{
    private const int ConnectionTimeout = 100000; //3 seconds
    private const byte Sof = 0x7E;
    private const byte Eof = 0x7F;
    private const byte Esc = 0x7D;
    private const byte XorMask = 0x20;
    private readonly Timer _connectionTimer = new(ConnectionTimeout) { AutoReset = false };
    private readonly ILogger<ComPortManager> _logger;
    private readonly List<byte> _receiveBuffer = [];
    private readonly object _serialPortLock = new();
    private SerialPort? _connectedPort;
    private ushort _sequenceId;

    public ComPortManager(ILogger<ComPortManager> logger)
    {
        _logger = logger;
        _connectionTimer.Elapsed += OnConnectionTimeout;
    }

    public bool IsConnected { get; set; }

    public event Action<AppMessage>? MessageReceived;
    public event Action<AppMessage>? AckReceived;


    //returns all available COM ports the machine has a device connected to
    public static string[] GetAvailableComPorts()
    {
        return SerialPort.GetPortNames().Distinct().ToArray();
    }

    //attempts to connect to the reactor controller and will put it in an idle state is successful
    public void ConnectToPort(string portName)
    {
        lock (_serialPortLock)
        {
            if (IsConnected) return;

            try
            {
                _connectedPort = new SerialPort(portName, 115200)
                {
                    ReadBufferSize = 4096,
                    WriteBufferSize = 2048
                };
                _connectedPort.Open();
                _connectedPort.DiscardInBuffer();
                _connectedPort.DiscardOutBuffer();
                _connectedPort.DataReceived += OnDataReceived;
                _connectedPort.ErrorReceived += OnErrorReceived;
                _logger.LogDebug("Connected to port {}", portName);
                _sequenceId = 0;
                IsConnected = true;
                StartPortDeletionWatch(portName);

                _connectionTimer.Interval = ConnectionTimeout;
                _connectionTimer.Start();
            }
            catch
            {
                _logger.LogError("Failed to connect to port {PortName}", portName);
                DisconnectFromPort();
            }
        }
    }

    //disconnects the active COM port 
    public void DisconnectFromPort()
    {
        lock (_serialPortLock)
        {
            if (_connectedPort is null) return;

            try
            {
                _connectionTimer.Stop();
                _connectedPort.DataReceived -= OnDataReceived;
                _connectedPort.ErrorReceived -= OnErrorReceived;
                _connectedPort.DiscardInBuffer();
                _connectedPort.DiscardOutBuffer();
                _connectedPort?.Close();
                _logger.LogDebug("Disconnected from port gracefully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to completely disconnect from port");
            }
            finally
            {
                _connectedPort = null;
                IsConnected = false;
            }
        }
    }

    public void OnCommandRequested(AppCommand command)
    {
        var serializedCommand = PrepareCommandForWrite(command);
        var framedCommand = FrameCommand(serializedCommand);
        lock (_serialPortLock)
        {
            if (_connectedPort is null || !_connectedPort.IsOpen)
            {
                _logger.LogWarning("Cannot send command {Command}, port is not connected", command.Command);
                return;
            }
            _connectedPort.Write(framedCommand, 0, framedCommand.Length);
            _logger.LogInformation("Command {Command} sent with sequence ID {SequenceId} using {len} bytes", command.Command,
                _sequenceId, framedCommand.Length);
        }
    }

    private void OnConnectionTimeout(object? sender, ElapsedEventArgs e)
    {
        _logger.LogWarning("Connection to port timed out, disconnecting");
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        lock (_serialPortLock)
        {
            if (_connectedPort is null || _connectedPort.BytesToRead <= 0)
            {
                _logger.LogWarning("Cannot read message, either port is not connected or no bytes to read");
                return;
            }
            try
            {
                var rawChunk = new byte[_connectedPort.BytesToRead];
                var read = _connectedPort.Read(rawChunk, 0, rawChunk.Length);
                if (read <= 0) return;
                _logger.LogTrace("Received {0} bytes from port {1}", read, _connectedPort.PortName);

                _connectionTimer.Stop();
                _receiveBuffer.AddRange(rawChunk.Take(read));
                TryParseBuffer();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while trying to read data");
            }
        }
    }

    //todo fix this
    private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
    {
        if (_connectedPort is null || !IsConnected) return;
        _logger.LogError("Serial port error received: {ErrorType}", e.EventType);
        //if (e.EventType == SerialError.Overrun)
        //{
        //    //_overrunCount += 1;
        //    //_connectedPort.DiscardInBuffer();
        //    //_connectedPort.DiscardOutBuffer();

        //    //if (_overrunCount < 2) return;
        //}


        ////var cmd = new CommandPacket { Command = Commands.InternalError, WithErrors = true };
        ////CommandReceived?.Invoke(cmd);
    }

    private void StartPortDeletionWatch(string portName)
    {
        var q = $@"
      SELECT * 
        FROM __InstanceDeletionEvent 
       WITHIN 2 
       WHERE TargetInstance ISA 'Win32_SerialPort'
         AND TargetInstance.DeviceID = '{portName}'";

        var watcher = new ManagementEventWatcher(new WqlEventQuery(q));
        watcher.EventArrived += (_, __) => DisconnectFromPort();
        watcher.Start();
        _logger.LogInformation("Started port deletion watch for {PortName}", portName);
    }

    private void TryParseBuffer()
    {
        while (true)
        {
            var sof = _receiveBuffer.IndexOf(Sof);
            if (sof < 0)
            {
                //no start yet
                _receiveBuffer.Clear();
                return;
            }

            var eof = _receiveBuffer.IndexOf(Eof, sof + 1);
            if (eof < 0)
            {
                //incomplete message
                if (sof > 0) _receiveBuffer.RemoveRange(0, sof);
                return;
            }

            //complete frame [sof..eof]
            var frame = _receiveBuffer.Skip(sof).Take(eof - sof + 1).ToArray();
            _receiveBuffer.RemoveRange(0, eof + 1);

            //try to decode the message
            try
            {
                var msg = BuildMessage(frame);
                _logger.LogInformation("Received message with sequence ID {1}", msg.SequenceId);
                if (msg.IsAck)
                    AckReceived?.Invoke(msg);
                else
                    MessageReceived?.Invoke(msg);
            }
            catch
            {
                _logger.LogWarning("Received invalid message, clearing buffer with {0} bytes lost", _receiveBuffer.Count());
                _receiveBuffer.Clear();
            }
        }
    }

    private AppMessage BuildMessage(byte[] rawFrame)
    {
        byte[] payload;
        try
        {
            payload = TrimAndCRC(rawFrame);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invalid CRC received. Frame: {0}", Convert.ToHexString(rawFrame));
            throw;
        }

        _logger.LogInformation("Attempting to build message from payload. Length: {0}, Frame: {1}", payload.Length, Convert.ToHexString(payload));

        if (payload.Length < Marshal.SizeOf<MessageHeader>()) {
            _logger.LogError("Frame is smaller than a MessageHeader. Expected at least {0}, got {1}.", Marshal.SizeOf<MessageHeader>(), payload.Length);
            return new AppMessage { };
        }

        try {
            //using var stream = new MemoryStream(payload);
            var reader = new BinaryReader(payload);
            var header = reader.ReadUnManaged<MessageHeader>();
            _logger.LogInformation("Received Header: Type={0}, PayloadLen={1}, SeqId={2}", header.type, header.payloadLen, header.sequenceId);

            //long remainingBytes = stream.Length - stream.Position;
            //if (header.payloadLen != remainingBytes) {
            //    _logger.LogError("Header payload length mismatch! Header says {0}, but actual remaining bytes are {1}.", header.payloadLen, remainingBytes);
            //    return new AppMessage { };
            //}

            if (header.type == MessageType.MessageAck)
            {
                var ack = reader.ReadUnManaged<RawAck>();
                return new AppMessage
                {
                    IsAck = true,
                    SequenceId = header.sequenceId,
                    AckSequenceId = ack.ackedSequenceId,
                    AckedCommand = ack.ackedCommand switch
                    {
                        RawCommands.Init => Commands.Init,
                        RawCommands.Start => Commands.Start,
                        RawCommands.Cooldown => Commands.Cooldown,
                        RawCommands.Stop => Commands.Stop,
                        _ => Commands.Init
                    }
                };
            }

            var status = reader.ReadUnManaged<RawStatus>();

            var msg = new AppMessage
            {
                SequenceId = header.sequenceId,
                State = (TestState)status.testState,
                SetpointState = (SetpointState)status.setpointState,
                PowerStep = status.powerStep switch
                {
                    RawPowerStep.Percent0 => 0,
                    RawPowerStep.Percent10 => 10,
                    RawPowerStep.Percent20 => 20,
                    RawPowerStep.Percent30 => 30,
                    RawPowerStep.Percent40 => 40,
                    RawPowerStep.Percent50 => 50,
                    RawPowerStep.Percent60 => 60,
                    RawPowerStep.Percent70 => 70,
                    RawPowerStep.Percent80 => 80,
                    RawPowerStep.Percent90 => 90,
                    _ => 100
                },

                ThermometerSample = new SensorInfo
                {
                    Data = status.IrSensorInfo.data,
                    Status = status.IrSensorInfo.status switch
                    {
                        RawSensorStatus.SensorOk => SensorStatus.Ok,
                        RawSensorStatus.SensorBusyRead => SensorStatus.Busy,
                        RawSensorStatus.SensorBusyWrite => SensorStatus.Busy,
                        _ => SensorStatus.Error
                    }
                },

                ThermometerMagnetron = new SensorInfo
                {
                    Data = status.magnetronTemp.data,
                    Status = status.magnetronTemp.status switch
                    {
                        RawSensorStatus.SensorOk => SensorStatus.Ok,
                        RawSensorStatus.SensorBusyRead => SensorStatus.Busy,
                        RawSensorStatus.SensorBusyWrite => SensorStatus.Busy,
                        _ => SensorStatus.Error
                    }
                },

                PowerMeter = new SensorInfo
                {
                    Data = status.PowerSensorInfo.data,
                    Status = status.PowerSensorInfo.status switch
                    {
                        RawSensorStatus.SensorOk => SensorStatus.Ok,
                        RawSensorStatus.SensorBusyRead => SensorStatus.Busy,
                        RawSensorStatus.SensorBusyWrite => SensorStatus.Busy,
                        _ => SensorStatus.Error
                    }
                }
            };
            return msg;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical failure when trying to build message: {0}", Convert.ToHexString(payload));
        }

        return new AppMessage {};

    }

    private byte[] PrepareCommandForWrite(AppCommand command)
    {
        var header = new CommandHeader
        {
            Type = command.Command switch
            {
                Commands.Init => RawCommands.Init,
                Commands.Start => RawCommands.Start,
                Commands.Cooldown => RawCommands.Cooldown,
                Commands.Stop => RawCommands.Stop,
                _ => RawCommands.Nop
            },
            Length = command.Command switch
            {
                Commands.Start => (byte)Marshal.SizeOf<RawTestSpecs>(),
                _ => 0
            },

            SequenceId = ++_sequenceId
        };

        var rawLen = Marshal.SizeOf<CommandHeader>() + header.Length;
        var rawData = new byte[rawLen + sizeof(ushort)];

        var offset = 0;
        Buffer.BlockCopy(StructToBytes(header), 0, rawData, offset,
            Marshal.SizeOf<CommandHeader>());
        offset += Marshal.SizeOf<CommandHeader>();
        
        if (header.Length > 0)
        {
            var convertedSpecs = new RawTestSpecs
            {
                TargetTemp = command.Specs.TargetTemp,
                DeltaTemp = command.Specs.DeltaTemp,
                TargetHoldTime = command.Specs.TargetHoldTime,
                Emissivity = command.Specs.Emissivity,
                PidKp = command.Specs.PidKp,
                PidKi = command.Specs.PidKi,
                PidKd = command.Specs.PidKd,
                TestMode = command.Specs.TestMode ? (byte)1 : (byte)0,
                PowerMode = command.Specs.PowerMode ? (byte)1 : (byte)0,
                PowerStep = ConvertToRawPowerStep(command.Specs.PowerModeStep)
            };

            Buffer.BlockCopy(StructToBytes(convertedSpecs), 0, rawData, offset, header.Length);
            offset += header.Length;
        }

        var crc = CRC16_CCITT(rawData, rawLen);
        Array.Copy(BitConverter.GetBytes(crc), 0, rawData, offset, 2);

        return rawData;
    }

    private static ushort CRC16_CCITT(byte[] data, int length)
    {
        ushort crc = 0xFFFF;
        for (var i = 0; i < length; i++)
        {
            crc ^= (ushort)(data[i] << 8);
            for (var j = 0; j < 8; j++)
                if ((crc & 0x8000) != 0)
                    crc = (ushort)((crc << 1) ^ 0x1021);
                else
                    crc <<= 1;
        }

        return crc;
    }

    private static byte[] StructToBytes<T>(T str) where T : struct
    {
        var size = Marshal.SizeOf<T>();
        var arr = new byte[size];
        var ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);

        return arr;
    }

    private static byte[] FrameCommand(byte[] raw)
    {
        var framed = new List<byte> { Sof };

        foreach (var b in raw)
            if (b is Sof or Eof or Esc)
            {
                framed.Add(Esc);
                framed.Add((byte)(b ^ XorMask));
            }
            else
            {
                framed.Add(b);
            }

        framed.Add(Eof);
        return framed.ToArray();
    }

    private byte[] TrimAndCRC(byte[] rawFrame)
    {
        var buf = new List<byte>(rawFrame.Length);
        var escaped = false;

        // skip first (SOF) and last (EOF)
        for (var i = 1; i < rawFrame.Length - 1; i++)
        {
            var b = rawFrame[i];
            if (escaped)
            {
                buf.Add((byte)(b ^ XorMask));
                escaped = false;
            }
            else if (b == Esc)
            {
                escaped = true;
            }
            else
            {
                buf.Add(b);
            }
        }

        // CRC check:
        var dataLen = buf.Count - 2;
        var recvCrc = BitConverter.ToUInt16(buf.ToArray(), dataLen);
        var calcCrc = CRC16_CCITT(buf.ToArray(), dataLen);
        if (recvCrc != calcCrc)
            throw new InvalidOperationException("CRC mismatch");

        return buf.Take(dataLen).ToArray();
    }

    public static RawPowerStep ConvertToRawPowerStep(PowerStep powerStep) {
        switch (powerStep) {
            case PowerStep.Power10: return RawPowerStep.Percent10;
            case PowerStep.Power20: return RawPowerStep.Percent20;
            case PowerStep.Power30: return RawPowerStep.Percent30;
            case PowerStep.Power40: return RawPowerStep.Percent40;
            case PowerStep.Power50: return RawPowerStep.Percent50;
            case PowerStep.Power60: return RawPowerStep.Percent60;
            case PowerStep.Power70: return RawPowerStep.Percent70;
            case PowerStep.Power80: return RawPowerStep.Percent80;
            case PowerStep.Power90: return RawPowerStep.Percent90;
            case PowerStep.Power100: return RawPowerStep.Percent100;
            default:
                throw new ArgumentOutOfRangeException(nameof(powerStep), powerStep, "Unknown PowerStep value for conversion.");
        }
    }
}