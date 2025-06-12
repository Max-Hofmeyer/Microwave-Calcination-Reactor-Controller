using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Thread = System.Threading.Thread;
using System.Timers;
using System.Text.RegularExpressions;

namespace ReactorControl.Classes;

public class ComPortManager
{
    private SerialPort? _connectedPort;
    private readonly System.Timers.Timer _connectionTimer = new (ConnectionTimeout){ AutoReset = false };
    private readonly StringBuilder _bufferStream = new();
    private readonly object _serialPortLock = new();
    private int _overrunCount = 0;
    private const int ConnectionTimeout = 3000; //3 seconds

    public ComPortManager()
    {
        _connectionTimer.Elapsed += OnConnectionTimeout;
    }

    public bool IsConnected { get; set; }

    public event Action<Models.CommandPacket>? CommandReceived;


    //returns all available COM ports the machine has a device connected to
    public static string[] GetAvailableComPorts()
    {
        return SerialPort.GetPortNames();
    }

    //attempts to connect to the reactor controller and will put it in an idle state is successful
    public void ConnectToPort(string portName)
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
            _overrunCount = 0;
            IsConnected = true;

            _connectionTimer.Interval = ConnectionTimeout;
            _connectionTimer.Start();
        }
        catch
        {
            DisconnectFromPort();
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Failed to disconnect from port: {ex.Message}");
            }
            finally
            {
                _connectedPort = null;
                IsConnected = false;
            }
        }
    }

    public void OnCommandRequested(Models.CommandPacket command)
    {
        lock (_serialPortLock)
        {
            if (_connectedPort is null || !_connectedPort.IsOpen) return;
            var serializedCommand = JsonSerializer.Serialize(command) + "\n";
            _connectedPort.Write(serializedCommand);
        }
    }

    private void OnConnectionTimeout(object? sender, ElapsedEventArgs e)
    {

        if (_connectedPort is null || !IsConnected) return;

        var cmd = new Models.CommandPacket { Command = Models.ReactorCommandsEnum.InternalError, WithErrors = true };
        CommandReceived?.Invoke(cmd);
        DisconnectFromPort();
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (_connectedPort is null || _connectedPort.BytesToRead <= 0) return;

        try
        {
            _bufferStream.Append(_connectedPort.ReadExisting());
            ProcessBufferStream();
        }
        catch(Exception ex)
        {
            Console.WriteLine($@"Error while receiving data: {ex.Message}");
        }
    }

    private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e) {
        if (_connectedPort is null || !IsConnected) return;
        if (e.EventType == SerialError.Overrun)
        {
            _overrunCount += 1;
            _connectedPort.DiscardInBuffer();
            _connectedPort.DiscardOutBuffer();

            if (_overrunCount < 2) return;

        }
        Console.WriteLine("Serial port error: " + e.EventType);


        var cmd = new Models.CommandPacket { Command = Models.ReactorCommandsEnum.InternalError, WithErrors = true };
        CommandReceived?.Invoke(cmd);
    }

    private void ProcessBufferStream()
    {
        var rawCommands = _bufferStream.ToString().Split(["\n"], StringSplitOptions.None);

        foreach (var command in rawCommands)
        {
            if (command == rawCommands.Last() || string.IsNullOrEmpty(command)) continue;

            var jsonStream = command.Trim();

            //replacing non-printable characters with empty
            //jsonStream = Regex.Replace(jsonStream, @"[^\u0020-\u007E]", string.Empty);

            //on reboot the esp32 will have this keyword, will only occur if the microcontrollers watchdog resets 
            if (jsonStream.Contains("SW_CPU_RESET"))
            {
                var stop = new Models.CommandPacket { Command = Models.ReactorCommandsEnum.Stop, WithErrors = true};
                CommandReceived?.Invoke(stop);
                _bufferStream.Clear();
            }

            try {
                var commandPacket = JsonSerializer.Deserialize<Models.CommandPacket>(jsonStream);
                CommandReceived?.Invoke(commandPacket);
                if (_connectionTimer.Enabled)
                {
                    _connectionTimer.Stop();
                }
            }
            catch(Exception e)
            {
                var error = ("Json failed, {0}, {1}", e.Message, jsonStream);
            }

        }

        _bufferStream.Clear();
        _bufferStream.Append(rawCommands.Last());
    }
}
