using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ReactorControl.Classes;
using ReactorControl.Types;
using ScottPlot;
using static System.Windows.Forms.AxHost;
using Color = System.Drawing.Color;
using Timer = System.Windows.Forms.Timer;

namespace ReactorControl;

public partial class MainForm : Form
{
    private readonly ILogger<MainForm> _logger;
    private readonly StateStore _store;
    private readonly ComPortManager _portManager;
    private readonly TestManager _testManager;
    private readonly Timer _pollTimer;
    //private readonly Stopwatch _inputTimeout = new();
    private string[] _lastPorts = [];
    private DeviceState _previousState;
    private bool _setpointHit = false;
    private double _setpoint_x = 0.0;
    private double _setpoint_y = 0.0;

    public MainForm(ComPortManager comPortManager, TestManager testManager, StateStore store, ILogger<MainForm> logger) {
        InitializeComponent();
        _store = store;
        _portManager = comPortManager;
        _testManager = testManager;
        _logger = logger;
        _pollTimer = new Timer { Interval = 25 };
        _pollTimer.Tick += UpdateUI;
    }

    //Executes when the app is finished loading. Grabs COM ports and sets up graph
    private void MainForm_Load(object sender, EventArgs e) {
        _pollTimer.Start();
        LockUI();
        ConnectionUnlockUI();

        _lastPorts = ComPortManager.GetAvailableComPorts();
        ComPortToolstripComboBox.ComboBox.DataSource = _lastPorts;

        _testManager.TargetTemp = TargetTempInput.Value;
        _testManager.DeltaTemp = DeltaTInput.Value;
        _testManager.TargetHoldTime = TargetHoldTimeInput.Value;
        _testManager.Emissivity = emissivityInput.Value;
        _testManager.PidKp = pidKpBox.Value;
        _testManager.PidKi = pidKiBox.Value;
        _testManager.PidKd = pidKdBox.Value;
        _testManager.TestMode = false;
        _testManager.PowerMode = false;

        TemperaturePlot.Plot.XLabel("Time (s)");
        TemperaturePlot.Plot.Axes.Left.Label.Text = "Temperature (°C)";
        TemperaturePlot.Plot.Axes.Right.Label.Text = "Load (W)";

        TemperaturePlot.Plot.Axes.Left.Label.ForeColor = ScottPlot.Colors.Blue;
        TemperaturePlot.Plot.Axes.Left.FrameLineStyle.Color = ScottPlot.Colors.Blue;
        TemperaturePlot.Plot.Axes.Left.FrameLineStyle.Width = 3;

        TemperaturePlot.Plot.Axes.Right.Label.ForeColor = ScottPlot.Colors.Red;
        TemperaturePlot.Plot.Axes.Right.FrameLineStyle.Color = ScottPlot.Colors.Red;
        TemperaturePlot.Plot.Axes.Right.FrameLineStyle.Width = 3;
        TemperaturePlot.Plot.Axes.AutoScale();
        TemperaturePlot.Plot.Axes.ContinuouslyAutoscale = true;

        pidKdBox.Visible = false;
        pidKpBox.Visible = false;
        pidKiBox.Visible = false;
        setPointLabel.Visible = false;
        setPointRemainingTimeBox.Visible = false;
        powerModeStepComboBox.Visible = false;
        label4.Visible = false;
        label12.Visible = false;
        label13.Visible = false;
        label14.Visible = false;

        powerModeStepComboBox.DataSource = Enum.GetValues(typeof(PowerStep));
        powerModeStepComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
    }

    private void MainForm_Closing(object sender, FormClosingEventArgs e) {
        TryStopTest();
        _portManager.DisconnectFromPort();
    }

    #region Button Click Events

    private void ConnectCOMButton_Click(object sender, EventArgs e) {
        if (ComPortToolstripComboBox.SelectedItem is not string portName) return;
        _logger.LogDebug("Attempting to connect to COM port {port}", portName);
        MessageBoxLog(LogLevel.Information, "Connecting...");
        TryConnectToPort(portName);
    }

    private void StartTestButton_Click(object sender, EventArgs e) {
        MessageBoxLog(LogLevel.Debug, "Start command sent");
        TryStartTest();
    }

    private void CoolDownButton_Click(object sender, EventArgs e) {
        MessageBoxLog(LogLevel.Debug, "Cooldown command sent");
        TryStartCooldown();
    }

    private void StopTestButton_Click(object sender, EventArgs e) {
        MessageBoxLog(LogLevel.Debug, "Stop command sent");
        TryStopTest();
    }

