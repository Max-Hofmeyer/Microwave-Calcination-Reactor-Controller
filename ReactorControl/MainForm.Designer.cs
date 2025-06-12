namespace ReactorControl
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            label4 = new Label();
            label3 = new Label();
            ConnectButton = new Button();
            TargetTempInput = new NumericUpDown();
            DeltaTInput = new NumericUpDown();
            label2 = new Label();
            ComPortComboBox = new ComboBox();
            MessageBox = new TextBox();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            TemperaturePlot = new ScottPlot.WinForms.FormsPlot();
            TestControlPanel = new Panel();
            PowerDrawTextBox = new TextBox();
            label8 = new Label();
            LowestTempBox = new TextBox();
            HighestTempBox = new TextBox();
            CurrentTempBox = new TextBox();
            label9 = new Label();
            label6 = new Label();
            label7 = new Label();
            InterpolateChart = new CheckBox();
            TargetHoldTimeInput = new NumericUpDown();
            label5 = new Label();
            ClearMessageButton = new Button();
            powerModeCheckBox = new CheckBox();
            label10 = new Label();
            emissivityInput = new NumericUpDown();
            RefreshCOMPorts = new ToolStripButton();
            DisconnectCOMButton = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            StartTestButton = new ToolStripButton();
            CoolDownButton = new ToolStripButton();
            StopTestButton = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            AutoScaleChartButton = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            toolStrip1 = new ToolStrip();
            toolStripDropDownButton1 = new ToolStripDropDownButton();
            chartClearItem = new ToolStripMenuItem();
            autoScaleOnItem = new ToolStripMenuItem();
            interpolateChartItem = new ToolStripMenuItem();
            exportDataToolStripMenuItem = new ToolStripMenuItem();
            testModeToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)TargetTempInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)DeltaTInput).BeginInit();
            TestControlPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)TargetHoldTimeInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)emissivityInput).BeginInit();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(10, 281);
            label4.Name = "label4";
            label4.Size = new Size(61, 15);
            label4.TabIndex = 10;
            label4.Text = "Messages:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(141, 194);
            label3.Name = "label3";
            label3.Size = new Size(48, 15);
            label3.TabIndex = 7;
            label3.Text = "∆T (℃):";
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ConnectButton
            // 
            ConnectButton.Location = new Point(140, 39);
            ConnectButton.Name = "ConnectButton";
            ConnectButton.Size = new Size(99, 23);
            ConnectButton.TabIndex = 1;
            ConnectButton.Text = "Connect";
            ConnectButton.UseVisualStyleBackColor = true;
            ConnectButton.Click += ConnectButton_Click;
            // 
            // TargetTempInput
            // 
            TargetTempInput.AutoSize = true;
            TargetTempInput.Location = new Point(12, 212);
            TargetTempInput.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            TargetTempInput.Minimum = new decimal(new int[] { 21, 0, 0, 0 });
            TargetTempInput.Name = "TargetTempInput";
            TargetTempInput.Size = new Size(100, 23);
            TargetTempInput.TabIndex = 3;
            TargetTempInput.Tag = "";
            TargetTempInput.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            TargetTempInput.ValueChanged += TargetTempInput_ValueChanged;
            // 
            // DeltaTInput
            // 
            DeltaTInput.Location = new Point(141, 212);
            DeltaTInput.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            DeltaTInput.Name = "DeltaTInput";
            DeltaTInput.Size = new Size(99, 23);
            DeltaTInput.TabIndex = 4;
            DeltaTInput.Value = new decimal(new int[] { 10, 0, 0, 0 });
            DeltaTInput.ValueChanged += DeltaTInput_ValueChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 194);
            label2.Name = "label2";
            label2.Size = new Size(66, 15);
            label2.TabIndex = 6;
            label2.Text = "Target (℃):";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ComPortComboBox
            // 
            ComPortComboBox.FormattingEnabled = true;
            ComPortComboBox.Location = new Point(10, 39);
            ComPortComboBox.Name = "ComPortComboBox";
            ComPortComboBox.Size = new Size(100, 23);
            ComPortComboBox.TabIndex = 0;
            // 
            // MessageBox
            // 
            MessageBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            MessageBox.Location = new Point(12, 308);
            MessageBox.Multiline = true;
            MessageBox.Name = "MessageBox";
            MessageBox.ReadOnly = true;
            MessageBox.ScrollBars = ScrollBars.Both;
            MessageBox.Size = new Size(228, 387);
            MessageBox.TabIndex = 0;
            MessageBox.WordWrap = false;
            // 
            // TemperaturePlot
            // 
            TemperaturePlot.DisplayScale = 1F;
            TemperaturePlot.Dock = DockStyle.Bottom;
            TemperaturePlot.Location = new Point(0, 52);
            TemperaturePlot.Name = "TemperaturePlot";
            TemperaturePlot.Size = new Size(1021, 618);
            TemperaturePlot.TabIndex = 19;
            // 
            // TestControlPanel
            // 
            TestControlPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TestControlPanel.Controls.Add(PowerDrawTextBox);
            TestControlPanel.Controls.Add(label8);
            TestControlPanel.Controls.Add(LowestTempBox);
            TestControlPanel.Controls.Add(HighestTempBox);
            TestControlPanel.Controls.Add(CurrentTempBox);
            TestControlPanel.Controls.Add(label9);
            TestControlPanel.Controls.Add(label6);
            TestControlPanel.Controls.Add(TemperaturePlot);
            TestControlPanel.Controls.Add(label7);
            TestControlPanel.Location = new Point(258, 23);
            TestControlPanel.Name = "TestControlPanel";
            TestControlPanel.Size = new Size(1021, 670);
            TestControlPanel.TabIndex = 8;
            // 
            // PowerDrawTextBox
            // 
            PowerDrawTextBox.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            PowerDrawTextBox.ForeColor = SystemColors.MenuHighlight;
            PowerDrawTextBox.Location = new Point(934, 12);
            PowerDrawTextBox.Name = "PowerDrawTextBox";
            PowerDrawTextBox.ReadOnly = true;
            PowerDrawTextBox.Size = new Size(75, 31);
            PowerDrawTextBox.TabIndex = 28;
            PowerDrawTextBox.TextAlign = HorizontalAlignment.Center;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(825, 20);
            label8.Name = "label8";
            label8.Size = new Size(95, 15);
            label8.TabIndex = 27;
            label8.Text = "Power Draw (W):";
            // 
            // LowestTempBox
            // 
            LowestTempBox.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            LowestTempBox.ForeColor = SystemColors.MenuHighlight;
            LowestTempBox.Location = new Point(573, 11);
            LowestTempBox.Name = "LowestTempBox";
            LowestTempBox.ReadOnly = true;
            LowestTempBox.Size = new Size(75, 31);
            LowestTempBox.TabIndex = 26;
            LowestTempBox.TextAlign = HorizontalAlignment.Center;
            // 
            // HighestTempBox
            // 
            HighestTempBox.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            HighestTempBox.ForeColor = Color.OrangeRed;
            HighestTempBox.Location = new Point(356, 12);
            HighestTempBox.Name = "HighestTempBox";
            HighestTempBox.ReadOnly = true;
            HighestTempBox.Size = new Size(75, 31);
            HighestTempBox.TabIndex = 25;
            HighestTempBox.TextAlign = HorizontalAlignment.Center;
            // 
            // CurrentTempBox
            // 
            CurrentTempBox.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            CurrentTempBox.Location = new Point(134, 12);
            CurrentTempBox.Name = "CurrentTempBox";
            CurrentTempBox.ReadOnly = true;
            CurrentTempBox.Size = new Size(75, 31);
            CurrentTempBox.TabIndex = 24;
            CurrentTempBox.TextAlign = HorizontalAlignment.Center;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(442, 20);
            label9.Name = "label9";
            label9.Size = new Size(117, 15);
            label9.TabIndex = 23;
            label9.Text = "Temp Change (℃/s):";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(235, 19);
            label6.Name = "label6";
            label6.Size = new Size(107, 15);
            label6.TabIndex = 21;
            label6.Text = "Highest Temp (℃):";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(15, 19);
            label7.Name = "label7";
            label7.Size = new Size(106, 15);
            label7.TabIndex = 16;
            label7.Text = "Current Temp (℃):";
            // 
            // InterpolateChart
            // 
            InterpolateChart.AutoSize = true;
            InterpolateChart.Location = new Point(1196, 699);
            InterpolateChart.Name = "InterpolateChart";
            InterpolateChart.Size = new Size(83, 19);
            InterpolateChart.TabIndex = 15;
            InterpolateChart.Text = "Interpolate";
            InterpolateChart.UseVisualStyleBackColor = true;
            InterpolateChart.Visible = false;
            InterpolateChart.CheckedChanged += InterpolateChart_CheckedChanged;
            // 
            // TargetHoldTimeInput
            // 
            TargetHoldTimeInput.Location = new Point(141, 152);
            TargetHoldTimeInput.Maximum = new decimal(new int[] { 86400, 0, 0, 0 });
            TargetHoldTimeInput.Name = "TargetHoldTimeInput";
            TargetHoldTimeInput.Size = new Size(99, 23);
            TargetHoldTimeInput.TabIndex = 12;
            TargetHoldTimeInput.Tag = "";
            TargetHoldTimeInput.Value = new decimal(new int[] { 30, 0, 0, 0 });
            TargetHoldTimeInput.ValueChanged += TargetHoldTimeInput_ValueChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(141, 134);
            label5.Name = "label5";
            label5.Size = new Size(82, 15);
            label5.TabIndex = 13;
            label5.Text = "Hold Time (s):";
            label5.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ClearMessageButton
            // 
            ClearMessageButton.Location = new Point(214, 281);
            ClearMessageButton.Name = "ClearMessageButton";
            ClearMessageButton.Size = new Size(25, 20);
            ClearMessageButton.TabIndex = 14;
            ClearMessageButton.Text = "✕";
            ClearMessageButton.UseVisualStyleBackColor = true;
            ClearMessageButton.Click += ClearMessageButton_Click;
            // 
            // powerModeCheckBox
            // 
            powerModeCheckBox.AutoSize = true;
            powerModeCheckBox.Location = new Point(12, 98);
            powerModeCheckBox.Name = "powerModeCheckBox";
            powerModeCheckBox.Size = new Size(91, 19);
            powerModeCheckBox.TabIndex = 15;
            powerModeCheckBox.Text = "Timer Mode";
            powerModeCheckBox.UseVisualStyleBackColor = true;
            powerModeCheckBox.CheckedChanged += powerModeCheckBox_CheckedChanged;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(12, 134);
            label10.Name = "label10";
            label10.Size = new Size(62, 15);
            label10.TabIndex = 17;
            label10.Text = "Emissivity:";
            label10.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // emissivityInput
            // 
            emissivityInput.AutoSize = true;
            emissivityInput.DecimalPlaces = 3;
            emissivityInput.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            emissivityInput.Location = new Point(12, 152);
            emissivityInput.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            emissivityInput.Minimum = new decimal(new int[] { 100, 0, 0, int.MinValue });
            emissivityInput.Name = "emissivityInput";
            emissivityInput.Size = new Size(100, 23);
            emissivityInput.TabIndex = 16;
            emissivityInput.Tag = "";
            emissivityInput.Value = new decimal(new int[] { 9, 0, 0, 65536 });
            emissivityInput.ValueChanged += emissivityInput_ValueChanged;
            // 
            // RefreshCOMPorts
            // 
            RefreshCOMPorts.DisplayStyle = ToolStripItemDisplayStyle.Text;
            RefreshCOMPorts.Image = (Image)resources.GetObject("RefreshCOMPorts.Image");
            RefreshCOMPorts.ImageTransparentColor = Color.Magenta;
            RefreshCOMPorts.Name = "RefreshCOMPorts";
            RefreshCOMPorts.Size = new Size(80, 22);
            RefreshCOMPorts.Text = "Refresh Ports";
            RefreshCOMPorts.Click += RefreshCOMButton_Click;
            // 
            // DisconnectCOMButton
            // 
            DisconnectCOMButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            DisconnectCOMButton.Image = (Image)resources.GetObject("DisconnectCOMButton.Image");
            DisconnectCOMButton.ImageTransparentColor = Color.Magenta;
            DisconnectCOMButton.Name = "DisconnectCOMButton";
            DisconnectCOMButton.Size = new Size(70, 22);
            DisconnectCOMButton.Text = "Disconnect";
            DisconnectCOMButton.Click += DisconnectCOMButton_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // StartTestButton
            // 
            StartTestButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            StartTestButton.Image = (Image)resources.GetObject("StartTestButton.Image");
            StartTestButton.ImageTransparentColor = Color.Magenta;
            StartTestButton.Name = "StartTestButton";
            StartTestButton.Size = new Size(35, 22);
            StartTestButton.Text = "Start";
            StartTestButton.Click += StartTestButton_Click;
            // 
            // CoolDownButton
            // 
            CoolDownButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            CoolDownButton.Image = (Image)resources.GetObject("CoolDownButton.Image");
            CoolDownButton.ImageTransparentColor = Color.Magenta;
            CoolDownButton.Name = "CoolDownButton";
            CoolDownButton.Size = new Size(66, 22);
            CoolDownButton.Text = "Cooldown";
            CoolDownButton.Click += CoolDownButton_Click;
            // 
            // StopTestButton
            // 
            StopTestButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            StopTestButton.Image = (Image)resources.GetObject("StopTestButton.Image");
            StopTestButton.ImageTransparentColor = Color.Magenta;
            StopTestButton.Name = "StopTestButton";
            StopTestButton.Size = new Size(35, 22);
            StopTestButton.Text = "Stop";
            StopTestButton.Click += StopTestButton_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // AutoScaleChartButton
            // 
            AutoScaleChartButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            AutoScaleChartButton.Image = (Image)resources.GetObject("AutoScaleChartButton.Image");
            AutoScaleChartButton.ImageTransparentColor = Color.Magenta;
            AutoScaleChartButton.Name = "AutoScaleChartButton";
            AutoScaleChartButton.Size = new Size(67, 22);
            AutoScaleChartButton.Text = "Auto Scale";
            AutoScaleChartButton.Click += AutoScaleChartButton_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Alignment = ToolStripItemAlignment.Right;
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 25);
            // 
            // toolStrip1
            // 
            toolStrip1.BackColor = SystemColors.ButtonFace;
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { RefreshCOMPorts, DisconnectCOMButton, toolStripSeparator1, StartTestButton, CoolDownButton, StopTestButton, toolStripSeparator2, AutoScaleChartButton, toolStripDropDownButton1, toolStripSeparator3 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1279, 25);
            toolStrip1.TabIndex = 11;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.Alignment = ToolStripItemAlignment.Right;
            toolStripDropDownButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[] { chartClearItem, autoScaleOnItem, interpolateChartItem, exportDataToolStripMenuItem, testModeToolStripMenuItem });
            toolStripDropDownButton1.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.RightToLeftAutoMirrorImage = true;
            toolStripDropDownButton1.Size = new Size(62, 22);
            toolStripDropDownButton1.Text = "Options";
            // 
            // chartClearItem
            // 
            chartClearItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
            chartClearItem.Name = "chartClearItem";
            chartClearItem.Size = new Size(180, 22);
            chartClearItem.Text = "Clear";
            chartClearItem.Click += chartClearItem_Click;
            // 
            // autoScaleOnItem
            // 
            autoScaleOnItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
            autoScaleOnItem.Name = "autoScaleOnItem";
            autoScaleOnItem.Size = new Size(180, 22);
            autoScaleOnItem.Text = "Always Autoscale";
            autoScaleOnItem.Click += autoScaleOnItem_Click;
            // 
            // interpolateChartItem
            // 
            interpolateChartItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
            interpolateChartItem.Name = "interpolateChartItem";
            interpolateChartItem.Size = new Size(180, 22);
            interpolateChartItem.Text = "Interpolate";
            interpolateChartItem.Click += toolStripMenuItem3_Click;
            // 
            // exportDataToolStripMenuItem
            // 
            exportDataToolStripMenuItem.Name = "exportDataToolStripMenuItem";
            exportDataToolStripMenuItem.Size = new Size(180, 22);
            exportDataToolStripMenuItem.Text = "Export Data";
            exportDataToolStripMenuItem.Click += exportDataToolStripMenuItem_Click;
            // 
            // testModeToolStripMenuItem
            // 
            testModeToolStripMenuItem.Name = "testModeToolStripMenuItem";
            testModeToolStripMenuItem.Size = new Size(180, 22);
            testModeToolStripMenuItem.Text = "Test Mode";
            testModeToolStripMenuItem.Click += testModeToolStripMenuItem_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(1279, 715);
            Controls.Add(InterpolateChart);
            Controls.Add(label10);
            Controls.Add(emissivityInput);
            Controls.Add(powerModeCheckBox);
            Controls.Add(ClearMessageButton);
            Controls.Add(label5);
            Controls.Add(TargetHoldTimeInput);
            Controls.Add(MessageBox);
            Controls.Add(toolStrip1);
            Controls.Add(label4);
            Controls.Add(TestControlPanel);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(DeltaTInput);
            Controls.Add(TargetTempInput);
            Controls.Add(ConnectButton);
            Controls.Add(ComPortComboBox);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "MainForm";
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "Reactor Controller";
            FormClosing += MainForm_Closing;
            Load += MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)TargetTempInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)DeltaTInput).EndInit();
            TestControlPanel.ResumeLayout(false);
            TestControlPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)TargetHoldTimeInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)emissivityInput).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label4;
        private Label label3;
        private Button ConnectButton;
        private NumericUpDown TargetTempInput;
        private NumericUpDown DeltaTInput;
        private Label label2;
        private ComboBox ComPortComboBox;
        private TextBox MessageBox;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private ScottPlot.WinForms.FormsPlot TemperaturePlot;
        private Panel TestControlPanel;
        private NumericUpDown TargetHoldTimeInput;
        private Label label5;
        private Button ClearMessageButton;
        private TextBox PowerDrawTextBox;
        private Label label8;
        private CheckBox InterpolateChart;
        private CheckBox powerModeCheckBox;
        private Label label10;
        private NumericUpDown emissivityInput;
        private ToolStripButton RefreshCOMPorts;
        private ToolStripButton DisconnectCOMButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton StartTestButton;
        private ToolStripButton CoolDownButton;
        private ToolStripButton StopTestButton;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton AutoScaleChartButton;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStrip toolStrip1;
        private TextBox LowestTempBox;
        private TextBox HighestTempBox;
        private TextBox CurrentTempBox;
        private Label label9;
        private Label label6;
        private Label label7;
        private ToolStripDropDownButton toolStripDropDownButton1;
        private ToolStripMenuItem chartClearItem;
        private ToolStripMenuItem autoScaleOnItem;
        private ToolStripMenuItem interpolateChartItem;
        private ToolStripMenuItem exportDataToolStripMenuItem;
        private ToolStripMenuItem testModeToolStripMenuItem;
    }
}
