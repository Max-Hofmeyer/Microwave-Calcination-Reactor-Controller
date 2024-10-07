using Microsoft.Extensions.Logging;
using ReactorControl.Classes;
using ScottPlot.WinForms;

namespace ReactorControl;

public partial class MainForm : Form
{
    private readonly ComPortManager _portManager;
    private readonly ILogger<MainForm> _logger;
    private readonly TestManager _testManager;
    private bool _watchdogError;
    private bool _unknownError;

    public MainForm(ComPortManager comPortManager, TestManager testManager, ILogger<MainForm> logger) {
        InitializeComponent();
        _portManager = comPortManager;
        _testManager = testManager;
        _logger = logger;

        _testManager.CommandReceived += OnCommandReceived;
        _testManager.TestStateChanged += OnTestStateChanged;
    }

    //Executes when the app is finished loading. Grabs COM ports and sets up graph
    private void MainForm_Load(object sender, EventArgs e) {
        ComPortComboBox.DataSource = ComPortManager.GetAvailableComPorts();
        StopTestButton.Enabled = false;
        StartTestButton.Enabled = false;
        CoolDownButton.Enabled = false;
        DisconnectCOMButton.Enabled = false;
        _testManager.TargetTemp = TargetTempInput.Value;
        _testManager.DeltaTemp = DeltaTInput.Value;
        _testManager.TargetHoldTime = TargetHoldTimeInput.Value;

        //TemperaturePlot.Plot.Title("Temperature vs. Time");

        TemperaturePlot.Plot.XLabel("Time (s)");
        TemperaturePlot.Plot.YLabel("Temperature (°C)");
        TemperaturePlot.Plot.Axes.SetLimitsX(0, 604800);
        TemperaturePlot.Plot.Axes.AutoScale();
        MessageBoxLog(LogLevel.Information, "App Loaded");
    }

    private void MainForm_Closing(Object sender, FormClosingEventArgs e) {
        TryStopTest();
        TryDisconnectFromPort();
    }

    #region Button Click Events

    private void ConnectButton_Click(object sender, EventArgs e) {
        if (ComPortComboBox.SelectedItem is not string portName) return;
        _logger.LogDebug("Attempting to connect to COM port {port}", portName);
        TryConnectToPort(portName);
    }

    private void RefreshCOMButton_Click(object sender, EventArgs e) {
        ComPortComboBox.DataSource = ComPortManager.GetAvailableComPorts();
        MessageBoxLog(LogLevel.Information, "COM ports refreshed");
    }

    private void StartTestButton_Click(object sender, EventArgs e) {
        MessageBoxLog(LogLevel.Information, "Sending start command");
        TryStartTest();
    }
    private void CoolDownButton_Click(object sender, EventArgs e) {
        MessageBoxLog(LogLevel.Information, "Sending cooldown command");
        TryStartCooldown();
    }

    private void StopTestButton_Click(object sender, EventArgs e) {
        MessageBoxLog(LogLevel.Information, "Sending stop command");
        TryStopTest();
    }

    private void ExportButton_Click(object sender, EventArgs e) {
        try {
            _testManager.ExportDataToCSV(null);
            MessageBoxLog(LogLevel.Information, "Successfully saved data to a CSV on the desktop");
        }
        catch (Exception ex) {
            MessageBoxLog(LogLevel.Error, "Failed to export data to CSV: " + ex.Message);
        }
    }

    private void ClearChartButton_Click(object sender, EventArgs e) {
        var result = System.Windows.Forms.MessageBox.Show(
            "Are you sure you want to clear the chart? All data from this session will be lost.",
            "Clear Chart?",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );

        if (result != DialogResult.Yes) return;

        ClearChart();
    }

    private void AutoScaleChartButton_Click(object sender, EventArgs e) {
        TemperaturePlot.Plot.Axes.AutoScale();
        TemperaturePlot.Refresh();
    }

    private void DisconnectCOMButton_Click(object sender, EventArgs e) {
        _logger.LogDebug("Attempting to disconnect from port");
        TryDisconnectFromPort();
    }