    private void AutoScaleChartButton_Click(object sender, EventArgs e) {
        TemperaturePlot.Plot.Axes.AutoScale();
        TemperaturePlot.Refresh();
    }

    private void DisconnectCOMButton_Click(object sender, EventArgs e) {
        _logger.LogDebug("Attempting to disconnect from port");
        TryDisconnectFromPort();
    }

    private void ClearMessageButton_Click(object sender, EventArgs e) {
        MessageBox.Text = string.Empty;
    }

    private void timerModeToolStripMenuItem_CheckedChanged_1(object sender, EventArgs e) {
        TogglePowerMode();
    }

    private void chartClearItem_Click(object sender, EventArgs e) {
        var result = System.Windows.Forms.MessageBox.Show(
            "Are you sure you want to clear the chart? All data from this session will be lost.",
            "Clear Chart?",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );

        if (result != DialogResult.Yes) return;

        ResetData();
    }

    private void autoScaleOnItem_Click(object sender, EventArgs e) {
        bool autoscaled = TemperaturePlot.Plot.Axes.ContinuouslyAutoscale;
        TemperaturePlot.Plot.Axes.ContinuouslyAutoscale = !autoscaled;
        autoScaleOnItem.Checked = !autoscaled;
        MessageBoxLog(LogLevel.Information, autoscaled == false ? "Always autoscale enabled" : "Always autoscale disabled");
    }

    private void exportDataToolStripMenuItem_Click(object sender, EventArgs e) {
        try {
            _testManager.ExportDataToCSV();
            MessageBoxLog(LogLevel.Information, "Data saved to desktop");
        }
        catch (Exception ex) {
            MessageBoxLog(LogLevel.Error, "Failed to export data: " + ex.Message);
        }
    }

    private void testModeToolStripMenuItem_Click(object sender, EventArgs e) {
        bool testMode = _testManager.TestMode;

        _testManager.TestMode = !testMode;
        testModeToolStripMenuItem.Checked = !testMode;
    }

    #endregion Button Click Events


    #region UI Helpers

    private void DeltaTInput_ValueChanged(object sender, EventArgs e) {
        _testManager.DeltaTemp = DeltaTInput.Value;
    }

    private void TargetTempInput_ValueChanged(object sender, EventArgs e) {
        _testManager.TargetTemp = TargetTempInput.Value;
    }

    private void TargetHoldTimeInput_ValueChanged(object sender, EventArgs e) {
        _testManager.TargetHoldTime = TargetHoldTimeInput.Value;
    }

    private void emissivityInput_ValueChanged(object sender, EventArgs e) {
        _testManager.Emissivity = emissivityInput.Value;
    }

    private void TryConnectToPort(string portName) {
        try {
            MessageBoxLog(LogLevel.Debug, "Connecting to " + portName);

            _portManager.ConnectToPort(portName);
            _testManager.SendCheckStatusCommand();
            ShowCommandStateChange(new DeviceState { GoodAck = true, LastCommand = Commands.Init });

            ComPortToolstripComboBox.Enabled = false;
            ConnectToolstripButton.Enabled = false;
        }
        catch (Exception ex) {
            ConnectToolstripButton.Enabled = true;
            ComPortToolstripComboBox.Enabled = true;
            MessageBoxLog(LogLevel.Error, "Failed to connect to " + portName + ": " + ex.Message);
        }
    }

    private void TryDisconnectFromPort() {
        try {
            _portManager.DisconnectFromPort();
            if (_portManager.IsConnected) return;

            SetUI(TestState.Idle, true);
            MessageBoxLog(LogLevel.Information, "Disconnected from " + ComPortToolstripComboBox.SelectedItem);
        }
        catch (Exception ex) {
            MessageBoxLog(LogLevel.Error, "Failed to disconnect from:" + ex.Message);
        }
    }

    private void TryStartTest() {
        try {

            _testManager.TargetTemp = TargetTempInput.Value;
            _testManager.DeltaTemp = DeltaTInput.Value;
            _testManager.TargetHoldTime = TargetHoldTimeInput.Value;
            _testManager.PidKp = pidKpBox.Value;
            _testManager.PidKi = pidKiBox.Value;
            _testManager.PidKd = pidKdBox.Value;
            _testManager.PowerMode = timerModeToolStripMenuItem.Checked;
            _testManager.PowerModeStep = (PowerStep)powerModeStepComboBox.SelectedItem!;
            _testManager.TestMode = testModeToolStripMenuItem.Checked;
            _setpointHit = false;
            _testManager.SendStartTestCommand();
            ShowCommandStateChange(new DeviceState { GoodAck = true, LastCommand = Commands.Start });
            ResetData();
        }
        catch (Exception ex) {
            MessageBoxLog(LogLevel.Error, "Start command failed: " + ex.Message);
        }
    }

