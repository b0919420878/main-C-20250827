// === Form2.cs 完整檔案內容（加上天數控制）===
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WinFormsApp1
{
    public partial class Form2 : Form
    {
        private List<StockData> allStockData1 = new List<StockData>(); // 上方股票完整資料
        private List<StockData> allStockData2 = new List<StockData>(); // 下方股票完整資料
        private string currentStockCode1 = ""; // 上方股票代號
        private string currentStockCode2 = ""; // 下方股票代號
        private int[] movingAveragePeriods = { 5, 10, 20, 60 }; // 均線週期
        private Color[] movingAverageColors = { Color.Orange, Color.Blue, Color.Red, Color.Purple }; // 均線顏色

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            // 載入均線設定
            LoadMovingAverageSettings();
            
            // 設定黑色背景和白色字體
            this.BackColor = Color.Black;
            this.ForeColor = Color.White;
            
            // 設定 Chart 背景為黑色
            SetChartBackgrounds();
        }

        // === 新增：取得顯示資料（根據天數篩選）===
        private List<StockData> GetDisplayData(List<StockData> allData)
        {
            if (allData.Count == 0)
                return allData;

            // 取得要顯示的天數
            if (!int.TryParse(textBoxDays.Text.Trim(), out int dayline) || dayline <= 0)
                return allData; // 如果天數無效，顯示全部

            // 如果要求的天數大於等於總資料數，就回傳全部
            if (dayline >= allData.Count)
                return allData;

            // 取最後 N 天的資料
            return allData.Skip(allData.Count - dayline).ToList();
        }

        // === 天數控制相關事件 ===
        private void buttonPlus_Click(object sender, EventArgs e)
        {
            // 增加 50 天
            if (int.TryParse(textBoxDays.Text.Trim(), out int currentDays))
            {
                int newDays = currentDays + 50;
                textBoxDays.Text = newDays.ToString();
                UpdateBothCharts();
            }
            else
            {
                textBoxDays.Text = "50";
                UpdateBothCharts();
            }
        }

        private void buttonMinus_Click(object sender, EventArgs e)
        {
            // 減少 50 天
            if (int.TryParse(textBoxDays.Text.Trim(), out int currentDays))
            {
                int newDays = 50;
                if (currentDays > 50)
                {
                    newDays = currentDays - 50;
                }
                textBoxDays.Text = newDays.ToString();
                UpdateBothCharts();
            }
            else
            {
                textBoxDays.Text = "50";
                UpdateBothCharts();
            }
        }

        private void textBoxDays_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Enter 鍵：更新圖表
            if (e.KeyChar == (char)Keys.Enter)
            {
                UpdateBothCharts();
                return;
            }

            // +/- 鍵：調整天數
            if (e.KeyChar == '+')
            {
                buttonPlus_Click(sender, e);
                e.Handled = true;
                return;
            }
            if (e.KeyChar == '-')
            {
                buttonMinus_Click(sender, e);
                e.Handled = true;
                return;
            }

            // 只允許數字、退格鍵
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true; // 阻止非數字字符
            }
        }

        // === Form 鍵盤事件（處理 +/- 快捷鍵）===
        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            // 處理 +/- 鍵調整天數
            if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus)
            {
                buttonPlus_Click(sender, e);
                e.Handled = true;
                return;
            }
            else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
            {
                buttonMinus_Click(sender, e);
                e.Handled = true;
                return;
            }
        }

        // === 更新兩個圖表 ===
        private void UpdateBothCharts()
        {
            if (allStockData1.Count > 0)
            {
                var filteredData1 = GetDisplayData(allStockData1);
                DrawCandlestickChart(chart1, filteredData1, currentStockCode1, "Chart1");
            }

            if (allStockData2.Count > 0)
            {
                var filteredData2 = GetDisplayData(allStockData2);
                DrawCandlestickChart(chart2, filteredData2, currentStockCode2, "Chart2");
            }
        }

        // 載入股票資料
        private List<StockData> LoadStockData(string stockCode)
        {
            string filePath = $"D:/stock/txt/{stockCode}.txt";
            var stockDataList = new List<StockData>();

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"找不到檔案: {filePath}");
            }

            string[] lines = File.ReadAllLines(filePath, System.Text.Encoding.UTF8);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split(',').Select(p => p.Trim('"', ' ')).ToArray();

                if (parts.Length >= 6)
                {
                    try
                    {
                        var stockData = new StockData
                        {
                            DateString = parts[0],
                            Open = decimal.Parse(parts[1]),
                            High = decimal.Parse(parts[2]),
                            Low = decimal.Parse(parts[3]),
                            Close = decimal.Parse(parts[4]),
                            Volume = long.Parse(parts[5])
                        };

                        stockDataList.Add(stockData);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"解析錯誤 - 行內容: {line}, 錯誤: {ex.Message}");
                    }
                }
            }

            return stockDataList;
        }

        // 計算移動平均線
        private List<decimal?> CalculateMovingAverage(List<StockData> stockData, int period)
        {
            var movingAverages = new List<decimal?>();

            for (int i = 0; i < stockData.Count; i++)
            {
                if (i < period - 1)
                {
                    movingAverages.Add(null);
                }
                else
                {
                    decimal sum = 0;
                    for (int j = i - period + 1; j <= i; j++)
                    {
                        sum += stockData[j].Close;
                    }
                    movingAverages.Add(sum / period);
                }
            }

            return movingAverages;
        }

        // 繪製K線圖（修改為使用全部資料計算均線）
        private void DrawCandlestickChart(Chart chart, List<StockData> stockData, string stockCode, string chartAreaPrefix)
        {
            try
            {
                chart.Series.Clear();
                chart.Titles.Clear();

                // 添加標題
                chart.Titles.Add($"{stockCode} K線圖");

                // K線序列
                Series candleSeries = new Series("K線")
                {
                    ChartType = SeriesChartType.Candlestick,
                    XValueType = ChartValueType.Double,
                    ChartArea = $"{chartAreaPrefix}StockArea"
                };

                candleSeries["PriceUpColor"] = "Red";
                candleSeries["PriceDownColor"] = "Green";
                candleSeries.BorderWidth = 0;

                // 成交量序列
                Series volumeSeries = new Series("成交量")
                {
                    ChartType = SeriesChartType.Column,
                    XValueType = ChartValueType.Double,
                    ChartArea = $"{chartAreaPrefix}VolumeArea",
                    Color = Color.Blue
                };

                // 計算價格範圍
                decimal dataMin = stockData.Min(x => Math.Min(x.Low, Math.Min(x.Open, Math.Min(x.High, x.Close))));
                decimal dataMax = stockData.Max(x => Math.Max(x.High, Math.Max(x.Open, Math.Max(x.Low, x.Close))));
                decimal range = dataMax - dataMin;
                decimal buffer = range * 0.02m;
                decimal yMin = dataMin - buffer;
                decimal yMax = dataMax + buffer;

                // 加入資料點
                for (int i = 0; i < stockData.Count; i++)
                {
                    var data = stockData[i];

                    // K線資料點
                    DataPoint candlePoint = new DataPoint();
                    candlePoint.XValue = i;
                    candlePoint.AxisLabel = data.DateString;
                    candlePoint.YValues = new double[]
                    {
                        (double)data.High, (double)data.Low, (double)data.Open, (double)data.Close
                    };
                    candleSeries.Points.Add(candlePoint);

                    // 成交量資料點
                    DataPoint volumePoint = new DataPoint();
                    volumePoint.XValue = i;
                    volumePoint.AxisLabel = data.DateString;
                    volumePoint.YValues = new double[] { (double)data.Volume };

                    if (data.Close >= data.Open)
                        volumePoint.Color = Color.Red;
                    else
                        volumePoint.Color = Color.Green;

                    volumeSeries.Points.Add(volumePoint);
                }

                chart.Series.Add(candleSeries);
                chart.Series.Add(volumeSeries);

                // === 添加移動平均線（使用全部資料計算）===
                List<StockData> allData = (chartAreaPrefix == "Chart1") ? allStockData1 : allStockData2;

                for (int maIndex = 0; maIndex < movingAveragePeriods.Length; maIndex++)
                {
                    int period = movingAveragePeriods[maIndex];
                    Color color = movingAverageColors[maIndex % movingAverageColors.Length];

                    // 使用全部資料計算均線
                    var allMovingAverages = CalculateMovingAverage(allData, period);

                    // 計算顯示範圍的起始索引
                    int startIndex = allData.Count - stockData.Count;

                    Series maSeries = new Series($"MA{period}")
                    {
                        ChartType = SeriesChartType.Line,
                        XValueType = ChartValueType.Double,
                        ChartArea = $"{chartAreaPrefix}StockArea",
                        Color = Color.FromArgb(150, color),
                        BorderWidth = 1,
                        IsVisibleInLegend = false
                    };

                    for (int i = 0; i < stockData.Count; i++)
                    {
                        DataPoint maPoint = new DataPoint();
                        maPoint.XValue = i;

                        // 對應到全部資料中的索引
                        int allDataIndex = startIndex + i;

                        if (allDataIndex < allMovingAverages.Count && allMovingAverages[allDataIndex].HasValue)
                        {
                            maPoint.YValues = new double[] { (double)allMovingAverages[allDataIndex].Value };
                            maSeries.Points.Add(maPoint);

                            decimal maValue = allMovingAverages[allDataIndex].Value;
                            if (maValue < dataMin) yMin = maValue - buffer;
                            if (maValue > dataMax) yMax = maValue + buffer;
                        }
                        else
                        {
                            maPoint.IsEmpty = true;
                            maPoint.YValues = new double[] { 0 };
                            maSeries.Points.Add(maPoint);
                        }
                    }

                    chart.Series.Add(maSeries);
                }

                // 設定Y軸範圍
                ChartArea stockArea = chart.ChartAreas[$"{chartAreaPrefix}StockArea"];
                stockArea.AxisY.Minimum = (double)yMin;
                stockArea.AxisY.Maximum = (double)yMax;
                stockArea.AxisY.IsStartedFromZero = false;
                stockArea.AxisY.LabelStyle.Format = "F0";

                ChartArea volumeArea = chart.ChartAreas[$"{chartAreaPrefix}VolumeArea"];
                long volumeMax = stockData.Max(x => x.Volume);
                volumeArea.AxisY.Minimum = 0;
                volumeArea.AxisY.Maximum = volumeMax * 1.1;
                volumeArea.AxisY.IsStartedFromZero = true;
                volumeArea.AxisY.LabelStyle.Format = "F0";

                // 設定X軸標籤
                int dataCount = stockData.Count;
                int labelInterval = Math.Max(1, dataCount / 10);

                stockArea.AxisX.Interval = labelInterval;
                stockArea.AxisX.LabelStyle.Enabled = false;
                stockArea.AxisX.MajorTickMark.Enabled = false;

                volumeArea.AxisX.Interval = labelInterval;
                volumeArea.AxisX.LabelStyle.Enabled = true;
                volumeArea.AxisX.LabelStyle.Angle = -45;
                volumeArea.AxisX.MajorTickMark.Enabled = true;
                volumeArea.AxisX.CustomLabels.Clear();

                for (int i = 0; i < stockData.Count; i += labelInterval)
                {
                    string dateString = stockData[i].DateString;
                    if (dateString.Length >= 6)
                    {
                        string monthDay = dateString.Substring(dateString.Length - 4, 4);
                        string month = monthDay.Substring(0, 2);
                        string day = monthDay.Substring(2, 2);
                        string shortLabel = $"{month}/{day}";
                        CustomLabel label = new CustomLabel(i - 0.5, i + 0.5, shortLabel, 0, LabelMarkStyle.None);
                        volumeArea.AxisX.CustomLabels.Add(label);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"繪製圖表時發生錯誤: {ex.Message}", "錯誤");
            }
        }

        // === 修改載入上方股票（加上天數篩選）===
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Enter 鍵：載入股票資料
            if (e.KeyChar == (char)Keys.Enter)
            {
                string stockCode = textBox1.Text.Trim();
                if (string.IsNullOrEmpty(stockCode))
                {
                    MessageBox.Show("請輸入股票代號", "提示");
                    return;
                }

                try
                {
                    allStockData1 = LoadStockData(stockCode);
                    currentStockCode1 = stockCode;
                    var filteredData = GetDisplayData(allStockData1);
                    DrawCandlestickChart(chart1, filteredData, stockCode, "Chart1");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"載入錯誤: {ex.Message}", "錯誤");
                }
                return;
            }

            // +/- 鍵：調整天數
            if (e.KeyChar == '+')
            {
                buttonPlus_Click(sender, e);
                e.Handled = true;
                return;
            }
            if (e.KeyChar == '-')
            {
                buttonMinus_Click(sender, e);
                e.Handled = true;
                return;
            }

            // 只允許數字、字母、退格鍵
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        // === 修改載入下方股票（加上天數篩選）===
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Enter 鍵：載入股票資料
            if (e.KeyChar == (char)Keys.Enter)
            {
                string stockCode = textBox2.Text.Trim();
                if (string.IsNullOrEmpty(stockCode))
                {
                    MessageBox.Show("請輸入股票代號", "提示");
                    return;
                }

                try
                {
                    allStockData2 = LoadStockData(stockCode);
                    currentStockCode2 = stockCode;
                    var filteredData = GetDisplayData(allStockData2);
                    DrawCandlestickChart(chart2, filteredData, stockCode, "Chart2");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"載入錯誤: {ex.Message}", "錯誤");
                }
                return;
            }

            // +/- 鍵：調整天數
            if (e.KeyChar == '+')
            {
                buttonPlus_Click(sender, e);
                e.Handled = true;
                return;
            }
            if (e.KeyChar == '-')
            {
                buttonMinus_Click(sender, e);
                e.Handled = true;
                return;
            }

            // 只允許數字、字母、退格鍵
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        // 載入均線設定
        private void LoadMovingAverageSettings()
        {
            try
            {
                string settingsPath = Path.Combine(Application.StartupPath, "ma_settings.txt");
                if (File.Exists(settingsPath))
                {
                    string savedPeriods = File.ReadAllText(settingsPath);
                    if (!string.IsNullOrEmpty(savedPeriods))
                    {
                        string[] periodStrings = savedPeriods.Split(',');
                        int[] newPeriods = periodStrings.Select(p => int.Parse(p.Trim())).ToArray();
                        if (newPeriods.All(p => p > 0))
                        {
                            movingAveragePeriods = newPeriods;
                        }
                    }
                }
            }
            catch
            {
                // 保持預設值
            }
        }
        
        // 設定 Chart 背景為黑色
        private void SetChartBackgrounds()
        {
            // Form2 的兩個 Chart
            if (chart1 != null)
            {
                chart1.BackColor = Color.Black;
                foreach (ChartArea area in chart1.ChartAreas)
                {
                    area.BackColor = Color.Black;
                    area.AxisX.LabelStyle.ForeColor = Color.White;
                    area.AxisY.LabelStyle.ForeColor = Color.White;
                    area.AxisX.LineColor = Color.Gray;
                    area.AxisY.LineColor = Color.Gray;
                    area.AxisX.MajorGrid.LineColor = Color.DarkGray;
                    area.AxisY.MajorGrid.LineColor = Color.DarkGray;
                }
            }
            
            if (chart2 != null)
            {
                chart2.BackColor = Color.Black;
                foreach (ChartArea area in chart2.ChartAreas)
                {
                    area.BackColor = Color.Black;
                    area.AxisX.LabelStyle.ForeColor = Color.White;
                    area.AxisY.LabelStyle.ForeColor = Color.White;
                    area.AxisX.LineColor = Color.Gray;
                    area.AxisY.LineColor = Color.Gray;
                    area.AxisX.MajorGrid.LineColor = Color.DarkGray;
                    area.AxisY.MajorGrid.LineColor = Color.DarkGray;
                }
            }
        }
    }
}