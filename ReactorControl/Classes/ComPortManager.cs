using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Thread = System.Threading.Thread;

namespace ReactorControl.Classes;

public class ComPortManager
{
    private SerialPort? _connectedPort;
    private StringBuilder _bufferStream = new();

    public bool IsConnected;
    public event Action<CommandPacket>? CommandReceived;

    //returns all available COM ports the machine has a device connected to
    public static string[] GetAvailableComPorts()
    {
        return SerialPort.GetPortNames();
    }

    //Attempts to connect to the reactor controller, does the handshaking and will put the controller in an idle state
    public void ConnectToPort(string portName)
    {
        if (IsConnected)
        {
            throw new InvalidOperationException("COM port is already connected");
        }

        _connectedPort = new SerialPort(portName, 115200);

        try
        {
            _connectedPort.Open();
            _connectedPort.DataReceived += OnDataReceived;
        }
        catch
        {
            _connectedPort.DataReceived -= OnDataReceived;
            DisconnectFromPort();
            throw;
        }
    }

    //disconnects the active COM port 
    public void DisconnectFromPort()
    {
        if (_connectedPort is null) return;

        //todo make this more managed
        //OnCommandRequested(new CommandPacket { Command = ReactorCommandsEnum.Stop, Checksum = (byte)ReactorCommandsEnum.Stop ^ 0xFF });
        _connectedPort.DataReceived -= OnDataReceived;
        _connectedPort.Close();
        
        _connectedPort = null;
        IsConnected = false;
    }

    public void HandleFrozenPort()
    {
        _bufferStream.Clear();
        var portInfo = _connectedPort;
        DisconnectFromPort();
        
        _connectedPort = portInfo;
        if (portInfo is null) return;
        ConnectToPort(portInfo.PortName);
    }

    public void OnCommandRequested(CommandPacket command)
    {
        if (_connectedPort is null || !_connectedPort.IsOpen) return;

        var serializedCommand = JsonSerializer.Serialize(command) + "\n";
        _connectedPort.Write(serializedCommand);
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            if (_connectedPort is null || _connectedPort.BytesToRead <= 0) return;
            _bufferStream.Append(_connectedPort.ReadExisting());
            ProcessBufferStream();
        }
        catch
        {
            //todo if data is being sent while port is closed/reopen this will be thrown
        }
    }
    private void ProcessBufferStream()
    {
        var rawCommands = _bufferStream.ToString().Split(["\n"], StringSplitOptions.None);

        foreach (var command in rawCommands)
        {
            if (command == rawCommands.Last() || string.IsNullOrEmpty(command)) continue;

            var jsonStream = command.Trim();

            try {
                var commandPacket = JsonSerializer.Deserialize<CommandPacket>(jsonStream);
                CommandReceived?.Invoke(commandPacket);
            }
            catch {
                string what = jsonStream;
                //todo add logging
                //CommandReceived?.Invoke(new CommandPacket { Command = ReactorCommandsEnum.Debug });
            }

        }

        _bufferStream.Clear();
        _bufferStream.Append(rawCommands.Last());
    }
}

public enum ReactorCommandsEnum
{
    Init,
    Start,
    Stop,
    Cooldown,
    Data,
    Debug,
    InternalError,
}