    private void TryStartCooldown() {
        try {

            _testManager.SendCommand(Commands.Cooldown);
            ShowCommandStateChange(new DeviceState { GoodAck = true, LastCommand = Commands.Cooldown });
        }
        catch (Exception ex) {
            MessageBoxLog(LogLevel.Error, "Cooldown command failed: " + ex.Message);
        }
    }

    private void TryStopTest() {
        try {

            _testManager.SendCommand(Commands.Stop);
            ShowCommandStateChange(new DeviceState { GoodAck = true, LastCommand = Commands.Stop });
        }
        catch (Exception ex) {
            MessageBoxLog(LogLevel.Error, "Stop command failed: " + ex.Message);
        }
    }

    private void UpdateUI(object? sender, EventArgs eventArgs) {
        RefreshComPorts();
        var (status, data) = _store.Snapshot();

        //if (_inputTimeout is { IsRunning: true, Elapsed.Seconds: > 3 }) {
        //    ShowCommandStateChange(status);
        //}

        if (status == _previousState || !_portManager.IsConnected) return;
        var points = data.Data ?? [];


        if (!string.IsNullOrEmpty(status.StatusMessage)) {
            MessageBoxLog(status.HasError ? LogLevel.Error : LogLevel.Information, status.StatusMessage);
        }

        if (status.SetpointHit) {
            if (!_setpointHit)
            {
                var hit = points.Last().TimeStamp;
                _setpointHit = true;
                setPointLabel.Visible = true;
                setPointRemainingTimeBox.Visible = true;
                _setpoint_x = status.CurrentTemp;
                _setpoint_y = points.Last().TimeStamp;

                TemperaturePlot.Refresh();
                MessageBoxLog(LogLevel.Information, "Setpoint hit");
            }
            
        }

        ShowTestElapsed(status.TestFinished);

        HighestTempBox.Text = $@"{status.HighestTemp:0.0}";
        RateOfChangeBox.Text = $@"{status.RateTemp:0.0}";
        CurrentTempBox.Text = $@"{status.CurrentTemp:0.0}";
        PowerDrawTextBox.Text = $@"{status.Power:0.0}";
        MagnetronTempBox.Text = $@"{status.MagnetronTemp:0.0}";
        PowerStepBox.Text = $@"{status.PowerStep}";
        setPointRemainingTimeBox.Text = $@"{status.RemainingTime}";
        _previousState = status;

        if (points.Length <= 0) {
            return;
        }

        TemperaturePlot.Plot.Clear();
        var timeStamps = points.Select(t => t.TimeStamp).ToArray();
        var tempData = points.Select(t => t.Temp).ToArray();
        var powerData = points.Select(t => t.Load).ToArray();
        var stepData = points.Select(t => t.Step).ToArray();

        var tempScatter = TemperaturePlot.Plot.Add.Scatter(timeStamps, tempData);
        var powerScatter = TemperaturePlot.Plot.Add.Scatter(timeStamps, powerData);
        tempScatter.Axes.YAxis = TemperaturePlot.Plot.Axes.Left;
        powerScatter.Axes.YAxis = TemperaturePlot.Plot.Axes.Right;
        tempScatter.Color = ScottPlot.Colors.Blue;
        powerScatter.Color = ScottPlot.Colors.Red;
       

        if (_setpointHit)
        {
            TemperaturePlot.Plot.Add.VerticalLine(_setpoint_y);
            AutoScaleChartButton.Text = _setpoint_y.ToString();
           TemperaturePlot.Plot.Add.HorizontalLine(_setpoint_x);
        }
        TemperaturePlot.Refresh();
    }

    private void RefreshComPorts() {
        var currentPorts = ComPortManager.GetAvailableComPorts();
        if (!_portManager.IsConnected && !ConnectToolstripButton.Enabled) {
            MessageBoxLog(LogLevel.Information, "Connection lost");
            SetUI(TestState.Idle, true);
        }
        if (currentPorts.SequenceEqual(_lastPorts)) return;

        var box = ComPortToolstripComboBox.ComboBox;

        box.BeginUpdate();
        box.DataSource = currentPorts;
        if (currentPorts.Length <= 0) {
            box.Text = string.Empty;
        }
        box.EndUpdate();
        _lastPorts = currentPorts;

    }

