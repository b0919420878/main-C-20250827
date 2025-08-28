// === Form2.Designer.cs 完整檔案內容 ===
namespace WinFormsApp1
{
    partial class Form2
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();

            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chart2 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();

            // === 新增天數控制相關控制項 ===
            this.textBoxDays = new System.Windows.Forms.TextBox();
            this.buttonMinus = new System.Windows.Forms.Button();
            this.buttonPlus = new System.Windows.Forms.Button();
            this.labelDays = new System.Windows.Forms.Label();

            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart2)).BeginInit();
            this.SuspendLayout();

            // chart1 (上方圖表) - 修正版本
            // K線區域（占 70% 高度）
            chartArea1.Name = "Chart1StockArea";
            chartArea1.Position.Auto = false;
            chartArea1.Position.Height = 70F;  // 改為 70%
            chartArea1.Position.Width = 100F;
            chartArea1.Position.X = 0F;
            chartArea1.Position.Y = 0F;       // 從頂部開始
            chartArea1.AxisX.LabelStyle.Enabled = false;
            chartArea1.BackColor = System.Drawing.Color.WhiteSmoke;

            // 成交量區域（占 25% 高度，留 5% 間隔）
            chartArea2.Name = "Chart1VolumeArea";
            chartArea2.Position.Auto = false;
            chartArea2.Position.Height = 25F;  // 改為 25%
            chartArea2.Position.Width = 100F;
            chartArea2.Position.X = 0F;
            chartArea2.Position.Y = 75F;      // 從 75% 位置開始
            chartArea2.BackColor = System.Drawing.Color.WhiteSmoke;

            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.ChartAreas.Add(chartArea2);
            this.chart1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.chart1.Location = new System.Drawing.Point(12, 50);
            this.chart1.Name = "chart1";
            this.chart1.Size = new System.Drawing.Size(1000, 280);
            this.chart1.TabIndex = 0;

            // chart2 (下方圖表) - 修正版本
            // K線區域（占 70% 高度）
            chartArea3.Name = "Chart2StockArea";
            chartArea3.Position.Auto = false;
            chartArea3.Position.Height = 70F;  // 改為 70%
            chartArea3.Position.Width = 100F;
            chartArea3.Position.X = 0F;
            chartArea3.Position.Y = 0F;       // 從頂部開始
            chartArea3.AxisX.LabelStyle.Enabled = false;
            chartArea3.BackColor = System.Drawing.Color.WhiteSmoke;

            // 成交量區域（占 25% 高度，留 5% 間隔）
            chartArea4.Name = "Chart2VolumeArea";
            chartArea4.Position.Auto = false;
            chartArea4.Position.Height = 25F;  // 改為 25%
            chartArea4.Position.Width = 100F;
            chartArea4.Position.X = 0F;
            chartArea4.Position.Y = 75F;      // 從 75% 位置開始
            chartArea4.BackColor = System.Drawing.Color.WhiteSmoke;

            this.chart2.ChartAreas.Add(chartArea3);
            this.chart2.ChartAreas.Add(chartArea4);
            this.chart2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.chart2.Location = new System.Drawing.Point(12, 360);
            this.chart2.Name = "chart2";
            this.chart2.Size = new System.Drawing.Size(1000, 280);
            this.chart2.TabIndex = 1;

            // textBox1 (上方股票代號輸入)
            this.textBox1.Location = new System.Drawing.Point(100, 12);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(150, 22);
            this.textBox1.TabIndex = 2;
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);

            // textBox2 (下方股票代號輸入)
            this.textBox2.Location = new System.Drawing.Point(350, 12);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(150, 22);
            this.textBox2.TabIndex = 3;
            this.textBox2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox2_KeyPress);

            // === 天數控制相關控制項 ===
            // labelDays
            this.labelDays.AutoSize = true;
            this.labelDays.Location = new System.Drawing.Point(520, 15);
            this.labelDays.Name = "labelDays";
            this.labelDays.Size = new System.Drawing.Size(44, 15);
            this.labelDays.TabIndex = 6;
            this.labelDays.Text = "天數:";

            // buttonMinus (-)
            this.buttonMinus.Location = new System.Drawing.Point(570, 12);
            this.buttonMinus.Name = "buttonMinus";
            this.buttonMinus.Size = new System.Drawing.Size(25, 25);
            this.buttonMinus.TabIndex = 7;
            this.buttonMinus.Text = "-";
            this.buttonMinus.UseVisualStyleBackColor = true;
            this.buttonMinus.Click += new System.EventHandler(this.buttonMinus_Click);

            // textBoxDays
            this.textBoxDays.Location = new System.Drawing.Point(601, 12);
            this.textBoxDays.Name = "textBoxDays";
            this.textBoxDays.Size = new System.Drawing.Size(50, 22);
            this.textBoxDays.TabIndex = 8;
            this.textBoxDays.Text = "200";
            this.textBoxDays.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxDays_KeyPress);

            // buttonPlus (+)
            this.buttonPlus.Location = new System.Drawing.Point(657, 12);
            this.buttonPlus.Name = "buttonPlus";
            this.buttonPlus.Size = new System.Drawing.Size(25, 25);
            this.buttonPlus.TabIndex = 9;
            this.buttonPlus.Text = "+";
            this.buttonPlus.UseVisualStyleBackColor = true;
            this.buttonPlus.Click += new System.EventHandler(this.buttonPlus_Click);

            // label1
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "上方股票代號:";

            // label2
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(262, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "下方股票代號:";

            // Form2
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 650);
            this.KeyPreview = true; // 允許接收鍵盤事件

            // 添加所有控制項
            this.Controls.Add(this.buttonPlus);
            this.Controls.Add(this.textBoxDays);
            this.Controls.Add(this.buttonMinus);
            this.Controls.Add(this.labelDays);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.chart2);
            this.Controls.Add(this.chart1);

            this.Name = "Form2";
            this.Text = "雙股票K線比較";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form2_KeyDown);

            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;

        // === 新增的天數控制控制項 ===
        private System.Windows.Forms.TextBox textBoxDays;
        private System.Windows.Forms.Button buttonMinus;
        private System.Windows.Forms.Button buttonPlus;
        private System.Windows.Forms.Label labelDays;
    }
}