    //private void ResetButton_Click(object sender, EventArgs e) {
    //    var result = System.Windows.Forms.MessageBox.Show(
    //        "Are you sure you want to reset the app? All data is deleted and a running test will be stopped",
    //        "Reset?",
    //        MessageBoxButtons.YesNo,
    //        MessageBoxIcon.Warning
    //    );

    //    if (result != DialogResult.Yes) return;
    //    ResetApp();
    //}

    private void ClearMessageButton_Click(object sender, EventArgs e) {
        MessageBox.Text = string.Empty;
    }

    private void OverrideSafeNumbers_Click(object sender, EventArgs e) {
        var result = System.Windows.Forms.MessageBox.Show(
            "Are you sure you want to override input limits for this test? This may cause damage or undefined behavior",
            "Disable Limits?",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );

        if (result != DialogResult.Yes) return;

        _testManager.DisableLimits = true;
        MessageBoxLog(LogLevel.Warning, "Input limits have been disabled");
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

    private void TryConnectToPort(string portName) {
        try {
            ConnectButton.Enabled = false;
            MessageBoxLog(LogLevel.Information, "Connecting to " + portName);
            _portManager.ConnectToPort(portName);
            _testManager.SendCheckStatusCommand();
        }
        catch (Exception ex) {
            ConnectButton.Enabled = true;
            MessageBoxLog(LogLevel.Error, "Failed to connect to " + portName + ": " + ex.Message);
        }
    }

    private void TryDisconnectFromPort() {
        if (!_portManager.IsConnected) return;
        try {
            MessageBoxLog(LogLevel.Information, "Disconnecting from " + ComPortComboBox.SelectedItem);
            _portManager.DisconnectFromPort();
            ConnectButton.Enabled = true;

        }
        catch (Exception ex) {
            MessageBoxLog(LogLevel.Error, "Failed to disconnect from " + ": " + ex.Message);
        }
    }

    private void TryStartTest() {
        try {
            _testManager.SendStartTestCommand();
        }
        catch (Exception ex) {
            MessageBoxLog(LogLevel.Error, "Start command failed: " + ex.Message);
        }
    }

    private void TryStartCooldown() {
        try {
            _testManager.SendCooldownCommand();
        }
        catch (Exception ex) {
            MessageBoxLog(LogLevel.Error, "Cooldown command failed: " + ex.Message);
        }
    }

    private void TryStopTest() {
        try {
            _testManager.SendStopTestCommand();
        }
        catch (Exception ex) {
            MessageBoxLog(LogLevel.Error, "Stop command failed, use stop button if needed: " + ex.Message);
        }
    }

    private void OnCommandReceived(CommandPacket command) {
        if (InvokeRequired) {
            BeginInvoke(new Action<CommandPacket>(OnCommandReceived), command);
            return;
        }

        switch (command.Command) {
            case ReactorCommandsEnum.Data:
                UpdateUI();
                break;

            case ReactorCommandsEnum.Debug:
                if (command.DebugMessage is null) break;
                MessageBoxLog(LogLevel.Critical, command.DebugMessage);
                break;

            case ReactorCommandsEnum.Init:
            case ReactorCommandsEnum.Start:
            case ReactorCommandsEnum.Stop:
                if (_testManager.TestFinished)
                {
                    System.Windows.Forms.MessageBox.Show(
                        "Setpoint time elapsed, test is finished",
                        "Finished",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                
                break;

            case ReactorCommandsEnum.InternalError:
            case ReactorCommandsEnum.Cooldown:
            default:
                break;
        }
    }

    private void OnTestStateChanged(TestState state) {

        if (InvokeRequired) {
            BeginInvoke(new Action<TestState>(OnTestStateChanged), state);
            return;
        }

        switch (state) {
            case TestState.Running:
                MessageBoxLog(LogLevel.Information, "Test is running");
                LockUI();
                break;

            case TestState.CoolingDown:
                MessageBoxLog(LogLevel.Information, "Test is cooling down");
                break;

            case TestState.Stopped:
                MessageBoxLog(LogLevel.Information, "Test has been stopped");
                UnlockUI();
                break;

            case TestState.Unknown:
                if (!_unknownError) {
                    System.Windows.Forms.MessageBox.Show(
                        "Reactor is in an unknown state, use stop button if needed",
                        "ERROR",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    _unknownError = true;
                    MessageBoxLog(LogLevel.Error, "Command failed, use stop button if needed");
                }
                break;

            case TestState.Idle:
                MessageBoxLog(LogLevel.Information, "Connection successful");
                _portManager.IsConnected = true;
                StartTestButton.Enabled = true;
                ConnectButton.Enabled = false;
                break;

            case TestState.Frozen:
                if (!_watchdogError) {
                    HandleFrozenTest();
                    _watchdogError = true;
                    ConnectButton.Enabled = true;
                }
                break;

            default:
                return;
        }
    }

    private void ClearChart() {
        _testManager.ClearData();
        TemperaturePlot.Plot.Clear();
        TemperaturePlot.Refresh();
        MessageBoxLog(LogLevel.Information, "Chart has been cleared");
    }

    //private void ResetApp() {
    //    ClearChart();
    //    TryStopTest();
    //    TryDisconnectFromPort();
    //    UnlockUI();
    //    _unknownError = false;
    //    _watchdogError = false;
    //    //todo make reset test a thing in test manager
    //    _testManager.CurrentTestState = TestState.Unknown;
    //    MessageBoxLog(LogLevel.Information, "App has been reset");
    //}

    private void UpdateUI() {
        try
        {
            var temperatureData = _testManager.GetTemperatureValues();
            var timeStamps = _testManager.GetTimeValues();

            var highestTemp = temperatureData.Max();
            var lowestTemp = temperatureData.Min();
            var currentTemp = temperatureData.LastOrDefault();
            var currentPower = _testManager.GetLatestPowerValue();

            HighestTempBox.Text = $@"{highestTemp:0.0}";
            LowestTempBox.Text = $@"{lowestTemp:0.0}";
            CurrentTempBox.Text = $@"{currentTemp:0.0}";
            PowerDrawTextBox.Text = $@"{currentPower:0.0}";

            TemperaturePlot.Plot.Clear();
            TemperaturePlot.Plot.Add.Scatter(timeStamps.ToArray(), temperatureData.ToArray());
            TemperaturePlot.Refresh();
        }
        catch
        {
            MessageBoxLog(LogLevel.Warning, "Failed to update UI");
        }
    }

    private void LockUI() {
        TargetTempInput.Enabled = false;
        DeltaTInput.Enabled = false;
        StartTestButton.Enabled = false;
        ComPortComboBox.Enabled = false;
        ConnectButton.Enabled = false;
        DisconnectCOMButton.Enabled = false;
        StopTestButton.Enabled = true;
        CoolDownButton.Enabled = true;
    }

    private void UnlockUI() {
        TargetTempInput.Enabled = true;
        DeltaTInput.Enabled = true;
        StartTestButton.Enabled = true;
        ComPortComboBox.Enabled = true;
        ConnectButton.Enabled = true;
        DisconnectCOMButton.Enabled = false;
        CoolDownButton.Enabled = false;
        StopTestButton.Enabled = false;
    }

    private void HandleFrozenTest() {
        var result = System.Windows.Forms.MessageBox.Show(
            "App has lost communication with reactor. Would you like the app to try and restore connection? If connection isn't restored, closed the app and power cycle the reactor",
            "Attempt to restore connection?",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Error
        );

        if (result != DialogResult.Yes) return;
        _portManager.HandleFrozenPort();
    }

    private void MessageBoxLog(LogLevel level, string message) {
        _logger.Log(level, message);
        if (level < LogLevel.Information) return;

        var stamp = DateTime.Now.ToString("MM/dd/yyyy h:mm tt");

        string levelLabel = string.Empty;
        if (level is not (LogLevel.Information or LogLevel.Critical)) {
            levelLabel = " [" + level + "] ";
        }

        if (level is LogLevel.Critical)
        {
            levelLabel = " [Reactor] ";
        }

        MessageBox.AppendText($"{levelLabel + message + " - " + stamp}\r\n");
        MessageBox.ScrollToCaret();
    }

    #endregion UI Helpers

}