    //input from the user changed the state of the mcu
    private void ShowCommandStateChange(DeviceState message) {

        //_inputTimeout.Reset();
        if (message.GoodAck) {
            switch (message.LastCommand) {
                case Commands.Init:
                    string msg = "Connected to " + ComPortToolstripComboBox.ComboBox.Text;
                    MessageBoxLog(LogLevel.Information, msg);
                    SetUI(TestState.Idle);
                    ConnectToolstripButton.Enabled = false;
                    ComPortToolstripComboBox.Enabled = false;
                    break;

                case Commands.Start:
                    MessageBoxLog(LogLevel.Information, "Running");
                    LockUI();
                    SetUI(TestState.Running);
                    break;

                case Commands.Cooldown:
                    MessageBoxLog(LogLevel.Information, "Cooling down");
                    SetUI(TestState.CoolingDown);
                    CoolDownButton.Enabled = false;
                    break;

                case Commands.Stop:
                    MessageBoxLog(LogLevel.Information, "Stopped");
                    _setpointHit = false;
                    _setpoint_x = 0;
                    _setpoint_y = 0;
                    SetUI(TestState.Idle);
                    UnlockUI();
                    break;
            }
        }
        else {
            SetUI(TestState.Idle, true);
            ConnectionUnlockUI();

            switch (message.LastCommand) {
                case Commands.Init:
                    MessageBoxLog(LogLevel.Error, "Connection timed out. Try again");
                    break;

                case Commands.Start:
                    MessageBoxLog(LogLevel.Error, "Start command failed.");
                    break;

                case Commands.Cooldown:
                    MessageBoxLog(LogLevel.Error, "Cooldown command failed. Halting test");
                    break;

                case Commands.Stop:
                    MessageBoxLog(LogLevel.Error, "Stop command failed. Attempting to halt test");
                    break;
            }
        }
    }

    //mcu changed state, show message
    private void ShowTestElapsed(bool testFinished) {
        if (!testFinished) return;

        SetUI(TestState.CoolingDown);
        MessageBoxLog(LogLevel.Information, "Setpoint elapsed. Cooling down");

    }

    private void LockUI() {
        TargetTempInput.Enabled = false;
        DeltaTInput.Enabled = false;
        StartTestButton.Enabled = false;
        ComPortToolstripComboBox.Enabled = false;
        ConnectToolstripButton.Enabled = false;
        DisconnectCOMButton.Enabled = false;
        TargetHoldTimeInput.Enabled = false;
        timerModeToolStripMenuItem.Enabled = false;
        emissivityInput.Enabled = false;
        pidKdBox.Enabled = false;
        pidKiBox.Enabled = false;
        pidKpBox.Enabled = false;
        powerModeStepComboBox.Enabled = false;

        StopTestButton.Enabled = true;
        CoolDownButton.Enabled = true;
    }

    private void UnlockUI() {
        TargetTempInput.Enabled = true;
        DeltaTInput.Enabled = true;
        StartTestButton.Enabled = true;
        ComPortToolstripComboBox.Enabled = true;
        DisconnectCOMButton.Enabled = true;
        TargetHoldTimeInput.Enabled = true;
        timerModeToolStripMenuItem.Enabled = true;
        emissivityInput.Enabled = true;
        pidKdBox.Enabled = true;
        pidKiBox.Enabled = true;
        pidKpBox.Enabled = true;
        powerModeStepComboBox.Enabled = true;

        CoolDownButton.Enabled = false;
        StopTestButton.Enabled = false;
        ConnectToolstripButton.Enabled = false;
    }

    private void ConnectionUnlockUI() {
        StopTestButton.Enabled = false;
        StartTestButton.Enabled = false;
        CoolDownButton.Enabled = false;
        DisconnectCOMButton.Enabled = false;
        TargetTempInput.Enabled = false;
        DeltaTInput.Enabled = false;
        pidKdBox.Enabled = false;
        pidKiBox.Enabled = false;
        pidKpBox.Enabled = false;
        powerModeStepComboBox.Enabled = false;
        TargetHoldTimeInput.Enabled = false;
        timerModeToolStripMenuItem.Enabled = false;
        emissivityInput.Enabled = false;

        ConnectToolstripButton.Enabled = true;
        ComPortToolstripComboBox.Enabled = true;
    }

