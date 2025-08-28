固定收益指標數據儲存資料夾
========================

此資料夾用於儲存從 Python API 抓取的固定收益市場指標數據

檔案說明：
---------
CSRA.txt - Credit Spread (信用利差)
VIX0.txt - VIX 波動率指數
Repo.txt - Repo Rate (附買回利率)
SAPS.txt - Swap Spread (交換利差)
IRS.txt  - IRS Spread (利率交換利差)
DURA.txt - Duration Spread (存續期間利差)
YC02.txt - 2年期公債殖利率
YC10.txt - 10年期公債殖利率
YC30.txt - 30年期公債殖利率

數據格式：
---------
日期,時間,數值,變化量,變化率
2024-01-15,09:30:00,25.5,+0.5,+2.0%

風險傳導順序：
------------
1. Repo Rate (最早期信號)
2. Swap Spread (銀行體系壓力)
3. Credit Spread (企業信用風險)
4. VIX (股市恐慌指數)

更新頻率：
---------
VIX: 每分鐘
其他指標: 每小時