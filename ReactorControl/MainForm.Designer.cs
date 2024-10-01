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
            label1 = new Label();
            label2 = new Label();
            ComPortComboBox = new ComboBox();
            RefreshCOMPorts = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            StartTestButton = new ToolStripButton();
            StopTestButton = new ToolStripButton();
            toolStrip1 = new ToolStrip();
            DisconnectCOMButton = new ToolStripButton();
            CoolDownButton = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            AutoScaleChartButton = new ToolStripButton();
            ExportButton = new ToolStripButton();
            ClearChartButton = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            MessageBox = new TextBox();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            label7 = new Label();
            TemperaturePlot = new ScottPlot.WinForms.FormsPlot();
            TestControlPanel = new Panel();
            textBox1 = new TextBox();
            label8 = new Label();
            LowestTempBox = new TextBox();
            HighestTempBox = new TextBox();
            CurrentTempBox = new TextBox();
            label9 = new Label();
            label6 = new Label();
            TargetHoldTimeInput = new NumericUpDown();
            label5 = new Label();
            ClearMessageButton = new Button();
            ((System.ComponentModel.ISupportInitialize)TargetTempInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)DeltaTInput).BeginInit();
            toolStrip1.SuspendLayout();
            TestControlPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)TargetHoldTimeInput).BeginInit();
            SuspendLayout();
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 388);
            label4.Name = "label4";
            label4.Size = new Size(61, 15);
            label4.TabIndex = 10;
            label4.Text = "Messages:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(167, 103);
            label3.Name = "label3";
            label3.Size = new Size(47, 15);
            label3.TabIndex = 7;
            label3.Text = "∆T (℃):";
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ConnectButton
            // 
            ConnectButton.Location = new Point(167, 57);
            ConnectButton.Name = "ConnectButton";
            ConnectButton.Size = new Size(73, 23);
            ConnectButton.TabIndex = 1;
            ConnectButton.Text = "Connect";
            ConnectButton.UseVisualStyleBackColor = true;
            ConnectButton.Click += ConnectButton_Click;
            // 
            // TargetTempInput
            // 
            TargetTempInput.AutoSize = true;
            TargetTempInput.Location = new Point(12, 121);
            TargetTempInput.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            TargetTempInput.Minimum = new decimal(new int[] { 21, 0, 0, 0 });
            TargetTempInput.Name = "TargetTempInput";
            TargetTempInput.Size = new Size(136, 23);
            TargetTempInput.TabIndex = 3;
            TargetTempInput.Tag = "";
            TargetTempInput.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            TargetTempInput.ValueChanged += TargetTempInput_ValueChanged;
            // 
            // DeltaTInput
            // 
            DeltaTInput.Location = new Point(167, 121);
            DeltaTInput.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            DeltaTInput.Name = "DeltaTInput";
            DeltaTInput.Size = new Size(73, 23);
            DeltaTInput.TabIndex = 4;
            DeltaTInput.Value = new decimal(new int[] { 10, 0, 0, 0 });
            DeltaTInput.ValueChanged += DeltaTInput_ValueChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 39);
            label1.Name = "label1";
            label1.Size = new Size(63, 15);
            label1.TabIndex = 5;
            label1.Text = "COM Port:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 103);
            label2.Name = "label2";
            label2.Size = new Size(97, 15);
            label2.TabIndex = 6;
            label2.Text = "Target Temp (℃):";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ComPortComboBox
            // 
            ComPortComboBox.FormattingEnabled = true;
            ComPortComboBox.Location = new Point(12, 57);
            ComPortComboBox.Name = "ComPortComboBox";
            ComPortComboBox.Size = new Size(136, 23);
            ComPortComboBox.TabIndex = 0;
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
            // toolStrip1
            // 
            toolStrip1.BackColor = SystemColors.GradientActiveCaption;
            toolStrip1.Items.AddRange(new ToolStripItem[] { RefreshCOMPorts, DisconnectCOMButton, toolStripSeparator1, StartTestButton, CoolDownButton, StopTestButton, toolStripSeparator2, AutoScaleChartButton, ExportButton, ClearChartButton, toolStripSeparator3 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1279, 25);
            toolStrip1.TabIndex = 11;
            toolStrip1.Text = "toolStrip1";
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
            // ExportButton
            // 
            ExportButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            ExportButton.Image = (Image)resources.GetObject("ExportButton.Image");
            ExportButton.ImageTransparentColor = Color.Magenta;
            ExportButton.Name = "ExportButton";
            ExportButton.Size = new Size(72, 22);
            ExportButton.Text = "Export Data";
            ExportButton.Click += ExportButton_Click;
            // 
            // ClearChartButton
            // 
            ClearChartButton.Alignment = ToolStripItemAlignment.Right;
            ClearChartButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            ClearChartButton.Image = (Image)resources.GetObject("ClearChartButton.Image");
            ClearChartButton.ImageTransparentColor = Color.Magenta;
            ClearChartButton.Name = "ClearChartButton";
            ClearChartButton.Size = new Size(70, 22);
            ClearChartButton.Text = "Clear Chart";
            ClearChartButton.Click += ClearChartButton_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Alignment = ToolStripItemAlignment.Right;
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 25);
            // 
            // MessageBox
            // 
            MessageBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            MessageBox.Location = new Point(12, 406);
            MessageBox.Multiline = true;
            MessageBox.Name = "MessageBox";
            MessageBox.ReadOnly = true;
            MessageBox.ScrollBars = ScrollBars.Both;
            MessageBox.Size = new Size(240, 365);
            MessageBox.TabIndex = 0;
            MessageBox.WordWrap = false;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(37, 11);
            label7.Name = "label7";
            label7.Size = new Size(105, 15);
            label7.TabIndex = 16;
            label7.Text = "Current Temp (℃):";
            // 
            // TemperaturePlot
            // 
            TemperaturePlot.DisplayScale = 1F;
            TemperaturePlot.Dock = DockStyle.Bottom;
            TemperaturePlot.Location = new Point(0, 29);
            TemperaturePlot.Name = "TemperaturePlot";
            TemperaturePlot.Size = new Size(1021, 727);
            TemperaturePlot.TabIndex = 19;
            // 
            // TestControlPanel
            // 
            TestControlPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TestControlPanel.Controls.Add(textBox1);
            TestControlPanel.Controls.Add(label8);
            TestControlPanel.Controls.Add(LowestTempBox);
            TestControlPanel.Controls.Add(HighestTempBox);
            TestControlPanel.Controls.Add(CurrentTempBox);
            TestControlPanel.Controls.Add(label9);
            TestControlPanel.Controls.Add(label6);
            TestControlPanel.Controls.Add(TemperaturePlot);
            TestControlPanel.Controls.Add(label7);
            TestControlPanel.Location = new Point(258, 28);
            TestControlPanel.Name = "TestControlPanel";
            TestControlPanel.Size = new Size(1021, 756);
            TestControlPanel.TabIndex = 8;
            // 
            // textBox1
            // 
            textBox1.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            textBox1.ForeColor = SystemColors.MenuHighlight;
            textBox1.Location = new Point(804, 3);
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.Size = new Size(75, 31);
            textBox1.TabIndex = 28;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(696, 11);
            label8.Name = "label8";
            label8.Size = new Size(95, 15);
            label8.TabIndex = 27;
            label8.Text = "Power Draw (W):";
            // 
            // LowestTempBox
            // 
            LowestTempBox.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            LowestTempBox.ForeColor = SystemColors.MenuHighlight;
            LowestTempBox.Location = new Point(586, 3);
            LowestTempBox.Name = "LowestTempBox";
            LowestTempBox.ReadOnly = true;
            LowestTempBox.Size = new Size(75, 31);
            LowestTempBox.TabIndex = 26;
            // 
            // HighestTempBox
            // 
            HighestTempBox.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            HighestTempBox.ForeColor = Color.OrangeRed;
            HighestTempBox.Location = new Point(368, 3);
            HighestTempBox.Name = "HighestTempBox";
            HighestTempBox.ReadOnly = true;
            HighestTempBox.Size = new Size(75, 31);
            HighestTempBox.TabIndex = 25;
            // 
            // CurrentTempBox
            // 
            CurrentTempBox.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            CurrentTempBox.Location = new Point(148, 3);
            CurrentTempBox.Name = "CurrentTempBox";
            CurrentTempBox.ReadOnly = true;
            CurrentTempBox.Size = new Size(75, 31);
            CurrentTempBox.TabIndex = 24;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(478, 11);
            label9.Name = "label9";
            label9.Size = new Size(102, 15);
            label9.TabIndex = 23;
            label9.Text = "Lowest Temp (℃):";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(256, 11);
            label6.Name = "label6";
            label6.Size = new Size(106, 15);
            label6.TabIndex = 21;
            label6.Text = "Highest Temp (℃):";
            // 
            // TargetHoldTimeInput
            // 
            TargetHoldTimeInput.Location = new Point(12, 184);
            TargetHoldTimeInput.Maximum = new decimal(new int[] { 86400, 0, 0, 0 });
            TargetHoldTimeInput.Name = "TargetHoldTimeInput";
            TargetHoldTimeInput.Size = new Size(136, 23);
            TargetHoldTimeInput.TabIndex = 12;
            TargetHoldTimeInput.Tag = "";
            TargetHoldTimeInput.ValueChanged += TargetHoldTimeInput_ValueChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 166);
            label5.Name = "label5";
            label5.Size = new Size(116, 15);
            label5.TabIndex = 13;
            label5.Text = "Target Hold Time (s):";
            label5.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ClearMessageButton
            // 
            ClearMessageButton.Location = new Point(227, 385);
            ClearMessageButton.Name = "ClearMessageButton";
            ClearMessageButton.Size = new Size(25, 20);
            ClearMessageButton.TabIndex = 14;
            ClearMessageButton.Text = "✕";
            ClearMessageButton.UseVisualStyleBackColor = true;
            ClearMessageButton.Click += ClearMessageButton_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(1279, 796);
            Controls.Add(ClearMessageButton);
            Controls.Add(label5);
            Controls.Add(TargetHoldTimeInput);
            Controls.Add(MessageBox);
            Controls.Add(toolStrip1);
            Controls.Add(label4);
            Controls.Add(TestControlPanel);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
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
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            TestControlPanel.ResumeLayout(false);
            TestControlPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)TargetHoldTimeInput).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label4;
        private Label label3;
        private Button ConnectButton;
        private NumericUpDown TargetTempInput;
        private NumericUpDown DeltaTInput;
        private Label label1;
        private Label label2;
        private ComboBox ComPortComboBox;
        private ToolStripButton RefreshCOMPorts;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton StartTestButton;
        private ToolStripButton StopTestButton;
        private ToolStrip toolStrip1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton ExportButton;
        private ToolStripButton ClearChartButton;
        private TextBox MessageBox;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private Label label7;
        private ScottPlot.WinForms.FormsPlot TemperaturePlot;
        private Panel TestControlPanel;
        private Label label9;
        private Label label6;
        private TextBox LowestTempBox;
        private TextBox HighestTempBox;
        private TextBox CurrentTempBox;
        private ToolStripButton AutoScaleChartButton;
        private NumericUpDown TargetHoldTimeInput;
        private Label label5;
        private Button ClearMessageButton;
        private ToolStripButton DisconnectCOMButton;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripButton CoolDownButton;
        private TextBox textBox1;
        private Label label8;
    }
}