    private void ConnectionLockUI() {
        TargetHoldTimeInput.Enabled = true;
        timerModeToolStripMenuItem.Enabled = true;
        emissivityInput.Enabled = true;
        StartTestButton.Enabled = true;
        DisconnectCOMButton.Enabled = true;
        TargetTempInput.Enabled = true;
        DeltaTInput.Enabled = true;
        pidKdBox.Enabled = true;
        pidKiBox.Enabled = true;
        pidKpBox.Enabled = true;
        ComPortToolstripComboBox.Enabled = true;
        powerModeStepComboBox.Enabled = true;

        ConnectToolstripButton.Enabled = false;
        ComPortToolstripComboBox.Enabled = false;
        StopTestButton.Enabled = false;
        CoolDownButton.Enabled = false;
    }

    private void TogglePowerMode() {
        var powerMode = timerModeToolStripMenuItem.Checked;
        _testManager.PowerMode = powerMode;
        label14.Visible = powerMode;
        powerModeStepComboBox.Visible = powerMode;
        powerModeStepComboBox.Enabled = powerMode;

        TargetTempInput.Visible = !powerMode;
        DeltaTInput.Visible = !powerMode;
        label2.Visible = !powerMode;
        label3.Visible = !powerMode;
    }

    private void ResetData() {
        _testManager.ClearData();
        TemperaturePlot.Plot.Clear();
        CurrentTempBox.Clear();
        HighestTempBox.Clear();
        PowerDrawTextBox.Clear();
        RateOfChangeBox.Clear();
        TemperaturePlot.Refresh();
    }

    private void SetUI(TestState state, bool disconnected = false) {
        switch (state) {
            case TestState.Idle:
                toolStrip1.BackColor = Color.FromArgb(185, 209, 234);
                ComPortToolstripComboBox.BackColor = Color.FromArgb(185, 209, 234);
                toolStripDropDownButton1.BackColor = Color.FromArgb(185, 209, 234);
                ConnectionLockUI();
                setPointRemainingTimeBox.Visible = false;
                setPointLabel.Visible = false;

                if (disconnected) {
                    toolStrip1.BackColor = Color.FromArgb(240, 240, 240);
                    ComPortToolstripComboBox.BackColor = Color.FromArgb(240, 240, 240);
                    toolStripDropDownButton1.BackColor = Color.FromArgb(240, 240, 240);
                    ConnectionUnlockUI();
                }
                break;

            case TestState.Running:
                toolStrip1.BackColor = Color.FromArgb(255, 174, 0);
                ComPortToolstripComboBox.BackColor = Color.FromArgb(255, 174, 0);
                toolStripDropDownButton1.BackColor = Color.FromArgb(255, 174, 0);
                LockUI();
                break;

            case TestState.CoolingDown:
                toolStrip1.BackColor = Color.FromArgb(69, 137, 255);
                ComPortToolstripComboBox.BackColor = Color.FromArgb(69, 137, 255);
                toolStripDropDownButton1.BackColor = Color.FromArgb(69, 137, 255);
                setPointRemainingTimeBox.Visible = false;
                setPointLabel.Visible = false;
                break;

            case TestState.Error:
                UnlockUI();
                break;

        }
    }

    private void MessageBoxLog(LogLevel level, string message) {
        _logger.Log(level, message);
        if (level < LogLevel.Information) return;

        var stamp = DateTime.Now.ToString("h:mm tt");
        var messages = message.Split(',').Select(m => m.Trim());

        foreach (var s in messages) {
            if (string.IsNullOrEmpty(s)) continue;

            var levelLabel = string.Empty;
            if (level is not (LogLevel.Information or LogLevel.Critical)) levelLabel = " [" + level + "] ";
            if (level is LogLevel.Critical) levelLabel = "[Reactor] ";
            MessageBox.AppendText($"{levelLabel + s + " - " + stamp}\r\n");
            MessageBox.ScrollToCaret();
        }
    }

    #endregion UI Helpers


    private void tunePIDToolStripMenuItem_Click(object sender, EventArgs e) {
        var isChecked = tunePIDToolStripMenuItem.Checked;
        pidKdBox.Visible = isChecked;
        pidKpBox.Visible = isChecked;
        pidKiBox.Visible = isChecked;
        label4.Visible = isChecked;
        label12.Visible = isChecked;
        label13.Visible = isChecked;
    }

    private void powerModeStepComboBox_SelectedIndexChanged(object sender, EventArgs e) {
        _testManager.PowerModeStep = (PowerStep)powerModeStepComboBox.SelectedItem!;

    }

}