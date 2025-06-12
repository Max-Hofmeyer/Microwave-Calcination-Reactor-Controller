using Microsoft.Extensions.Logging;
using ReactorControl.Classes;

namespace ReactorControl;

public partial class MainForm : Form
{
    private readonly ILogger<MainForm> _logger;
    private readonly ComPortManager _portManager;
    private readonly TestManager _testManager;
    private string _lastDebugCommand;

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
        LockUI();
        ConnectionLockUI();

        _testManager.TargetTemp = TargetTempInput.Value;
        _testManager.DeltaTemp = DeltaTInput.Value;
        _testManager.TargetHoldTime = TargetHoldTimeInput.Value;
        _testManager.Emissivity = emissivityInput.Value;
        _testManager.TestMode = false;

        //TemperaturePlot.Plot.Title("Temperature vs. Time");

        TemperaturePlot.Plot.XLabel("Time (s)");
        TemperaturePlot.Plot.YLabel("Temperature (°C)");
        TemperaturePlot.Plot.Axes.AutoScale();
    }

    private void MainForm_Closing(object sender, FormClosingEventArgs e) {
        TryStopTest();
        _portManager.DisconnectFromPort();
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

    private void powerModeCheckBox_CheckedChanged(object sender, EventArgs e) {
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

    private void toolStripMenuItem3_Click(object sender, EventArgs e) {
        bool interpolated = InterpolateChart.Checked;
        InterpolateChart.Checked = !interpolated;
        interpolateChartItem.Checked = !interpolated;
    }

    private void exportDataToolStripMenuItem_Click(object sender, EventArgs e) {
        try {
            _testManager.ExportDataToCSV(null);
            MessageBoxLog(LogLevel.Information, "Successfully saved data to a CSV on the desktop");
        }
        catch (Exception ex) {
            MessageBoxLog(LogLevel.Error, "Failed to export data to CSV: " + ex.Message);
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
            MessageBoxLog(LogLevel.Information, "Connecting to " + portName);
            _portManager.ConnectToPort(portName);
            _testManager.SendCheckStatusCommand();
            UnlockUI();

            ComPortComboBox.Enabled = false;
            ConnectButton.Enabled = false;
            DisconnectCOMButton.Enabled = true;
        }
        catch (Exception ex) {
            ConnectButton.Enabled = true;
            ComPortComboBox.Enabled = true;
            DisconnectCOMButton.Enabled = false;
            MessageBoxLog(LogLevel.Error, "Failed to connect to " + portName + ": " + ex.Message);
        }
    }

    private void TryDisconnectFromPort() {
        try {
            _portManager.DisconnectFromPort();
            if (_portManager.IsConnected) return;

            LockUI();
            ConnectionLockUI();
            toolStrip1.BackColor = Color.FromArgb(240, 240, 240);
            MessageBoxLog(LogLevel.Information, "Disconnected from " + ComPortComboBox.SelectedItem);
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
            _testManager.PowerMode = powerModeCheckBox.Checked;
            _testManager.SendStartTestCommand();
            ResetData();
        }
        catch (Exception ex) {
            MessageBoxLog(LogLevel.Error, "Start command failed: " + ex.Message);
        }
    }

    private void TryStartCooldown() {
        try {
            _testManager.SubmitCommand(Models.ReactorCommandsEnum.Cooldown);
        }
        catch (Exception ex) {
            MessageBoxLog(LogLevel.Error, "Cooldown command failed: " + ex.Message);
        }
    }

    private void TryStopTest() {
        try {
            _testManager.SubmitCommand(Models.ReactorCommandsEnum.Stop);
        }
        catch (Exception ex) {
            MessageBoxLog(LogLevel.Error, "Stop command failed. Use stop button if needed: " + ex.Message);
        }
    }

    //executes every time a command is received, used mainly to refresh UI and route debug messages 
    private void OnCommandReceived(Models.CommandPacket command) {
        //wait until UI thread 
        if (InvokeRequired) {
            BeginInvoke(new Action<Models.CommandPacket>(OnCommandReceived), command);
            return;
        }

        if (command.Command == Models.ReactorCommandsEnum.Data)
            if (!string.IsNullOrEmpty(command.DebugMessage))
                MessageBoxLog(LogLevel.Critical, command.DebugMessage.Trim());

        if (command is { Command: Models.ReactorCommandsEnum.Debug, DebugMessage: not null })
            MessageBoxLog(LogLevel.Critical, command.DebugMessage);

        UpdateUI();
    }


    //change UI based on the test state sent back by the reactor
    private void OnTestStateChanged(Models.TestState state) {
        //wait until UI thread 
        if (InvokeRequired) {
            BeginInvoke(new Action<Models.TestState>(OnTestStateChanged), state);
            return;
        }

        toolStrip1.BackColor = Color.FromArgb(185, 209, 234);
        switch (state) {
            case Models.TestState.Running:
                toolStrip1.BackColor = Color.FromArgb(255, 174, 0);
                MessageBoxLog(LogLevel.Information, "Test is running");
                LockUI();
                break;

            case Models.TestState.CoolingDown:
                toolStrip1.BackColor = Color.FromArgb(69, 137, 255);
                MessageBoxLog(LogLevel.Information, "Test is cooling down");
                CoolDownButton.Enabled = false;
                StartTestButton.Enabled = true;
                if (_testManager.TestFinished) {
                    System.Windows.Forms.MessageBox.Show(
                        "Setpoint time elapsed, test is finished. Cooling down",
                        "Finished",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    StartTestButton.Enabled = false;
                }

                break;

            case Models.TestState.Stopped:
                MessageBoxLog(LogLevel.Information, "Test has been stopped");
                UnlockUI();

                //if app detected a reactor crash notify the user
                if (_testManager.CommandLastReceived.WithErrors.HasValue &&
                    _testManager.CommandLastReceived.WithErrors.Value)
                    System.Windows.Forms.MessageBox.Show(
                        "Microwave hard crashed, test has been stopped",
                        "Crash",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Stop
                    );
                break;

            case Models.TestState.Idle:
                MessageBoxLog(LogLevel.Information, "Connection successful");
                UnlockUI();
                ConnectButton.Enabled = false;
                ComPortComboBox.Enabled = false;
                break;

            case Models.TestState.Disconnected:
                System.Windows.Forms.MessageBox.Show(
                    "USB connection error. Failed to connect to microwave or the USB was unplugged during a test",
                    "Disconnected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop
                );
                MessageBoxLog(LogLevel.Information, "Microwave disconnected. Resetting app");
                TryDisconnectFromPort();
                break;

            case Models.TestState.Frozen:
                MessageBoxLog(LogLevel.Information, "Watchdog elapsed");
                break;

            case Models.TestState.Unknown:
            default:
                return;
        }
    }

    //display the temperature data on the chart and boxes everytime data is received 
    private void UpdateUI() {
        try {
            List<double> temperatureData;
            List<double> timeStamps;
            var highestTemp = 0.0;
            var rateOfChangeTemp = 0.0;

            if (InterpolateChart.Checked) {
                temperatureData = _testManager.GetTemperatureInterpolatedValues();
                timeStamps = _testManager.GetTimeInterpolatedValues();
            }
            else {
                temperatureData = _testManager.GetTemperatureValues();
                timeStamps = _testManager.GetTimeValues();
            }

            var currentTemp = temperatureData.LastOrDefault();
            var currentPower = _testManager.GetLatestPowerValue();

            if (temperatureData.Count > 0)
                highestTemp = temperatureData.Max();

            if (temperatureData.Count > 6) {
                rateOfChangeTemp = temperatureData[^1] - temperatureData[^6];

                if (InterpolateChart.Checked)
                    currentTemp = temperatureData.TakeLast(6).Average();
            }

            HighestTempBox.Text = $@"{highestTemp:0.0}";
            LowestTempBox.Text = $@"{rateOfChangeTemp:0.0}";
            CurrentTempBox.Text = $@"{currentTemp:0.0}";
            PowerDrawTextBox.Text = $@"{currentPower:0.0}";

            TemperaturePlot.Plot.Clear();
            TemperaturePlot.Plot.Add.Scatter(timeStamps.ToArray(), temperatureData.ToArray());
            TemperaturePlot.Refresh();
        }
        catch (Exception ex) {
            Console.WriteLine($@"Error in UI update: {ex.Message}");
            //MessageBoxLog(LogLevel.Warning, "No data to display. Check temperature sensor connections");
        }
    }

    private void LockUI() {
        TargetTempInput.Enabled = false;
        DeltaTInput.Enabled = false;
        StartTestButton.Enabled = false;
        ComPortComboBox.Enabled = false;
        ConnectButton.Enabled = false;
        DisconnectCOMButton.Enabled = false;
        TargetHoldTimeInput.Enabled = false;
        powerModeCheckBox.Enabled = false;
        emissivityInput.Enabled = false;
        StopTestButton.Enabled = true;
        CoolDownButton.Enabled = true;
    }

    private void UnlockUI() {
        TargetTempInput.Enabled = true;
        DeltaTInput.Enabled = true;
        StartTestButton.Enabled = true;
        ComPortComboBox.Enabled = true;
        DisconnectCOMButton.Enabled = true;
        TargetHoldTimeInput.Enabled = true;
        powerModeCheckBox.Enabled = true;
        emissivityInput.Enabled = true;
        CoolDownButton.Enabled = false;
        StopTestButton.Enabled = false;
    }

    private void ConnectionLockUI() {
        StopTestButton.Enabled = false;
        CoolDownButton.Enabled = false;
        ConnectButton.Enabled = true;
        ComPortComboBox.Enabled = true;
    }

    private void TogglePowerMode() {
        var powerMode = powerModeCheckBox.Checked;
        _testManager.PowerMode = powerMode;
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
        LowestTempBox.Clear();
        TemperaturePlot.Refresh();
    }

    private void MessageBoxLog(LogLevel level, string message) {
        _logger.Log(level, message);
        if (level < LogLevel.Information || _lastDebugCommand == message) return;
        _lastDebugCommand = message;
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


    private void InterpolateChart_CheckedChanged(object sender, EventArgs e) {
        UpdateUI();
    }

    #endregion UI Helpers


   
}