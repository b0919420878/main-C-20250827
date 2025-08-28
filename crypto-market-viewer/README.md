# 🪙 加密貨幣市值監控器

## 功能特色
- 即時顯示加密貨幣市值排行（排除穩定幣）
- 自動計算每個幣種的市值佔比
- 30秒自動更新數據
- 支援中英文界面

## 檔案說明
- `simple_market_viewer.py` - 中文版顯示器
- `crypto_viewer_en.py` - 英文版顯示器
- `crypto_market_viewer.py` - 豪華版（需要 rich 套件）
- `get_market_data.py` - 單次查詢腳本

## 使用方法

### 基本版（推薦）
```bash
python simple_market_viewer.py
```

### 英文版
```bash
python crypto_viewer_en.py
```

### 豪華版
```bash
pip install rich
python crypto_market_viewer.py
```

## 依賴套件
```bash
pip install requests pandas
pip install rich  # 僅豪華版需要
```

## 數據來源
- CoinGecko API（免費）
- 自動排除主要穩定幣