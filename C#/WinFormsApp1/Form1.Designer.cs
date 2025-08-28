using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WinFormsApp1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null; // 用於管理控制項生命週期的容器

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) // 釋放資源的方法
        {
            if (disposing && (components != null)) // 如果正在釋放資源且容器存在
            {
                components.Dispose(); // 釋放容器內的所有控制項
            }
            base.Dispose(disposing); // 呼叫父類別的釋放方法
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() // 初始化所有控制項的主要方法
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            topPanel = new Panel();
            label7 = new Label();
            label6 = new Label();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            btnPeriod = new Button();
            maSettingsButton = new Button();
            button3 = new Button();
            button1 = new Button();
            textBox2 = new TextBox();
            button2 = new Button();
            labelDays = new Label();
            textBox1 = new TextBox();
            label1 = new Label();
            chartPanel = new Panel();
            chart1Panel = new Panel();
            Kchart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            chart2Panel = new Panel();
            comboBox1 = new ComboBox();
            chart2 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            chart3Panel = new Panel();
            comboBox2 = new ComboBox();
            chart3 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            topPanel.SuspendLayout();
            chartPanel.SuspendLayout();
            chart1Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)Kchart).BeginInit();
            chart2Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chart2).BeginInit();
            chart3Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chart3).BeginInit();
            SuspendLayout();
            // 
            // topPanel
            // 
            topPanel.BackColor = Color.FromArgb(40, 40, 40);
            topPanel.Controls.Add(label7);
            topPanel.Controls.Add(label6);
            topPanel.Controls.Add(label5);
            topPanel.Controls.Add(label4);
            topPanel.Controls.Add(label3);
            topPanel.Controls.Add(label2);
            topPanel.Controls.Add(btnPeriod);
            topPanel.Controls.Add(maSettingsButton);
            topPanel.Controls.Add(button3);
            topPanel.Controls.Add(button1);
            topPanel.Controls.Add(textBox2);
            topPanel.Controls.Add(button2);
            topPanel.Controls.Add(labelDays);
            topPanel.Controls.Add(textBox1);
            topPanel.Controls.Add(label1);
            topPanel.Dock = DockStyle.Top;
            topPanel.Location = new Point(0, 0);
            topPanel.Margin = new Padding(6, 8, 6, 8);
            topPanel.Name = "topPanel";
            topPanel.Size = new Size(3747, 152);
            topPanel.TabIndex = 0;
            // 
            // label7
            // 
            label7.BorderStyle = BorderStyle.Fixed3D;
            label7.Font = new System.Drawing.Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 136);
            label7.Location = new Point(3398, 41);
            label7.Name = "label7";
            label7.Size = new Size(350, 69);
            label7.TabIndex = 13;
            // 
            // label6
            // 
            label6.BorderStyle = BorderStyle.Fixed3D;
            label6.Font = new System.Drawing.Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 136);
            label6.Location = new Point(3048, 41);
            label6.Name = "label6";
            label6.Size = new Size(350, 69);
            label6.TabIndex = 12;
            // 
            // label5
            // 
            label5.BorderStyle = BorderStyle.Fixed3D;
            label5.Font = new System.Drawing.Font("Microsoft JhengHei UI", 12F, FontStyle.Bold);
            label5.Location = new Point(2698, 41);
            label5.Name = "label5";
            label5.Size = new Size(350, 69);
            label5.TabIndex = 11;
            // 
            // label4
            // 
            label4.BorderStyle = BorderStyle.Fixed3D;
            label4.Font = new System.Drawing.Font("Microsoft JhengHei UI", 12F, FontStyle.Bold);
            label4.Location = new Point(2348, 41);
            label4.Name = "label4";
            label4.Size = new Size(350, 69);
            label4.TabIndex = 10;
            // 
            // label3
            // 
            label3.BorderStyle = BorderStyle.Fixed3D;
            label3.Font = new System.Drawing.Font("Microsoft JhengHei UI", 12F, FontStyle.Bold);
            label3.Location = new Point(1998, 41);
            label3.Name = "label3";
            label3.Size = new Size(350, 69);
            label3.TabIndex = 9;
            // 
            // label2
            // 
            label2.BorderStyle = BorderStyle.Fixed3D;
            label2.Font = new System.Drawing.Font("Microsoft JhengHei UI", 12F, FontStyle.Bold);
            label2.Location = new Point(1648, 41);
            label2.Name = "label2";
            label2.Size = new Size(350, 69);
            label2.TabIndex = 8;
            // 
            // btnPeriod
            // 
            btnPeriod.BackColor = Color.LightBlue;
            btnPeriod.Font = new System.Drawing.Font("Microsoft JhengHei UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 136);
            btnPeriod.ForeColor = Color.DimGray;
            btnPeriod.Location = new Point(990, 38);
            btnPeriod.Margin = new Padding(6, 8, 6, 8);
            btnPeriod.Name = "btnPeriod";
            btnPeriod.Size = new Size(160, 76);
            btnPeriod.TabIndex = 14;
            btnPeriod.Text = "日線";
            btnPeriod.UseVisualStyleBackColor = false;
            btnPeriod.Click += BtnPeriod_Click;
            // 
            // maSettingsButton
            // 
            maSettingsButton.BackColor = Color.Black;
            maSettingsButton.ForeColor = SystemColors.AppWorkspace;
            maSettingsButton.Location = new Point(1406, 38);
            maSettingsButton.Margin = new Padding(6, 8, 6, 8);
            maSettingsButton.Name = "maSettingsButton";
            maSettingsButton.Size = new Size(216, 76);
            maSettingsButton.TabIndex = 7;
            maSettingsButton.Text = "均線設定";
            maSettingsButton.UseVisualStyleBackColor = false;
            maSettingsButton.Click += MASettingsButton_Click;
            // 
            // button3
            // 
            button3.BackColor = Color.Black;
            button3.ForeColor = SystemColors.AppWorkspace;
            button3.Location = new Point(1170, 38);
            button3.Margin = new Padding(6, 8, 6, 8);
            button3.Name = "button3";
            button3.Size = new Size(216, 76);
            button3.TabIndex = 6;
            button3.Text = "比較選股";
            button3.UseVisualStyleBackColor = false;
            button3.Click += button3_Click;
            // 
            // button1
            // 
            button1.Location = new Point(900, 38);
            button1.Margin = new Padding(6, 8, 6, 8);
            button1.Name = "button1";
            button1.Size = new Size(80, 76);
            button1.TabIndex = 5;
            button1.Text = "+";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(720, 48);
            textBox2.Margin = new Padding(6, 8, 6, 8);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(150, 46);
            textBox2.TabIndex = 4;
            textBox2.Text = "200";
            textBox2.KeyPress += textBox2_KeyPress;
            // 
            // button2
            // 
            button2.Location = new Point(612, 38);
            button2.Margin = new Padding(6, 8, 6, 8);
            button2.Name = "button2";
            button2.Size = new Size(80, 76);
            button2.TabIndex = 3;
            button2.Text = "-";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // labelDays
            // 
            labelDays.AutoSize = true;
            labelDays.ForeColor = SystemColors.AppWorkspace;
            labelDays.Location = new Point(504, 56);
            labelDays.Margin = new Padding(6, 0, 6, 0);
            labelDays.Name = "labelDays";
            labelDays.Size = new Size(84, 38);
            labelDays.TabIndex = 2;
            labelDays.Text = "天數:";
            // 
            // textBox1
            // 
            textBox1.BackColor = SystemColors.ScrollBar;
            textBox1.Location = new Point(216, 48);
            textBox1.Margin = new Padding(6, 8, 6, 8);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(248, 46);
            textBox1.TabIndex = 1;
            textBox1.KeyPress += textBox1_KeyPress;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = SystemColors.AppWorkspace;
            label1.Location = new Point(36, 56);
            label1.Margin = new Padding(6, 0, 6, 0);
            label1.Name = "label1";
            label1.Size = new Size(144, 38);
            label1.TabIndex = 0;
            label1.Text = "股票代號:";
            // 
            // chartPanel
            // 
            chartPanel.Controls.Add(chart1Panel);
            chartPanel.Controls.Add(chart2Panel);
            chartPanel.Controls.Add(chart3Panel);
            chartPanel.Dock = DockStyle.Fill;
            chartPanel.Location = new Point(0, 152);
            chartPanel.Margin = new Padding(6, 8, 6, 8);
            chartPanel.Name = "chartPanel";
            chartPanel.Size = new Size(3747, 1852);
            chartPanel.TabIndex = 1;
            // 
            // chart1Panel
            // 
            chart1Panel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            chart1Panel.Controls.Add(Kchart);
            chart1Panel.Location = new Point(0, 0);
            chart1Panel.Margin = new Padding(6, 8, 6, 8);
            chart1Panel.Name = "chart1Panel";
            chart1Panel.Size = new Size(3747, 1108);
            chart1Panel.TabIndex = 0;
            // 
            // Kchart
            // 
            Kchart.BackColor = Color.Black;
            Kchart.BorderlineColor = Color.Transparent;
            Kchart.BorderlineWidth = 0;
            chartArea1.AxisX.LabelStyle.Format = "MM/dd";
            chartArea1.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea1.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartArea1.BackColor = Color.Black;
            chartArea1.BorderColor = Color.Transparent;
            chartArea1.BorderWidth = 0;
            chartArea1.InnerPlotPosition.Auto = false;
            chartArea1.InnerPlotPosition.Height = 85F;
            chartArea1.InnerPlotPosition.Width = 90F;
            chartArea1.InnerPlotPosition.X = 5F;
            chartArea1.InnerPlotPosition.Y = 5F;
            chartArea1.Name = "StockArea";
            chartArea1.Position.Auto = false;
            chartArea1.Position.Height = 90F;
            chartArea1.Position.Width = 100F;
            chartArea1.Position.Y = 5F;
            Kchart.ChartAreas.Add(chartArea1);
            Kchart.Dock = DockStyle.Fill;
            Kchart.Location = new Point(0, 0);
            Kchart.Margin = new Padding(6, 8, 6, 8);
            Kchart.Name = "Kchart";
            Kchart.Size = new Size(3747, 1108);
            Kchart.TabIndex = 0;
            Kchart.Paint += Kchart_Paint;
            Kchart.MouseDoubleClick += Kchart_MouseDoubleClick;
            Kchart.MouseDown += Kchart_MouseDown;
            Kchart.MouseMove += Kchart_MouseMove;
            Kchart.MouseUp += Kchart_MouseUp;
            // 
            // chart2Panel
            // 
            chart2Panel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            chart2Panel.Controls.Add(comboBox1);
            chart2Panel.Controls.Add(chart2);
            chart2Panel.Location = new Point(0, 1124);
            chart2Panel.Margin = new Padding(6, 8, 6, 8);
            chart2Panel.Name = "chart2Panel";
            chart2Panel.Size = new Size(3747, 408);
            chart2Panel.TabIndex = 1;
            // 
            // comboBox1
            // 
            comboBox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "成交量", "外資", "投信", "自營商", "融資", "融券", "KD" });
            comboBox1.Location = new Point(3495, 28);
            comboBox1.Margin = new Padding(6, 8, 6, 8);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(212, 46);
            comboBox1.TabIndex = 1;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // chart2
            // 
            chart2.BackColor = Color.Black;
            chart2.BorderlineColor = Color.Transparent;
            chart2.BorderlineWidth = 0;
            chartArea2.AxisX.LabelStyle.Format = "MM/dd";
            chartArea2.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea2.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartArea2.BackColor = Color.Black;
            chartArea2.BorderColor = Color.Transparent;
            chartArea2.BorderWidth = 0;
            chartArea2.Name = "Chart2Area";
            chart2.ChartAreas.Add(chartArea2);
            chart2.Dock = DockStyle.Fill;
            chart2.Location = new Point(0, 0);
            chart2.Margin = new Padding(6, 8, 6, 8);
            chart2.Name = "chart2";
            chart2.Size = new Size(3747, 408);
            chart2.TabIndex = 0;
            // 
            // chart3Panel
            // 
            chart3Panel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            chart3Panel.Controls.Add(comboBox2);
            chart3Panel.Controls.Add(chart3);
            chart3Panel.Location = new Point(0, 1536);
            chart3Panel.Margin = new Padding(6, 8, 6, 8);
            chart3Panel.Name = "chart3Panel";
            chart3Panel.Size = new Size(3747, 316);
            chart3Panel.TabIndex = 2;
            // 
            // comboBox2
            // 
            comboBox2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.FormattingEnabled = true;
            comboBox2.Items.AddRange(new object[] { "外資", "投信", "自營商", "融資", "融券", "KD", "RSI", "MACD" });
            comboBox2.Location = new Point(3495, 28);
            comboBox2.Margin = new Padding(6, 8, 6, 8);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(212, 46);
            comboBox2.TabIndex = 1;
            comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;
            // 
            // chart3
            // 
            chart3.BackColor = Color.Black;
            chart3.BorderlineColor = Color.Transparent;
            chart3.BorderlineWidth = 0;
            chartArea3.AxisX.LabelStyle.Format = "MM/dd";
            chartArea3.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea3.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartArea3.BackColor = Color.Black;
            chartArea3.BorderColor = Color.Transparent;
            chartArea3.BorderWidth = 0;
            chartArea3.Name = "Chart3Area";
            chart3.ChartAreas.Add(chartArea3);
            chart3.Dock = DockStyle.Fill;
            chart3.Location = new Point(0, 0);
            chart3.Margin = new Padding(6, 8, 6, 8);
            chart3.Name = "chart3";
            chart3.Size = new Size(3747, 316);
            chart3.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(18F, 38F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(3747, 2004);
            Controls.Add(chartPanel);
            Controls.Add(topPanel);
            KeyPreview = true;
            Margin = new Padding(6, 8, 6, 8);
            MinimumSize = new Size(2132, 1436);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "股票K線圖分析系統";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load;
            KeyDown += Form1_KeyDown;
            topPanel.ResumeLayout(false);
            topPanel.PerformLayout();
            chartPanel.ResumeLayout(false);
            chart1Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)Kchart).EndInit();
            chart2Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)chart2).EndInit();
            chart3Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)chart3).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private void InitializeChart() // 初始化圖表的自定義方法
        {
            // Designer已經設定好所有參數，這裡只需要清除可能存在的Series
            if (Kchart != null) Kchart.Series.Clear();     // 如果K線圖存在，清除所有數據系列
            if (chart2 != null) chart2.Series.Clear();     // 如果第二個圖表存在，清除所有數據系列
            if (chart3 != null) chart3.Series.Clear();     // 如果第三個圖表存在，清除所有數據系列
        }

        // 控制項宣告區域 - 宣告所有需要在程式中存取的控制項
        private Panel topPanel;         // 頂部控制面板
        private Panel chartPanel;       // 圖表容器面板
        private Panel chart1Panel;      // K線圖容器面板
        private Panel chart2Panel;      // 第二個圖表容器面板
        private Panel chart3Panel;      // 第三個圖表容器面板
        private System.Windows.Forms.DataVisualization.Charting.Chart Kchart;    // K線圖控制項
        private System.Windows.Forms.DataVisualization.Charting.Chart chart2;    // 第二個圖表控制項
        private System.Windows.Forms.DataVisualization.Charting.Chart chart3;    // 第三個圖表控制項
        private Label label1;           // "股票代號:"標籤
        private TextBox textBox1;       // 股票代號輸入框
        private Label labelDays;        // "天數:"標籤
        private Button button2;         // 減少天數按鈕(-)
        private TextBox textBox2;       // 天數輸入框
        private Button button1;         // 增加天數按鈕(+)
        private Button button3;         // 比較選股按鈕
        private Button maSettingsButton; // 均線設定按鈕
        private ComboBox comboBox1;     // 第二個圖表選擇下拉框
        private ComboBox comboBox2;     // 第三個圖表選擇下拉框
        private Label label7;
        private Label label6;
        private Label label5;
        private Label label4;
        private Label label3;
        private Label label2;
        private Button btnPeriod;       // 週期切換按鈕
    }
}