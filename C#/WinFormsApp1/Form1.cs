using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WinFormsApp1
{
    // *** 新增：週期列舉型別 ***
    public enum PeriodType
    {
        Daily,   // 日線
        Weekly,  // 周線
        Monthly  // 月線
    }

    public partial class Form1 : Form
    {
        private List<StockData> allStockData = new List<StockData>(); // 完整資料

        // *** 新增：法人資料相關變數 ***
        private List<LawData> allLawData = new List<LawData>(); // 完整法人資料
        
        // *** 新增：融資融券資料相關變數 ***
        private List<MarginData> allMarginData = new List<MarginData>(); // 完整融資融券資料
        private string currentStockCode = ""; // 目前載入的股票代號
        private int currentSelectedIndex = -1; // 目前選中的K棒索引
        private bool crosshairEnabled = false; // 十字線是否啟用
        private List<StockData> displayedData = new List<StockData>(); // 目前顯示的資料

        // *** 新增：週期相關變數 ***
        private PeriodType currentPeriod = PeriodType.Daily; // 目前選擇的週期

        // ===== 簡化的繪圖相關變數 =====
        private bool drawingMode = false; // 是否進入畫圖模式
        private bool isDrawingLine = false; // 是否正在畫直線
        private Point lineStartPoint; // 直線起始點
        private List<Point> currentPath = new List<Point>(); // 目前的繪圖路徑
        private List<DrawnObject> drawnObjects = new List<DrawnObject>(); // 所有繪製的物件

        // 畫圖控制項
        private Panel drawingPanel; // 畫圖控制面板
        private ComboBox cboDrawingTool; // 工具選擇
        private Button btnColor; // 顏色選擇
        private Button btnClear; // 清除繪圖

        private int[] movingAveragePeriods = { 5, 10, 20, 60 }; // 均線週期
        private Color[] movingAverageColors = { Color.FromArgb(255, 200, 0), Color.Cyan, Color.FromArgb(255, 120, 120), Color.FromArgb(255, 150, 255) }; // 均線顏色

        public Form1()
        {
            InitializeComponent(); // 這會載入設計器設計的介面
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            
            // 設定黑色背景和白色字體
            this.BackColor = Color.Black;
            this.ForeColor = Color.White;
            
            // 設定 Chart 背景為黑色
            SetChartBackgrounds();
            
            // ===Form載入時初始化===
            InitializeControls();

            // === 載入均線設定 ===
            LoadMovingAverageSettings();

            // *** 新增：載入週期設定 ***
            LoadPeriodSettings();

            // *** 新增：初始化ComboBox預設選項 ***
            if (comboBox1.Items.Count > 0) comboBox1.SelectedIndex = 0; // 預設選擇"成交量"
            if (comboBox2.Items.Count > 0) comboBox2.SelectedIndex = 0; // 預設選擇"外資"
        }

        private void InitializeControls()
        {
            // 創建畫圖控制面板
            CreateDrawingPanel();
        }

        // ===== 新增創建畫圖面板的方法 =====
        private void CreateDrawingPanel()
        {
            // 創建一個面板來放置所有畫圖控制項
            drawingPanel = new Panel();
            drawingPanel.Location = new Point(70, 15); // 放在合理的位置
            drawingPanel.Size = new Size(330, 40);
            drawingPanel.Visible = false; // 初始隱藏

            // 工具選擇下拉選單
            cboDrawingTool = new ComboBox();
            cboDrawingTool.Items.AddRange(new string[] { "直線", "自由繪圖" });
            cboDrawingTool.SelectedIndex = 0;
            cboDrawingTool.DropDownStyle = ComboBoxStyle.DropDownList;
            cboDrawingTool.Location = new Point(5, 3);
            cboDrawingTool.Size = new Size(100, 25);
            drawingPanel.Controls.Add(cboDrawingTool);

            // 顏色按鈕
            btnColor = new Button();
            btnColor.Text = "顏色";
            btnColor.BackColor = Color.Gray;
            btnColor.ForeColor = Color.White;
            btnColor.Location = new Point(115, 2);
            btnColor.Size = new Size(60, 25);
            btnColor.Click += BtnColor_Click;
            drawingPanel.Controls.Add(btnColor);

            // 清除按鈕
            btnClear = new Button();
            btnClear.Text = "清除繪圖";
            btnClear.BackColor = Color.Gray;
            btnClear.ForeColor = Color.White;
            btnClear.Location = new Point(185, 2);
            btnClear.Size = new Size(80, 25);
            btnClear.Click += BtnClear_Click;
            drawingPanel.Controls.Add(btnClear);

            // 加入提示標籤
            Label lblHint = new Label();
            lblHint.Text = "ESC退出";
            lblHint.Location = new Point(275, 6);
            lblHint.Size = new Size(60, 20);
            drawingPanel.Controls.Add(lblHint);

            // 將面板加入表單
            this.Controls.Add(drawingPanel);
            drawingPanel.BringToFront();
        }


        // *** 新增：週期切換按鈕事件 ***
        private void BtnPeriod_Click(object sender, EventArgs e)
        {
            // 循環切換週期
            switch (currentPeriod)
            {
                case PeriodType.Daily:
                    currentPeriod = PeriodType.Weekly;
                    break;
                case PeriodType.Weekly:
                    currentPeriod = PeriodType.Monthly;
                    break;
                case PeriodType.Monthly:
                    currentPeriod = PeriodType.Daily;
                    break;
            }

            // *** 修改：使用統一方法更新按鈕顯示 ***
            UpdatePeriodButtonDisplay();

            // *** 新增：保存週期設定 ***
            SavePeriodSettings();

            // 如果已經有載入資料，重新繪製圖表
            if (allStockData.Count > 0)
            {
                var convertedData = ConvertDataByPeriod(allStockData, currentPeriod);
                var filteredData = GetDisplayDataByPeriod(convertedData);
                DrawAllCharts(filteredData, currentStockCode);
            }
        }

        // *** 修改：原本的繪製邏輯改名為DrawChart1，並新增DrawAllCharts方法 ***
        private void DrawAllCharts(List<StockData> stockData, string stockCode)
        {
            try
            {
                // *** 新增：根據目前週期轉換法人資料 ***
                var convertedLawData = ConvertLawDataByPeriod(allLawData, currentPeriod);
                
                // *** 新增：根據目前週期轉換融資融券資料 ***
                var convertedMarginData = ConvertMarginDataByPeriod(allMarginData, currentPeriod);

                DrawChart1_Candlestick(stockData, stockCode);  // K線圖
                DrawChart2_BySelection(stockData, convertedLawData, convertedMarginData);             // Chart2 根據 comboBox1 選擇
                DrawChart3_BySelection(stockData, convertedLawData, convertedMarginData);             // Chart3 根據 comboBox2 選擇

                // *** 新增：顯示最後一天的資料 ***
                ShowLastDayData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"繪製圖表時發生錯誤: {ex.Message}", "錯誤");
                System.Diagnostics.Debug.WriteLine($"DrawAllCharts錯誤: {ex.Message}");
            }
        }

        // 繪製K線 ***
        private void DrawChart1_Candlestick(List<StockData> stockData, string stockCode)
        {
            try
            {
                // 清除既有的資料
                Kchart.Series.Clear();
                Kchart.Titles.Clear();

                // === 建立K線序列 (僅K線圖區域) ===
                Series candleSeries = new Series("K線")
                {
                    ChartType = SeriesChartType.Candlestick,
                    XValueType = ChartValueType.Double,
                    ChartArea = "StockArea"
                };

                // 設定K線顏色 (台股習慣：紅漲綠跌)
                candleSeries["PriceUpColor"] = "Red";
                candleSeries["PriceDownColor"] = "Green";
                candleSeries.BorderWidth = 1;  // 讓上下影線顯示
                candleSeries["PointWidth"] = "0.8";  // K棒寬度
                
                // 設定上下影線顏色跟隨K棒顏色
                candleSeries["ShowOpenClose"] = "Both";
                candleSeries["OpenCloseStyle"] = "Triangle";

                // 計算所有資料的最高價和最低價
                decimal dataMin = stockData.Min(x => Math.Min(x.Low, Math.Min(x.Open, Math.Min(x.High, x.Close))));
                decimal dataMax = stockData.Max(x => Math.Max(x.High, Math.Max(x.Open, Math.Max(x.Low, x.Close))));

                // 加入一點點緩衝空間（例如2%），避免K線貼到邊緣
                decimal range = dataMax - dataMin;
                decimal buffer = range * 0.02m; // 2%的緩衝空間
                decimal yMin = dataMin - buffer;
                decimal yMax = dataMax + buffer;

                // 加入K線資料點
                for (int i = 0; i < stockData.Count; i++)
                {
                    var data = stockData[i];

                    // K線資料點
                    DataPoint candlePoint = new DataPoint();
                    candlePoint.XValue = i; // 設定 X 軸位置為索引值
                    candlePoint.AxisLabel = data.DateString; // 標籤仍使用日期字串
                    candlePoint.YValues = new double[]
                    {
        (double)data.High, (double)data.Low, (double)data.Open, (double)data.Close
                    };

                    // 顏色設定將在加入序列後統一處理

                    candleSeries.Points.Add(candlePoint);
                }

                // 加入K線序列到圖表
                Kchart.Series.Add(candleSeries);
                
                // 修正上下影線顏色問題
                candleSeries.BorderColor = Color.Empty;  // 清除預設邊框顏色
                
                // 設定每個資料點的影線顏色
                for (int i = 0; i < candleSeries.Points.Count; i++)
                {
                    var point = candleSeries.Points[i];
                    var data = stockData[i];
                    
                    if (data.Close > data.Open)
                    {
                        // 上漲：設定為紅色
                        point.BorderColor = Color.Red;
                        point.Color = Color.Red;
                    }
                    else if (data.Close < data.Open)
                    {
                        // 下跌：設定為綠色
                        point.BorderColor = Color.Green;
                        point.Color = Color.Green;
                    }
                    else
                    {
                        // 平盤：設定為白色
                        point.BorderColor = Color.White;
                        point.Color = Color.White;
                    }
                }

                // === 添加移動平均線（根據週期使用對應資料計算）===
                for (int maIndex = 0; maIndex < movingAveragePeriods.Length; maIndex++)
                {
                    int period = movingAveragePeriods[maIndex];
                    Color color = movingAverageColors[maIndex % movingAverageColors.Length];

                    // *** 修改：根據目前週期選擇適合的資料來計算均線 ***
                    var allConvertedData = ConvertDataByPeriod(allStockData, currentPeriod);
                    var allMovingAverages = CalculateMovingAverage(allConvertedData, period);

                    // 計算顯示範圍的起始索引
                    int startIndex = allConvertedData.Count - stockData.Count;

                    // 創建均線序列
                    Series maSeries = new Series($"MA{period}")
                    {
                        ChartType = SeriesChartType.Line,
                        XValueType = ChartValueType.Double,
                        ChartArea = "StockArea",
                        Color = Color.FromArgb(80, color), // 使用半透明顏色，80是透明度(0-255)
                        BorderWidth = 1,
                        IsVisibleInLegend = false
                    };

                    // 加入均線資料點（只加入顯示範圍內的點）
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

                            // 更新Y軸範圍以包含均線
                            decimal maValue = allMovingAverages[allDataIndex].Value;
                            if (maValue < dataMin) yMin = maValue - buffer;
                            if (maValue > dataMax) yMax = maValue + buffer;
                        }
                        else
                        {
                            // 資料不足時加入空點
                            maPoint.IsEmpty = true;
                            maPoint.YValues = new double[] { 0 };
                            maSeries.Points.Add(maPoint);
                        }
                    }

                    // 將均線加入圖表
                    Kchart.Series.Add(maSeries);
                }

                // === 設定K線圖的Y軸範圍 ===
                ChartArea stockArea = Kchart.ChartAreas["StockArea"];
                stockArea.AxisY.Minimum = (double)yMin;
                stockArea.AxisY.Maximum = (double)yMax;
                stockArea.AxisY.IsStartedFromZero = false;
                stockArea.AxisY.LabelStyle.Format = "F0";

                // *** 修改：簡化標籤間隔計算 ***
                int labelInterval = Math.Max(1, stockData.Count / 6); // 固定顯示6個標籤
                SetupUnifiedXAxis(stockData, stockArea, labelInterval, true); // K線圖不顯示X軸標籤
            }
            catch (Exception ex)
            {
                MessageBox.Show($"繪製K線圖表時發生錯誤: {ex.Message}", "錯誤");
                System.Diagnostics.Debug.WriteLine($"DrawChart1_Candlestick錯誤: {ex.Message}");
            }

            displayedData = stockData.ToList();// 保存目前顯示的資料

            // 如果十字線已啟用，重新繪製
            if (crosshairEnabled && currentSelectedIndex >= 0)
            {
                // 確保索引在有效範圍內
                if (currentSelectedIndex >= displayedData.Count)
                    currentSelectedIndex = displayedData.Count - 1;
                DrawCrosshair();
            }
        }

        // *** 新增：Chart2 繪製邏輯 ***
        private void DrawChart2_BySelection(List<StockData> stockData, List<LawData> lawData = null, List<MarginData> marginData = null)
        {
            string selectedItem = comboBox1.SelectedItem?.ToString() ?? "成交量";

            switch (selectedItem)
            {
                case "成交量":
                    DrawVolumeChart(stockData, chart2, "Chart2Area");
                    break;
                case "外資":
                    DrawLawChart(stockData, "Foreign", chart2, "Chart2Area", lawData);
                    break;
                case "投信":
                    DrawLawChart(stockData, "Investment", chart2, "Chart2Area", lawData);
                    break;
                case "自營商":
                    DrawLawChart(stockData, "Dealer", chart2, "Chart2Area", lawData);
                    break;
                case "融資":
                    DrawMarginChart(stockData, chart2, "Chart2Area", marginData);
                    break;
                case "融券":
                    DrawShortChart(stockData, chart2, "Chart2Area", marginData);
                    break;
                case "KD":
                    DrawKDChart(stockData, chart2, "Chart2Area");
                    break;
                default:
                    DrawVolumeChart(stockData, chart2, "Chart2Area");
                    break;
            }
        }

        // *** 新增：Chart3 繪製邏輯 ***
        private void DrawChart3_BySelection(List<StockData> stockData, List<LawData> lawData = null, List<MarginData> marginData = null)
        {
            string selectedItem = comboBox2.SelectedItem?.ToString() ?? "外資";

            switch (selectedItem)
            {
                case "外資":
                    DrawLawChart(stockData, "Foreign", chart3, "Chart3Area", lawData);
                    break;
                case "投信":
                    DrawLawChart(stockData, "Investment", chart3, "Chart3Area", lawData);
                    break;
                case "自營商":
                    DrawLawChart(stockData, "Dealer", chart3, "Chart3Area", lawData);
                    break;
                case "融資":
                    DrawMarginChart(stockData, chart3, "Chart3Area", marginData);
                    break;
                case "融券":
                    DrawShortChart(stockData, chart3, "Chart3Area", marginData);
                    break;
                case "KD":
                    DrawKDChart(stockData, chart3, "Chart3Area");
                    break;
                case "RSI":
                    // TODO: 實作RSI邏輯
                    break;
                case "MACD":
                    // TODO: 實作MACD邏輯
                    break;
                default:
                    DrawLawChart(stockData, "Foreign", chart3, "Chart3Area", lawData);
                    break;
            }
        }

        // *** 新增：單純成交量繪製方法 ***
        private void DrawVolumeChart(List<StockData> stockData, Chart targetChart, string chartAreaName)
        {
            try
            {
                targetChart.Series.Clear();

                Series volumeSeries = new Series("成交量")
                {
                    ChartType = SeriesChartType.Column,
                    XValueType = ChartValueType.Double,
                    ChartArea = chartAreaName
                };

                // 計算成交量的範圍
                long volumeMax = stockData.Max(x => x.Volume);

                // 加入資料點
                for (int i = 0; i < stockData.Count; i++)
                {
                    var data = stockData[i];
                    DataPoint volumePoint = new DataPoint();
                    volumePoint.XValue = i;
                    volumePoint.AxisLabel = data.DateString;
                    volumePoint.YValues = new double[] { (double)data.Volume };

                    // 根據漲跌設定成交量顏色
                    if (data.Close >= data.Open)
                        volumePoint.Color = Color.Red; // 上漲用紅色
                    else
                        volumePoint.Color = Color.Green; // 下跌用綠色

                    volumeSeries.Points.Add(volumePoint);
                }

                targetChart.Series.Add(volumeSeries);

                // 設定Y軸
                ChartArea area = targetChart.ChartAreas[chartAreaName];
                area.AxisY.Minimum = 0;
                area.AxisY.Maximum = volumeMax * 1.1;
                area.AxisY.IsStartedFromZero = true;
                area.AxisY.LabelStyle.Format = "F0";

                // *** 修改：統一設定X軸對齊 ***
                int labelInterval = Math.Max(1, stockData.Count / 6); // 固定顯示6個標籤
                SetupUnifiedXAxis(stockData, area, labelInterval, false); // 顯示X軸標籤
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DrawVolumeChart錯誤: {ex.Message}");
            }
        }

        // *** 法人(包含外資投信自營商)資料繪製方法 ***
        private void DrawLawChart(List<StockData> stockData, string lawType, Chart targetChart, string chartAreaName, List<LawData> lawData = null)
        {
            try
            {
                if (stockData == null || stockData.Count == 0)
                {
                    targetChart.Series.Clear();
                    return;
                }

                targetChart.Series.Clear();

                // *** 修改：使用傳入的法人資料或預設的完整法人資料 ***
                var useLawData = lawData ?? allLawData;
                var alignedLawData = AlignLawDataWithStock(stockData, useLawData);

                Series lawSeries = new Series($"{lawType}買賣超")
                {
                    ChartType = SeriesChartType.Column,
                    XValueType = ChartValueType.Double,
                    ChartArea = chartAreaName
                };

                double maxValue = 1;  // 預設範圍
                double minValue = -1;
                bool hasValidData = false;

                // 簡單處理：逐一加入資料點
                for (int i = 0; i < stockData.Count; i++)
                {
                    DataPoint point = new DataPoint();
                    point.XValue = i;
                    point.AxisLabel = stockData[i].DateString;

                    // 檢查是否有對應的法人資料
                    if (i < alignedLawData.Count && alignedLawData[i] != null)
                    {
                        long netValue = 0;
                        switch (lawType)
                        {
                            case "Foreign":
                                netValue = alignedLawData[i].ForeignNet;
                                break;
                            case "Investment":
                                netValue = alignedLawData[i].InvestmentNet;
                                break;
                            case "Dealer":
                                netValue = alignedLawData[i].DealerNet;
                                break;
                        }
                        //將Y軸座標數字除以1000
                        double displayValue = (double)netValue / 1000.0;
                        point.YValues = new double[] { displayValue };
                        point.Color = netValue >= 0 ? Color.Red : Color.Green;
                        hasValidData = true;

                        // 更新範圍 (已經是千為單位)
                        if (displayValue > maxValue) maxValue = displayValue;
                        if (displayValue < minValue) minValue = displayValue;
                    }
                    else
                    {
                        // 沒有資料：設為空點
                        point.IsEmpty = true;
                        point.YValues = new double[] { 0 };
                    }

                    lawSeries.Points.Add(point);
                }

                targetChart.Series.Add(lawSeries);

                // 簡單的Y軸設定
                ChartArea area = targetChart.ChartAreas[chartAreaName];
                area.AxisY.LabelStyle.Angle = 0;
                area.AxisY.LabelStyle.Format = "F0";
                if (hasValidData)
                {
                    double range = Math.Max(Math.Abs(maxValue), Math.Abs(minValue));
                    area.AxisY.Minimum = -range * 1.2;
                    area.AxisY.Maximum = range * 1.2;
                }
                else
                {
                    area.AxisY.Minimum = -1.0;
                    area.AxisY.Maximum = 1.0;
                }

                // *** 新增：Y軸標籤格式和單位說明 ***
                area.AxisY.LabelStyle.Format = "F0";
                area.AxisY.LabelStyle.Angle = 0;  // 水平顯示
                area.AxisY.Title = $"{lawType}買賣超 (千股)";  // 加入單位說明

                // 零線
                area.AxisY.StripLines.Clear();
                if (hasValidData)
                {
                    StripLine zeroLine = new StripLine();
                    zeroLine.IntervalOffset = 0;
                    zeroLine.StripWidth = (area.AxisY.Maximum - area.AxisY.Minimum) * 0.001;
                    zeroLine.BackColor = Color.Gray;
                    area.AxisY.StripLines.Add(zeroLine);
                }

                // *** 修改：X軸對齊 ***
                int labelInterval = Math.Max(1, stockData.Count / 6); // 固定顯示6個標籤
                SetupUnifiedXAxis(stockData, area, labelInterval, false);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DrawLawChart錯誤: {ex.Message}");
                targetChart.Series.Clear();
            }
        }

        // *** 新增：融資圖表繪製方法 ***
        private void DrawMarginChart(List<StockData> stockData, Chart targetChart, string chartAreaName, List<MarginData> marginData = null)
        {
            try
            {
                if (stockData == null || stockData.Count == 0)
                {
                    targetChart.Series.Clear();
                    return;
                }

                targetChart.Series.Clear();

                // 使用傳入的融資融券資料或預設的完整融資融券資料
                var useMarginData = marginData ?? allMarginData;
                var alignedMarginData = AlignMarginDataWithStock(stockData, useMarginData);

                // *** 調試信息 ***
                System.Diagnostics.Debug.WriteLine($"DrawMarginChart - stockData筆數: {stockData.Count}, useMarginData筆數: {useMarginData.Count}, alignedMarginData筆數: {alignedMarginData.Count}");
                if (alignedMarginData.Count > 0)
                {
                    var nullCount = alignedMarginData.Count(m => m == null);
                    System.Diagnostics.Debug.WriteLine($"DrawMarginChart - 有效資料: {alignedMarginData.Count - nullCount}, null資料: {nullCount}");
                }

                Series marginSeries = new Series("融資淨額")
                {
                    ChartType = SeriesChartType.Column,
                    XValueType = ChartValueType.Double,
                    ChartArea = chartAreaName
                };

                double maxValue = 1;  // 預設範圍
                double minValue = -1;
                bool hasValidData = false;

                // 加入資料點
                for (int i = 0; i < stockData.Count; i++)
                {
                    DataPoint point = new DataPoint();
                    point.XValue = i;
                    point.AxisLabel = stockData[i].DateString;

                    // 檢查是否有對應的融資資料
                    if (i < alignedMarginData.Count && alignedMarginData[i] != null)
                    {
                        long netValue = alignedMarginData[i].MarginNet; // 融資淨額
                        double displayValue = (double)netValue / 1000; // 將Y軸座標數字除以1000

                        point.YValues = new double[] { displayValue };

                        // 設定顏色：融資淨流入為紅色，淨流出為綠色
                        if (netValue > 0)
                            point.Color = Color.Red;
                        else if (netValue < 0)
                            point.Color = Color.Green;
                        else
                            point.Color = Color.Gray;

                        // 更新範圍
                        if (hasValidData)
                        {
                            maxValue = Math.Max(maxValue, displayValue);
                            minValue = Math.Min(minValue, displayValue);
                        }
                        else
                        {
                            maxValue = displayValue;
                            minValue = displayValue;
                            hasValidData = true;
                        }
                    }
                    else
                    {
                        point.YValues = new double[] { 0 };
                        point.Color = Color.Gray;
                    }

                    marginSeries.Points.Add(point);
                }

                targetChart.Series.Add(marginSeries);

                // 設定Y軸
                ChartArea area = targetChart.ChartAreas[chartAreaName];
                if (hasValidData)
                {
                    double range = Math.Max(Math.Abs(maxValue), Math.Abs(minValue));
                    area.AxisY.Maximum = range * 1.2;
                    area.AxisY.Minimum = -range * 1.2;
                }
                else
                {
                    area.AxisY.Maximum = 1000;
                    area.AxisY.Minimum = -1000;
                }

                area.AxisY.LabelStyle.Format = "F0";
                area.AxisY.IsStartedFromZero = false;

                // 添加零線
                var stripLine = new StripLine();
                stripLine.Interval = 0;
                stripLine.IntervalOffset = 0;
                stripLine.StripWidth = 0;
                stripLine.BorderColor = Color.Black;
                stripLine.BorderWidth = 1;
                area.AxisY.StripLines.Clear();
                area.AxisY.StripLines.Add(stripLine);

                // *** 修改：使用統一的X軸設定方法 ***
                int labelInterval = Math.Max(1, stockData.Count / 6);
                SetupUnifiedXAxis(stockData, area, labelInterval, false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DrawMarginChart錯誤: {ex.Message}");
                targetChart.Series.Clear();
            }
        }

        // *** 新增：融券圖表繪製方法 ***
        private void DrawShortChart(List<StockData> stockData, Chart targetChart, string chartAreaName, List<MarginData> marginData = null)
        {
            try
            {
                if (stockData == null || stockData.Count == 0)
                {
                    targetChart.Series.Clear();
                    return;
                }

                targetChart.Series.Clear();

                // 使用傳入的融資融券資料或預設的完整融資融券資料
                var useMarginData = marginData ?? allMarginData;
                var alignedMarginData = AlignMarginDataWithStock(stockData, useMarginData);

                Series shortSeries = new Series("融券淨額")
                {
                    ChartType = SeriesChartType.Column,
                    XValueType = ChartValueType.Double,
                    ChartArea = chartAreaName
                };

                double maxValue = 1;  // 預設範圍
                double minValue = -1;
                bool hasValidData = false;

                // 加入資料點
                for (int i = 0; i < stockData.Count; i++)
                {
                    DataPoint point = new DataPoint();
                    point.XValue = i;
                    point.AxisLabel = stockData[i].DateString;

                    // 檢查是否有對應的融券資料
                    if (i < alignedMarginData.Count && alignedMarginData[i] != null)
                    {
                        long netValue = alignedMarginData[i].ShortNet; // 融券淨額
                        double displayValue = (double)netValue / 1000; // 將Y軸座標數字除以1000

                        point.YValues = new double[] { displayValue };

                        // 設定顏色：融券淨流入為紅色，淨流出為綠色
                        if (netValue > 0)
                            point.Color = Color.Red;
                        else if (netValue < 0)
                            point.Color = Color.Green;
                        else
                            point.Color = Color.Gray;

                        // 更新範圍
                        if (hasValidData)
                        {
                            maxValue = Math.Max(maxValue, displayValue);
                            minValue = Math.Min(minValue, displayValue);
                        }
                        else
                        {
                            maxValue = displayValue;
                            minValue = displayValue;
                            hasValidData = true;
                        }
                    }
                    else
                    {
                        point.YValues = new double[] { 0 };
                        point.Color = Color.Gray;
                    }

                    shortSeries.Points.Add(point);
                }

                targetChart.Series.Add(shortSeries);

                // 設定Y軸
                ChartArea area = targetChart.ChartAreas[chartAreaName];
                if (hasValidData)
                {
                    double range = Math.Max(Math.Abs(maxValue), Math.Abs(minValue));
                    area.AxisY.Maximum = range * 1.2;
                    area.AxisY.Minimum = -range * 1.2;
                }
                else
                {
                    area.AxisY.Maximum = 1000;
                    area.AxisY.Minimum = -1000;
                }

                area.AxisY.LabelStyle.Format = "F0";
                area.AxisY.IsStartedFromZero = false;

                // 添加零線
                var stripLine = new StripLine();
                stripLine.Interval = 0;
                stripLine.IntervalOffset = 0;
                stripLine.StripWidth = 0;
                stripLine.BorderColor = Color.Black;
                stripLine.BorderWidth = 1;
                area.AxisY.StripLines.Clear();
                area.AxisY.StripLines.Add(stripLine);

                // *** 修改：使用統一的X軸設定方法 ***
                int labelInterval = Math.Max(1, stockData.Count / 6);
                SetupUnifiedXAxis(stockData, area, labelInterval, false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DrawShortChart錯誤: {ex.Message}");
                targetChart.Series.Clear();
            }
        }

        private void DrawKDChart(List<StockData> stockData, Chart targetChart, string chartAreaName)
        {
            try
            {
                targetChart.Series.Clear();

                // *** 修改：根據目前週期計算KD值 ***
                // 注意：這裡的stockData已經是轉換後的週期資料
                var kdData = CalculateKD(stockData, 9); // 使用9個週期(日/周/月)

                // 建立K線序列
                Series kSeries = new Series("K線")
                {
                    ChartType = SeriesChartType.Line,
                    XValueType = ChartValueType.Double,
                    ChartArea = chartAreaName,
                    Color = Color.Red, // 對應VB6的QBColor(12)
                    BorderWidth = 2,
                    IsVisibleInLegend = true
                };

                // 建立D線序列
                Series dSeries = new Series("D線")
                {
                    ChartType = SeriesChartType.Line,
                    XValueType = ChartValueType.Double,
                    ChartArea = chartAreaName,
                    Color = Color.Lime, // 對應VB6的QBColor(10)
                    BorderWidth = 2,
                    IsVisibleInLegend = true
                };

                // 加入資料點
                for (int i = 0; i < stockData.Count; i++)
                {
                    // K線資料點
                    DataPoint kPoint = new DataPoint();
                    kPoint.XValue = i;
                    kPoint.AxisLabel = stockData[i].DateString;
                    kPoint.YValues = new double[] { (double)kdData[i].K };
                    kSeries.Points.Add(kPoint);

                    // D線資料點
                    DataPoint dPoint = new DataPoint();
                    dPoint.XValue = i;
                    dPoint.AxisLabel = stockData[i].DateString;
                    dPoint.YValues = new double[] { (double)kdData[i].D };
                    dSeries.Points.Add(dPoint);
                }

                targetChart.Series.Add(kSeries);
                targetChart.Series.Add(dSeries);

                // 設定Y軸
                ChartArea area = targetChart.ChartAreas[chartAreaName];
                area.AxisY.Minimum = 0;
                area.AxisY.Maximum = 100;
                area.AxisY.IsStartedFromZero = true;
                area.AxisY.LabelStyle.Format = "F0";
                area.AxisY.Title = "KD值";

                // 加入超買超賣線
                area.AxisY.StripLines.Clear();

                // 超買線(80)
                StripLine overboughtLine = new StripLine();
                overboughtLine.IntervalOffset = 80;
                overboughtLine.StripWidth = 0.5;
                overboughtLine.BackColor = Color.FromArgb(50, Color.Red);
                area.AxisY.StripLines.Add(overboughtLine);

                // 超賣線(20)
                StripLine oversoldLine = new StripLine();
                oversoldLine.IntervalOffset = 20;
                oversoldLine.StripWidth = 0.5;
                oversoldLine.BackColor = Color.FromArgb(50, Color.Green);
                area.AxisY.StripLines.Add(oversoldLine);

                // 中線(50)
                StripLine midLine = new StripLine();
                midLine.IntervalOffset = 50;
                midLine.StripWidth = 0.3;
                midLine.BackColor = Color.FromArgb(30, Color.Gray);
                area.AxisY.StripLines.Add(midLine);

                // *** 修改：X軸對齊 ***
                int labelInterval = Math.Max(1, stockData.Count / 6); // 固定顯示6個標籤
                SetupUnifiedXAxis(stockData, area, labelInterval, false);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DrawKDChart錯誤: {ex.Message}");
                targetChart.Series.Clear();
            }
        }


        // *** 新增：統一的X軸設定方法 ***
        private void SetupUnifiedXAxis(List<StockData> stockData, ChartArea area, int labelInterval, bool showLabels)
        {
            try
            {
                // *** 新增：防護條件 - 檢查資料是否有效 ***
                if (stockData == null || stockData.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("警告：stockData 為空，跳過 X 軸設定");
                    return;
                }

                // *** 新增：確保 labelInterval 有效 ***
                if (labelInterval <= 0)
                {
                    labelInterval = Math.Max(1, stockData.Count / 8);
                    if (labelInterval <= 0) labelInterval = 1;
                }

                // *** 確保 ChartArea Position 一致 ***
                area.Position.Auto = false;
                area.Position.Height = 100F;
                area.Position.Width = 95F;     // 稍微縮小以防止爆出
                area.Position.X = 2.5F;        // 居中對齊
                area.Position.Y = 3F;

                // *** 設定 InnerPlotPosition 確保繪圖區域對齊 ***
                area.InnerPlotPosition.Auto = false;
                area.InnerPlotPosition.Height = 90F;
                area.InnerPlotPosition.Width = 95F;   // 配合 ChartArea 寬度
                area.InnerPlotPosition.X = 2.5F;      // 留一點邊緣
                area.InnerPlotPosition.Y = 2F;

                // *** 新增：防護條件 - 確保軸範圍有效 ***
                double minValue = -0.5;
                double maxValue = Math.Max(minValue + 1, stockData.Count - 0.5); // 確保 Max > Min

                // 設定X軸範圍和間隔
                area.AxisX.Minimum = minValue;
                area.AxisX.Maximum = maxValue;

                // *** 新增：防護條件 - 確保間隔不會太大 ***
                if (labelInterval > stockData.Count)
                {
                    labelInterval = Math.Max(1, stockData.Count);
                }

                area.AxisX.Interval = labelInterval;
                area.AxisX.LabelStyle.Enabled = showLabels;
                area.AxisX.LabelStyle.Angle = -1;
                area.AxisX.MajorTickMark.Enabled = showLabels;
                area.AxisX.CustomLabels.Clear();

                if (showLabels && stockData.Count > 0)
                {
                    // 簡化的標籤顯示策略 - 固定間隔
                    for (int i = 0; i < stockData.Count; i += labelInterval)
                    {
                        if (i >= stockData.Count) break;

                        string dateString = stockData[i].DateString;
                        if (!string.IsNullOrEmpty(dateString) && dateString.Length >= 6)
                        {
                            try
                            {
                                string shortLabel = FormatDateLabel(dateString, currentPeriod);
                                
                                // 設定 CustomLabel 的範圍，讓文字精確對齊在數據點上
                                // 使用合適的範圍，讓文字在資料點 i 的位置顯示
                                double startPos = i - 5.0;
                                double endPos = i + 5.0;
                                
                                CustomLabel label = new CustomLabel(startPos, endPos, shortLabel, 0, LabelMarkStyle.None);
                                area.AxisX.CustomLabels.Add(label);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"處理日期標籤錯誤: {dateString}, 錯誤: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetupUnifiedXAxis 錯誤: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"資料筆數: {stockData?.Count ?? 0}, labelInterval: {labelInterval}");
            }
        }
        
        // 設定 Chart 背景為黑色
        private void SetChartBackgrounds()
        {
            // Form1 的三個 Chart
            if (Kchart != null)
            {
                Kchart.BackColor = Color.Black;
                foreach (ChartArea area in Kchart.ChartAreas)
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
            
            if (chart3 != null)
            {
                chart3.BackColor = Color.Black;
                foreach (ChartArea area in chart3.ChartAreas)
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
        
        private List<LawData> AlignLawDataWithStock(List<StockData> stockData, List<LawData> allLawData)
        {
            var alignedData = new List<LawData>();

            // 簡單對齊：對每個股票日期，找對應的法人資料
            foreach (var stock in stockData)
            {
                var lawData = allLawData.FirstOrDefault(l => l.DateString == stock.DateString);
                alignedData.Add(lawData); // 找到就加入，找不到就加入 null
            }

            return alignedData;
        }

        // *** 日期對齊驗證方法 ***
        private void VerifyDataAlignment(List<StockData> stockData, List<LawData> alignedLawData)
        {
            if (stockData.Count != alignedLawData.Count)
            {
                System.Diagnostics.Debug.WriteLine($"嚴重錯誤：資料筆數不匹配！股票:{stockData.Count}, 法人:{alignedLawData.Count}");
                return;
            }

            int mismatchCount = 0;
            int nullCount = 0;
            for (int i = 0; i < Math.Min(stockData.Count, alignedLawData.Count); i++)
            {
                if (alignedLawData[i] == null)
                {
                    nullCount++;
                    continue; // null 是正常的，代表該日期沒有法人資料
                }
                if (stockData[i].DateString != alignedLawData[i].DateString)
                {
                    mismatchCount++;
                    if (mismatchCount <= 3) // 只顯示前3個不匹配的
                    {
                        System.Diagnostics.Debug.WriteLine($"日期不匹配 索引{i}: 股票={stockData[i].DateString}, 法人={alignedLawData[i].DateString}");
                    }
                }
            }

            if (mismatchCount == 0)
            {
                System.Diagnostics.Debug.WriteLine($"✓ 日期對齊驗證通過：所有日期都正確對齊，其中 {nullCount} 天沒有法人資料");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"⚠ 發現 {mismatchCount} 個日期不匹配的項目，{nullCount} 天沒有法人資料");
            }
        }

        // *** 新增：載入法人資料方法 ***
        private List<LawData> LoadLawData(string stockCode)
        {
            string filePath = $"D:/stock/law/{stockCode}.law";
            var lawDataList = new List<LawData>();

            if (!File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine($"找不到法人資料檔案: {filePath}");
                return lawDataList;
            }

            try
            {
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

                // 用字典來暫存每個日期的法人資料
                var dailyLawData = new Dictionary<string, LawData>();

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split(',').Select(p => p.Trim('"', ' ')).ToArray();

                    // 格式：日期, 買進, 賣出, 法人類型(1=外資, 2=投信, 3=自營商)
                    if (parts.Length >= 4)
                    {
                        try
                        {
                            string dateString = parts[0];
                            long buyAmount = long.Parse(parts[1]);
                            long sellAmount = long.Parse(parts[2]);
                            int lawType = int.Parse(parts[3]);

                            // 如果這個日期還沒有資料，先建立空的法人資料
                            if (!dailyLawData.ContainsKey(dateString))
                            {
                                dailyLawData[dateString] = new LawData
                                {
                                    DateString = dateString,
                                    ForeignBuy = 0,
                                    ForeignSell = 0,
                                    InvestmentBuy = 0,
                                    InvestmentSell = 0,
                                    DealerBuy = 0,
                                    DealerSell = 0
                                };
                            }

                            // 根據法人類型填入對應的買賣數據
                            var lawData = dailyLawData[dateString];
                            switch (lawType)
                            {
                                case 1: // 外資
                                    lawData.ForeignBuy = buyAmount;
                                    lawData.ForeignSell = sellAmount;
                                    break;
                                case 2: // 投信
                                    lawData.InvestmentBuy = buyAmount;
                                    lawData.InvestmentSell = sellAmount;
                                    break;
                                case 3: // 自營商
                                    lawData.DealerBuy = buyAmount;
                                    lawData.DealerSell = sellAmount;
                                    break;
                                default:
                                    System.Diagnostics.Debug.WriteLine($"未知的法人類型: {lawType} 在行: {line}");
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"解析法人資料錯誤 - 行內容: {line}, 錯誤: {ex.Message}");
                        }
                    }
                }

                // 將字典轉換為List，並按日期排序
                lawDataList = dailyLawData.Values
                    .OrderBy(x => x.DateString)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"成功載入 {lawDataList.Count} 天的法人資料");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"讀取法人資料檔案錯誤: {ex.Message}");
            }

            return lawDataList;
        }

        // *** 新增：載入融資融券資料方法 ***
        private List<MarginData> LoadMarginData(string stockCode)
        {
            string filePath = $"D:/stock/inv/{stockCode}.inv";
            var marginDataList = new List<MarginData>();

            if (!File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine($"找不到融資融券資料檔案: {filePath}");
                return marginDataList;
            }

            try
            {
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split(',').Select(p => p.Trim('"', ' ')).ToArray();

                    // 格式：日期 融資加 融資減 融資餘額 融券加 融券減 融券餘額
                    if (parts.Length >= 7)
                    {
                        try
                        {
                            var marginData = new MarginData
                            {
                                DateString = parts[0],
                                MarginBuy = long.Parse(parts[1]),    // 融資加
                                MarginSell = long.Parse(parts[2]),   // 融資減
                                MarginBalance = long.Parse(parts[3]), // 融資餘額
                                ShortBuy = long.Parse(parts[4]),     // 融券加
                                ShortSell = long.Parse(parts[5]),    // 融券減
                                ShortBalance = long.Parse(parts[6])  // 融券餘額
                            };

                            marginDataList.Add(marginData);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"解析融資融券資料錯誤 - 行內容: {line}, 錯誤: {ex.Message}");
                        }
                    }
                }

                // 按日期排序
                marginDataList = marginDataList.OrderBy(x => x.DateString).ToList();

                System.Diagnostics.Debug.WriteLine($"成功載入 {marginDataList.Count} 天的融資融券資料");
                if (marginDataList.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"第一筆融資融券資料: {marginDataList.First()}");
                    System.Diagnostics.Debug.WriteLine($"最後一筆融資融券資料: {marginDataList.Last()}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"讀取融資融券資料檔案錯誤: {ex.Message}");
            }

            return marginDataList;
        }

        // *** 新增：融資融券資料與股價資料對齊方法 ***
        private List<MarginData> AlignMarginDataWithStock(List<StockData> stockData, List<MarginData> allMarginData)
        {
            var alignedData = new List<MarginData>();

            // 簡單對齊：對每個股票日期，找對應的融資融券資料
            foreach (var stock in stockData)
            {
                var marginData = allMarginData.FirstOrDefault(m => m.DateString == stock.DateString);
                alignedData.Add(marginData); // 找到就加入，找不到就加入 null
            }

            return alignedData;
        }

        // *** 新增：根據週期轉換資料 ***
        private List<StockData> ConvertDataByPeriod(List<StockData> dailyData, PeriodType period)
        {
            if (period == PeriodType.Daily)
                return dailyData; // 日線直接回傳

            if (dailyData == null || dailyData.Count == 0)
                return new List<StockData>();

            switch (period)
            {
                case PeriodType.Weekly:
                    return ConvertToWeeklyData(dailyData);
                case PeriodType.Monthly:
                    return ConvertToMonthlyData(dailyData);
                default:
                    return dailyData;
            }
        }

        // *** 新增：日線轉周線 (每5個交易日為一組) ***
        private List<StockData> ConvertToWeeklyData(List<StockData> dailyData)
        {
            try
            {
                var weeklyData = new List<StockData>();
                int groupSize = 5; // 5個交易日為一周

                for (int i = 0; i < dailyData.Count; i += groupSize)
                {
                    // 取得這一組的資料（最多5天）
                    var group = dailyData.Skip(i).Take(groupSize).ToList();
                    if (group.Count == 0) break;

                    // 創建周K線資料
                    var weekData = new StockData
                    {
                        DateString = group.First().DateString,   // 該組第一天日期
                        Open = group.First().Open,               // 該組第一天開盤價
                        Close = group.Last().Close,              // 該組最後一天收盤價
                        High = group.Max(d => d.High),           // 該組最高價
                        Low = group.Min(d => d.Low),             // 該組最低價
                        Volume = group.Sum(d => d.Volume)        // 該組總成交量
                    };

                    weeklyData.Add(weekData);
                }

                System.Diagnostics.Debug.WriteLine($"日線轉周線：{dailyData.Count} -> {weeklyData.Count} (每{groupSize}天一組)");
                return weeklyData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"日線轉周線錯誤: {ex.Message}");
                return dailyData; // 出錯時回傳原始資料
            }
        }

        // *** 新增：日線轉月線 (每20個交易日為一組) ***
        private List<StockData> ConvertToMonthlyData(List<StockData> dailyData)
        {
            try
            {
                var monthlyData = new List<StockData>();
                int groupSize = 20; // 20個交易日為一月

                for (int i = 0; i < dailyData.Count; i += groupSize)
                {
                    // 取得這一組的資料（最多20天）
                    var group = dailyData.Skip(i).Take(groupSize).ToList();
                    if (group.Count == 0) break;

                    // 創建月K線資料
                    var monthData = new StockData
                    {
                        DateString = group.First().DateString,   // 該組第一天日期
                        Open = group.First().Open,               // 該組第一天開盤價
                        Close = group.Last().Close,              // 該組最後一天收盤價
                        High = group.Max(d => d.High),           // 該組最高價
                        Low = group.Min(d => d.Low),             // 該組最低價
                        Volume = group.Sum(d => d.Volume)        // 該組總成交量
                    };

                    monthlyData.Add(monthData);
                }

                System.Diagnostics.Debug.WriteLine($"日線轉月線：{dailyData.Count} -> {monthlyData.Count} (每{groupSize}天一組)");
                return monthlyData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"日線轉月線錯誤: {ex.Message}");
                return dailyData; // 出錯時回傳原始資料
            }
        }


        // *** 新增：根據週期取得顯示資料 ***
        private List<StockData> GetDisplayDataByPeriod(List<StockData> convertedData)
        {
            if (convertedData.Count == 0)
                return convertedData;

            // 取得要顯示的天數/周數/月數
            if (!int.TryParse(textBox2.Text.Trim(), out int displayCount) || displayCount <= 0)
                return convertedData; // 如果天數無效，顯示全部

            // 如果要求的數量大於等於總資料數，就回傳全部
            if (displayCount >= convertedData.Count)
                return convertedData;

            // 取最後 N 個週期的資料
            return convertedData.Skip(convertedData.Count - displayCount).ToList();
        }

        // *** 新增：法人資料週期轉換 (簡單分組) ***
        private List<LawData> ConvertLawDataByPeriod(List<LawData> dailyLawData, PeriodType period)
        {
            if (period == PeriodType.Daily || dailyLawData == null || dailyLawData.Count == 0)
                return dailyLawData;

            try
            {
                var convertedLawData = new List<LawData>();
                int groupSize = period == PeriodType.Weekly ? 5 : 20; // 周線5天，月線20天

                for (int i = 0; i < dailyLawData.Count; i += groupSize)
                {
                    // 取得這一組的資料
                    var group = dailyLawData.Skip(i).Take(groupSize).Where(d => d != null).ToList();
                    if (group.Count == 0) continue;

                    // 創建合併的法人資料
                    var groupData = new LawData
                    {
                        DateString = group.First().DateString, // 該組第一天日期
                        ForeignBuy = group.Sum(d => d.ForeignBuy),
                        ForeignSell = group.Sum(d => d.ForeignSell),
                        InvestmentBuy = group.Sum(d => d.InvestmentBuy),
                        InvestmentSell = group.Sum(d => d.InvestmentSell),
                        DealerBuy = group.Sum(d => d.DealerBuy),
                        DealerSell = group.Sum(d => d.DealerSell)
                    };

                    convertedLawData.Add(groupData);
                }

                System.Diagnostics.Debug.WriteLine($"法人資料轉換 {period}：{dailyLawData.Count} -> {convertedLawData.Count} (每{groupSize}天一組)");
                return convertedLawData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"法人資料週期轉換錯誤: {ex.Message}");
                return dailyLawData; // 出錯時回傳原始資料
            }
        }

        // *** 新增：融資融券資料週期轉換方法 ***
        private List<MarginData> ConvertMarginDataByPeriod(List<MarginData> dailyMarginData, PeriodType period)
        {
            if (period == PeriodType.Daily || dailyMarginData == null || dailyMarginData.Count == 0)
                return dailyMarginData;

            try
            {
                var convertedMarginData = new List<MarginData>();
                int groupSize = period == PeriodType.Weekly ? 5 : 20; // 周線5天，月線20天

                for (int i = 0; i < dailyMarginData.Count; i += groupSize)
                {
                    // 取得這一組的資料
                    var group = dailyMarginData.Skip(i).Take(groupSize).Where(d => d != null).ToList();
                    if (group.Count == 0) continue;

                    // 創建合併的融資融券資料
                    var groupData = new MarginData
                    {
                        DateString = group.First().DateString, // 該組第一天日期
                        MarginBuy = group.Sum(d => d.MarginBuy),
                        MarginSell = group.Sum(d => d.MarginSell),
                        MarginBalance = group.Last().MarginBalance, // 使用最後一天的餘額
                        ShortBuy = group.Sum(d => d.ShortBuy),
                        ShortSell = group.Sum(d => d.ShortSell),
                        ShortBalance = group.Last().ShortBalance // 使用最後一天的餘額
                    };

                    convertedMarginData.Add(groupData);
                }

                System.Diagnostics.Debug.WriteLine($"融資融券資料轉換 {period}：{dailyMarginData.Count} -> {convertedMarginData.Count} (每{groupSize}天一組)");
                if (convertedMarginData.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"轉換後第一筆: {convertedMarginData.First()}");
                }
                return convertedMarginData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"融資融券資料週期轉換錯誤: {ex.Message}");
                return dailyMarginData; // 出錯時回傳原始資料
            }
        }

        private List<StockData> LoadStockData(string stockCode)
        {
            string filePath = $"D:/stock/txt/{stockCode}.txt";
            var stockDataList = new List<StockData>();

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"找不到檔案: {filePath}");
            }

            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

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
                            DateString = parts[0],  // 直接用字串，如 "1140424"
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
            allStockData = stockDataList; // 儲存完整資料
            currentStockCode = stockCode; // 記錄股票代號

            return stockDataList;
        }

        private List<StockData> GetDisplayData()
        {
            if (allStockData.Count == 0)
                return allStockData;

            // 取得要顯示的天數
            if (!int.TryParse(textBox2.Text.Trim(), out int dayline) || dayline <= 0)
                return allStockData; // 如果天數無效，顯示全部

            // 如果要求的天數大於等於總資料數，就回傳全部
            if (dayline >= allStockData.Count)
                return allStockData;

            // 取最後 N 天的資料
            return allStockData.Skip(allStockData.Count - dayline).ToList();
        }

        // *** 修改：textBox1事件處理，加入法人資料載入 ***
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Enter 鍵：載入股票資料
            if (e.KeyChar == (char)Keys.Enter)
            {
                string stockCode = textBox1.Text.Trim();

                if (string.IsNullOrEmpty(stockCode))
                {
                    MessageBox.Show("請輸入股票代號", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    var stockData = LoadStockData(stockCode);

                    // *** 新增：載入法人資料 ***
                    allLawData = LoadLawData(stockCode);
                    
                    // *** 新增：載入融資融券資料 ***
                    allMarginData = LoadMarginData(stockCode);

                    if (stockData.Count > 0)
                    {
                        var filteredData = GetDisplayData();
                        // *** 修改：使用新的DrawAllCharts方法 ***
                        DrawAllCharts(filteredData, stockCode);
                    }
                    else
                    {
                        MessageBox.Show($"找不到股票代號 {stockCode} 的資料檔案", "載入失敗",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"載入錯誤: {ex.Message}", "錯誤",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            // +/- 鍵：調整天數
            if (e.KeyChar == '+')
            {
                button1_Click(sender, e);
                e.Handled = true;
                return;
            }
            if (e.KeyChar == '-')
            {
                button2_Click(sender, e);
                e.Handled = true;
                return;
            }

            // 只允許數字、字母、退格鍵
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true; // 阻止其他字符
            }
        }

        // *** 修改：textBox2事件處理，保持當前週期 ***
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Enter 鍵：更新圖表
            if (e.KeyChar == (char)Keys.Enter)
            {
                // 如果已經有載入資料，就重新顯示
                if (allStockData.Count > 0)
                {
                    var convertedData = ConvertDataByPeriod(allStockData, currentPeriod);
                    var filteredData = GetDisplayDataByPeriod(convertedData);
                    DrawAllCharts(filteredData, currentStockCode);
                }
                return;
            }

            // +/- 鍵：調整天數
            if (e.KeyChar == '+')
            {
                button1_Click(sender, e);
                e.Handled = true;
                return;
            }
            if (e.KeyChar == '-')
            {
                button2_Click(sender, e);
                e.Handled = true;
                return;
            }

            // 只允許數字、退格鍵
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true; // 阻止非數字字符
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 讀取目前 textBox2 的值
            if (int.TryParse(textBox2.Text.Trim(), out int currentDays))
            {
                // 加上 50
                int newDays = currentDays + 50;

                // 更新 textBox2 的文字
                textBox2.Text = newDays.ToString();

                // *** 修改：直接更新圖表，保持當前週期 ***
                if (allStockData.Count > 0)
                {
                    var convertedData = ConvertDataByPeriod(allStockData, currentPeriod);
                    var filteredData = GetDisplayDataByPeriod(convertedData);
                    DrawAllCharts(filteredData, currentStockCode);
                }
            }
            else
            {
                // 如果 textBox2 沒有有效數字，預設設為 50
                textBox2.Text = "50";

                // *** 修改：直接更新圖表，保持當前週期 ***
                if (allStockData.Count > 0)
                {
                    var convertedData = ConvertDataByPeriod(allStockData, currentPeriod);
                    var filteredData = GetDisplayDataByPeriod(convertedData);
                    DrawAllCharts(filteredData, currentStockCode);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 讀取目前 textBox2 的值
            if (int.TryParse(textBox2.Text.Trim(), out int currentDays))
            {
                int newDays = 50;

                // 減掉 50，但確保不會小於 1
                if (currentDays > 50)
                {
                    newDays = currentDays - 50;
                }
                // 更新 textBox2 的文字
                textBox2.Text = newDays.ToString();

                // *** 修改：直接更新圖表，保持當前週期 ***
                if (allStockData.Count > 0)
                {
                    var convertedData = ConvertDataByPeriod(allStockData, currentPeriod);
                    var filteredData = GetDisplayDataByPeriod(convertedData);
                    DrawAllCharts(filteredData, currentStockCode);
                }
            }
            else
            {
                // 如果 textBox2 沒有有效數字，預設設為 50
                textBox2.Text = "50";

                // *** 修改：直接更新圖表，保持當前週期 ***
                if (allStockData.Count > 0)
                {
                    var convertedData = ConvertDataByPeriod(allStockData, currentPeriod);
                    var filteredData = GetDisplayDataByPeriod(convertedData);
                    DrawAllCharts(filteredData, currentStockCode);
                }
            }
        }

        // *** 新增：ComboBox1 事件處理 ***
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (displayedData.Count > 0)
            {
                var convertedLawData = ConvertLawDataByPeriod(allLawData, currentPeriod);
                var convertedMarginData = ConvertMarginDataByPeriod(allMarginData, currentPeriod);
                DrawChart2_BySelection(displayedData, convertedLawData, convertedMarginData);
                // *** X軸對齊在繪製時立即執行，不需要額外調用 ***
            }
        }

        // *** 新增：ComboBox2 事件處理 ***
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (displayedData.Count > 0)
            {
                var convertedLawData = ConvertLawDataByPeriod(allLawData, currentPeriod);
                var convertedMarginData = ConvertMarginDataByPeriod(allMarginData, currentPeriod);
                DrawChart3_BySelection(displayedData, convertedLawData, convertedMarginData);
                // *** X軸對齊在繪製時立即執行，不需要額外調用 ***
            }
        }

        // ===== Form1_KeyDown 修改版 =====
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // ESC 鍵處理
            if (e.KeyCode == Keys.Escape)
            {
                if (drawingMode)
                {
                    // 退出畫圖模式
                    drawingMode = false;
                    drawingPanel.Visible = false;
                    Kchart.Cursor = Cursors.Default;
                    isDrawingLine = false;
                    currentPath.Clear();
                    Kchart.Invalidate();
                    e.Handled = true;
                    return;
                }
                else if (crosshairEnabled)
                {
                    // 關閉十字線
                    crosshairEnabled = false;
                    currentSelectedIndex = -1;
                    ClearCrosshair();
                    e.Handled = true;
                    return;
                }
            }

            // === 新增：處理 +/- 鍵調整天數 ===
            if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus) // + 鍵（數字鍵盤或主鍵盤）
            {
                button1_Click(sender, e); // 呼叫增加天數的功能
                e.Handled = true;
                return;
            }
            else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus) // - 鍵（數字鍵盤或主鍵盤）
            {
                button2_Click(sender, e); // 呼叫減少天數的功能
                e.Handled = true;
                return;
            }

            // 在畫圖模式下，只處理ESC鍵退出畫圖模式
            if (drawingMode)
            {
                return; // 畫圖模式下不處理其他鍵盤事件
            }

            // 非畫圖模式下的鍵盤處理
            // 只有在有資料的情況下才處理十字線
            if (displayedData == null || displayedData.Count == 0)
                return;

            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                // 啟用十字線
                if (!crosshairEnabled)
                {
                    crosshairEnabled = true;
                    // 預設選擇最後一根K棒（最新的）
                    currentSelectedIndex = displayedData.Count - 1;
                }
                else
                {
                    // 移動選擇
                    if (e.KeyCode == Keys.Left && currentSelectedIndex > 0)
                    {
                        currentSelectedIndex--;
                    }
                    else if (e.KeyCode == Keys.Right && currentSelectedIndex < displayedData.Count - 1)
                    {
                        currentSelectedIndex++;
                    }
                }

                // 繪製十字線
                DrawCrosshair();
                e.Handled = true;
            }
        }

        private void DrawCrosshair()
        {
            if (currentSelectedIndex < 0 || currentSelectedIndex >= displayedData.Count)
                return;

            // 清除之前的標註
            ClearCrosshair();

            var selectedData = displayedData[currentSelectedIndex];

            try
            {
                // === 在所有三個圖表繪製垂直線 ===
                // K線圖
                ChartArea stockArea = Kchart.ChartAreas["StockArea"];
                StripLine verticalLineStock = new StripLine();
                verticalLineStock.Interval = 0;
                verticalLineStock.IntervalOffset = currentSelectedIndex;
                verticalLineStock.StripWidth = 0.03;
                verticalLineStock.BackColor = Color.Yellow;
                stockArea.AxisX.StripLines.Add(verticalLineStock);

                // Chart2
                if (chart2.ChartAreas.Count > 0)
                {
                    ChartArea chart2Area = chart2.ChartAreas["Chart2Area"];
                    StripLine verticalLineChart2 = new StripLine();
                    verticalLineChart2.Interval = 0;
                    verticalLineChart2.IntervalOffset = currentSelectedIndex;
                    verticalLineChart2.StripWidth = 0.03;
                    verticalLineChart2.BackColor = Color.Yellow;
                    chart2Area.AxisX.StripLines.Add(verticalLineChart2);
                }

                // Chart3
                if (chart3.ChartAreas.Count > 0)
                {
                    ChartArea chart3Area = chart3.ChartAreas["Chart3Area"];
                    StripLine verticalLineChart3 = new StripLine();
                    verticalLineChart3.Interval = 0;
                    verticalLineChart3.IntervalOffset = currentSelectedIndex;
                    verticalLineChart3.StripWidth = 0.03;
                    verticalLineChart3.BackColor = Color.Yellow;
                    chart3Area.AxisX.StripLines.Add(verticalLineChart3);
                }

                // === 繪製水平線（在K線圖的收盤價位置） ===
                StripLine horizontalLine = new StripLine();
                horizontalLine.Interval = 0;
                horizontalLine.IntervalOffset = (double)selectedData.Close;
                horizontalLine.StripWidth = (stockArea.AxisY.Maximum - stockArea.AxisY.Minimum) * 0.001;
                horizontalLine.BackColor = Color.Yellow;
                stockArea.AxisY.StripLines.Add(horizontalLine);

                // === 更新 Label2~Label7 顯示資料 ===
                UpdateDataLabels(selectedData);

                // === 原本的資訊標籤（可選擇保留或移除） ===
                // 清除舊的標註
                foreach (var annotation in Kchart.Annotations.ToList())
                {
                    if (annotation.Name.StartsWith("CrosshairInfo"))
                        Kchart.Annotations.Remove(annotation);
                }

                // 創建資訊標籤（如果不想要圖表上的標籤，可以註解掉這段）
                TextAnnotation infoAnnotation = new TextAnnotation();
                infoAnnotation.Name = "CrosshairInfo";
                infoAnnotation.Text = $"日期: {FormatDateString(selectedData.DateString)}\n" +
                                     $"開盤: {selectedData.Open:F2}\n" +
                                     $"最高: {selectedData.High:F2}\n" +
                                     $"最低: {selectedData.Low:F2}\n" +
                                     $"收盤: {selectedData.Close:F2}\n" +
                                     $"成交量: {selectedData.Volume:N0}";

                infoAnnotation.ForeColor = Color.White;
                infoAnnotation.BackColor = Color.DarkGray;
                infoAnnotation.Font = new Font("Arial", 9, FontStyle.Bold);

                // 設定標籤位置（右上角）
                infoAnnotation.AnchorX = Math.Min(currentSelectedIndex + 2, displayedData.Count - 1);
                infoAnnotation.AnchorY = (double)selectedData.High;
                infoAnnotation.Alignment = ContentAlignment.TopLeft;

                Kchart.Annotations.Add(infoAnnotation);

                // 刷新所有圖表
                Kchart.Invalidate();
                chart2.Invalidate();
                chart3.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"繪製十字線錯誤: {ex.Message}");
            }
        }
        private void ShowLastDayData()
        {
            if (displayedData == null || displayedData.Count == 0)
            {
                // 沒有資料時清空標籤
                label2.Text = "";
                label3.Text = "";
                label4.Text = "";
                label5.Text = "";
                label6.Text = "";
                label7.Text = "";
                return;
            }

            // 取得最後一天的資料
            var lastData = displayedData[displayedData.Count - 1];
            UpdateDataLabels(lastData);
        }
        private void UpdateDataLabels(StockData data)
        {
            try
            {
                // Label2: 日期
                label2.Text = FormatDateString(data.DateString);

                // Label3: 開盤價
                label3.Text = $"開: {data.Open:F2}";

                // Label4: 最高價
                label4.Text = $"高: {data.High:F2}";

                // Label5: 最低價
                label5.Text = $"低: {data.Low:F2}";

                // Label6: 收盤價
                label6.Text = $"收: {data.Close:F2}";

                // Label7: 成交量
                label7.Text = $"量: {data.Volume:N0}";

                // 根據漲跌設定收盤價顏色
                if (data.Close > data.Open)
                {
                    label6.ForeColor = Color.Red;
                }
                else if (data.Close < data.Open)
                {
                    label6.ForeColor = Color.Green;
                }
                else
                {
                    label6.ForeColor = Color.Black;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新標籤錯誤: {ex.Message}");
            }
        }
        // 清除十字線
        private void ClearCrosshair()
        {
            try
            {
                // 清除所有圖表的 StripLines
                if (Kchart.ChartAreas.Count > 0)
                {
                    foreach (ChartArea area in Kchart.ChartAreas)
                    {
                        area.AxisX.StripLines.Clear();
                        area.AxisY.StripLines.Clear();
                    }
                }

                if (chart2.ChartAreas.Count > 0)
                {
                    foreach (ChartArea area in chart2.ChartAreas)
                    {
                        area.AxisX.StripLines.Clear();
                        area.AxisY.StripLines.Clear();
                    }
                }

                if (chart3.ChartAreas.Count > 0)
                {
                    foreach (ChartArea area in chart3.ChartAreas)
                    {
                        area.AxisX.StripLines.Clear();
                        area.AxisY.StripLines.Clear();
                    }
                }

                // 清除標註
                foreach (var annotation in Kchart.Annotations.ToList())
                {
                    if (annotation.Name.StartsWith("CrosshairInfo"))
                        Kchart.Annotations.Remove(annotation);
                }

                // *** 修改：顯示最後一天的資料而非清空 ***
                ShowLastDayData();

                // 刷新所有圖表
                Kchart.Invalidate();
                chart2.Invalidate();
                chart3.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"清除十字線錯誤: {ex.Message}");
            }
        }

        // 格式化日期字串的輔助方法
        private string FormatDateString(string dateString)
        {
            if (dateString.Length >= 6)
            {
                // 假設格式是 "1140424" (民國年月日)
                string year = dateString.Substring(0, 3);
                string month = dateString.Substring(3, 2);
                string day = dateString.Substring(5, 2);

                // 轉換為西元年份顯示
                if (int.TryParse(year, out int rocYear))
                {
                    int adYear = rocYear + 1911;
                    return $"{adYear}/{month}/{day}";
                }
            }
            return dateString;
        }

        // *** 新增：根據週期格式化日期標籤 ***
        private string FormatDateLabel(string dateString, PeriodType period)
        {
            try
            {
                if (dateString.Length < 6) return dateString;

                string year = dateString.Substring(0, 3);
                string month = dateString.Substring(3, 2);
                string day = dateString.Substring(5, 2);

                // *** 修改：統一顯示年/月格式，避免標籤過密 ***
                if (int.TryParse(year, out int rocYear))
                {
                    int adYear = rocYear + 1911;
                    return $"{adYear}/{month}";
                }
                return $"{year}/{month}";
            }
            catch
            {
                return dateString;
            }
        }

        // 計算移動平均線的方法
        private List<decimal?> CalculateMovingAverage(List<StockData> stockData, int period)
        {
            var movingAverages = new List<decimal?>();

            for (int i = 0; i < stockData.Count; i++)
            {
                if (i < period - 1)
                {
                    // 資料不足時設為 null
                    movingAverages.Add(null);
                }
                else
                {
                    // 計算過去 period 天的平均收盤價
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

        private List<KDData> CalculateKD(List<StockData> stockData, int kdPeriod = 9)
        {
            var kdList = new List<KDData>();

            if (stockData.Count < kdPeriod)
            {
                // 資料不足，全部設為50
                for (int i = 0; i < stockData.Count; i++)
                {
                    kdList.Add(new KDData(50, 50));
                }
                return kdList;
            }

            // 前kdPeriod-1天都設為50
            for (int i = 0; i < kdPeriod - 1; i++)
            {
                kdList.Add(new KDData(50, 50));
            }

            // 從第kdPeriod天開始計算
            for (int i = kdPeriod - 1; i < stockData.Count; i++)
            {
                // 找過去kdPeriod天的最高價和最低價
                decimal highPrice = decimal.MinValue;
                decimal lowPrice = decimal.MaxValue;

                for (int j = i - kdPeriod + 1; j <= i; j++)
                {
                    if (stockData[j].High > highPrice) highPrice = stockData[j].High;
                    if (stockData[j].Low < lowPrice) lowPrice = stockData[j].Low;
                }

                // 計算RSV
                decimal rsv = 0;
                if (highPrice != lowPrice)
                {
                    rsv = ((stockData[i].Close - lowPrice) / (highPrice - lowPrice)) * 100;
                }

                // 計算K值
                decimal prevK = kdList[i - 1].K;
                decimal k = (2m / 3m) * prevK + (1m / 3m) * rsv;
                if (k >= 100) k = 99;

                // 計算D值
                decimal prevD = kdList[i - 1].D;
                decimal d = (2m / 3m) * prevD + (1m / 3m) * k;
                if (d >= 100) d = 99;

                kdList.Add(new KDData(k, d));
            }

            return kdList;
        }

        // 保存均線設定到檔案
        private void SaveMovingAverageSettings()
        {
            try
            {
                string settingsPath = Path.Combine(Application.StartupPath, "ma_settings.txt");
                string periodsString = string.Join(",", movingAveragePeriods);
                File.WriteAllText(settingsPath, periodsString);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存設定錯誤: {ex.Message}");
            }
        }

        // 從檔案讀取均線設定
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"讀取設定錯誤: {ex.Message}，使用預設值");
                // 保持預設值 { 5, 10, 20, 60 }
            }
        }

        // *** 新增：保存週期選擇到檔案 ***
        private void SavePeriodSettings()
        {
            try
            {
                string settingsPath = Path.Combine(Application.StartupPath, "period_settings.txt");
                File.WriteAllText(settingsPath, currentPeriod.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存週期設定錯誤: {ex.Message}");
            }
        }

        // *** 新增：從檔案讀取週期設定 ***
        private void LoadPeriodSettings()
        {
            try
            {
                string settingsPath = Path.Combine(Application.StartupPath, "period_settings.txt");

                if (File.Exists(settingsPath))
                {
                    string savedPeriod = File.ReadAllText(settingsPath).Trim();

                    if (Enum.TryParse<PeriodType>(savedPeriod, out PeriodType period))
                    {
                        currentPeriod = period;
                        UpdatePeriodButtonDisplay();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"讀取週期設定錯誤: {ex.Message}，使用預設值");
                // 保持預設值 Daily
            }
        }

        // *** 新增：更新週期按鈕顯示 ***
        private void UpdatePeriodButtonDisplay()
        {
            if (btnPeriod == null) return;

            switch (currentPeriod)
            {
                case PeriodType.Daily:
                    btnPeriod.Text = "日線";
                    btnPeriod.BackColor = Color.Gray;
                    btnPeriod.ForeColor = Color.White;
                    break;
                case PeriodType.Weekly:
                    btnPeriod.Text = "周線";
                    btnPeriod.BackColor = Color.Gray;
                    btnPeriod.ForeColor = Color.White;
                    break;
                case PeriodType.Monthly:
                    btnPeriod.Text = "月線";
                    btnPeriod.BackColor = Color.Gray;
                    btnPeriod.ForeColor = Color.White;
                    break;
            }
        }

        // ===== 簡化的畫圖事件處理方法 =====

        // 雙擊切換畫圖模式
        private void Kchart_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                drawingMode = !drawingMode;
                drawingPanel.Visible = drawingMode;

                if (drawingMode)
                {
                    Kchart.Cursor = Cursors.Cross;
                    // 關閉十字線
                    crosshairEnabled = false;
                    ClearCrosshair();
                }
                else
                {
                    Kchart.Cursor = Cursors.Default;
                    isDrawingLine = false;
                    currentPath.Clear();
                }
            }
        }

        // 顏色選擇
        private void BtnColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog dlg = new ColorDialog())
            {
                dlg.Color = btnColor.BackColor;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    btnColor.BackColor = dlg.Color;
                    btnColor.ForeColor = dlg.Color.GetBrightness() > 0.5 ? Color.Black : Color.White;
                }
            }
        }

        // 清除所有繪圖
        private void BtnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("確定要清除所有繪圖嗎？", "確認", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                drawnObjects.Clear();
                Kchart.Invalidate();
            }
        }

        // 滑鼠按下
        private void Kchart_MouseDown(object sender, MouseEventArgs e)
        {
            if (!drawingMode || e.Button != MouseButtons.Left) return;

            string tool = cboDrawingTool.SelectedItem?.ToString() ?? "直線";

            if (tool == "直線")
            {
                lineStartPoint = e.Location;
                isDrawingLine = true;
            }
            else // 自由繪圖
            {
                currentPath.Clear();
                currentPath.Add(e.Location);
            }
        }

        // 滑鼠移動
        private void Kchart_MouseMove(object sender, MouseEventArgs e)
        {
            if (!drawingMode || e.Button != MouseButtons.Left) return;

            string tool = cboDrawingTool.SelectedItem?.ToString() ?? "直線";

            if (tool == "自由繪圖" && currentPath.Count > 0)
            {
                currentPath.Add(e.Location);
                Kchart.Invalidate();
            }
            else if (isDrawingLine)
            {
                Kchart.Invalidate(); // 顯示直線預覽
            }
        }

        // 滑鼠放開
        private void Kchart_MouseUp(object sender, MouseEventArgs e)
        {
            if (!drawingMode || e.Button != MouseButtons.Left) return;

            string tool = cboDrawingTool.SelectedItem?.ToString() ?? "直線";

            if (tool == "直線" && isDrawingLine)
            {
                // 儲存直線
                var line = new DrawnObject
                {
                    Type = "Line",
                    Color = btnColor.BackColor,
                    Width = 2
                };
                line.Points.Add(lineStartPoint);
                line.Points.Add(e.Location);
                drawnObjects.Add(line);

                isDrawingLine = false;
            }
            else if (tool == "自由繪圖" && currentPath.Count > 1)
            {
                // 儲存路徑
                var path = new DrawnObject
                {
                    Type = "Path",
                    Color = btnColor.BackColor,
                    Width = 2,
                    Points = new List<Point>(currentPath)
                };
                drawnObjects.Add(path);

                currentPath.Clear();
            }

            Kchart.Invalidate();
        }

        // 繪圖方法（簡化版）
        private void Kchart_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 繪製已儲存的物件
            foreach (var obj in drawnObjects)
            {
                using (var pen = new Pen(obj.Color, obj.Width))
                {
                    if (obj.Type == "Line" && obj.Points.Count >= 2)
                    {
                        g.DrawLine(pen, obj.Points[0], obj.Points[1]);
                    }
                    else if (obj.Type == "Path" && obj.Points.Count > 1)
                    {
                        g.DrawLines(pen, obj.Points.ToArray());
                    }
                }
            }

            // 繪製預覽
            if (drawingMode)
            {
                using (var pen = new Pen(btnColor.BackColor, 2))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

                    if (isDrawingLine)
                    {
                        var currentPos = Kchart.PointToClient(MousePosition);
                        g.DrawLine(pen, lineStartPoint, currentPos);
                    }
                    else if (currentPath.Count > 1)
                    {
                        g.DrawLines(pen, currentPath.ToArray());
                    }
                }
            }
        }

        // 均線設定事件
        private void MASettingsButton_Click(object sender, EventArgs e)
        {
            ShowMovingAverageSettings();
        }

        // 顯示均線設定對話框
        private void ShowMovingAverageSettings()
        {
            Form settingsForm = new Form();
            settingsForm.Text = "均線設定";
            settingsForm.Size = new Size(300, 200);
            settingsForm.StartPosition = FormStartPosition.CenterParent;
            settingsForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            settingsForm.MaximizeBox = false;
            settingsForm.MinimizeBox = false;

            Label label = new Label();
            label.Text = "均線週期 (用逗號分隔):";
            label.Location = new Point(10, 20);
            label.Size = new Size(200, 20);
            settingsForm.Controls.Add(label);

            TextBox periodsTextBox = new TextBox();
            periodsTextBox.Text = string.Join(",", movingAveragePeriods);
            periodsTextBox.Location = new Point(10, 50);
            periodsTextBox.Size = new Size(250, 25);
            settingsForm.Controls.Add(periodsTextBox);

            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Text = "確定";
            okButton.Location = new Point(100, 100);
            okButton.Size = new Size(75, 25);
            okButton.Click += (s, e) =>
            {
                try
                {
                    string[] periodStrings = periodsTextBox.Text.Split(',');
                    int[] newPeriods = periodStrings.Select(p => int.Parse(p.Trim())).ToArray();

                    if (newPeriods.All(p => p > 0))
                    {
                        movingAveragePeriods = newPeriods;

                        // === 新增：保存設定到檔案 ===
                        SaveMovingAverageSettings();

                        settingsForm.DialogResult = DialogResult.OK;
                        settingsForm.Close();

                        // 重新繪製圖表
                        if (allStockData.Count > 0)
                        {
                            var filteredData = GetDisplayData();
                            // *** 修改：使用新的DrawAllCharts方法 ***
                            DrawAllCharts(filteredData, currentStockCode);
                        }
                    }
                    else
                    {
                        MessageBox.Show("請輸入大於0的數字", "錯誤");
                    }
                }
                catch
                {
                    MessageBox.Show("請輸入有效的數字，用逗號分隔", "錯誤");
                }
            };
            settingsForm.Controls.Add(okButton);

            System.Windows.Forms.Button cancelButton = new System.Windows.Forms.Button();
            cancelButton.Text = "取消";
            cancelButton.Location = new Point(185, 100);
            cancelButton.Size = new Size(75, 25);
            cancelButton.Click += (s, e) =>
            {
                settingsForm.DialogResult = DialogResult.Cancel;
                settingsForm.Close();
            };
            settingsForm.Controls.Add(cancelButton);

            settingsForm.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // 開啟 Form2 - 雙股票K線比較視窗
            Form2 form2 = new Form2();
            form2.Show(); // 使用 Show() 可以同時開啟兩個視窗
        }

    }

    public class DrawnObject
    {
        public string Type { get; set; } // "Line" 或 "Path"
        public List<Point> Points { get; set; } = new List<Point>();
        public Color Color { get; set; } = Color.Red;
        public int Width { get; set; } = 2;
    }

    // *** 法人資料類別 ***
    public class LawData
    {
        public string DateString { get; set; }  // 日期字串，如 "1140424"

        // 外資
        public long ForeignBuy { get; set; }    // 外資買進
        public long ForeignSell { get; set; }   // 外資賣出
        public long ForeignNet => ForeignBuy - ForeignSell;  // 外資買賣超

        // 投信
        public long InvestmentBuy { get; set; }  // 投信買進
        public long InvestmentSell { get; set; } // 投信賣出
        public long InvestmentNet => InvestmentBuy - InvestmentSell;  // 投信買賣超

        // 自營商
        public long DealerBuy { get; set; }     // 自營商買進
        public long DealerSell { get; set; }    // 自營商賣出
        public long DealerNet => DealerBuy - DealerSell;  // 自營商買賣超
    }

    public class StockData
    {
        public string DateString { get; set; }  // 改用字串，如 "1140424"
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }

        public override string ToString()
        {
            return $"日期:{DateString}, 開:{Open:F2}, 高:{High:F2}, 低:{Low:F2}, 收:{Close:F2}, 量:{Volume:N0}";
        }
    }

    public class KDData
    {
        public decimal K { get; set; }
        public decimal D { get; set; }

        public KDData(decimal k = 50, decimal d = 50)
        {
            K = k;
            D = d;
        }
    }

    // *** 新增：融資融券資料類別 ***
    public class MarginData
    {
        public string DateString { get; set; }  // 日期字串，如 "1140424"

        // 融資
        public long MarginBuy { get; set; }     // 融資買進
        public long MarginSell { get; set; }    // 融資賣出
        public long MarginBalance { get; set; } // 融資餘額
        public long MarginNet => MarginBuy - MarginSell;  // 融資淨額

        // 融券
        public long ShortBuy { get; set; }      // 融券買進
        public long ShortSell { get; set; }     // 融券賣出
        public long ShortBalance { get; set; }  // 融券餘額
        public long ShortNet => ShortSell - ShortBuy;  // 融券淨額（賣出減買進）
    }
}