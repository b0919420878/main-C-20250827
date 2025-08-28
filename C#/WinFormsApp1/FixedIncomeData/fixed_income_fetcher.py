import os
import json
import time
import requests
import yfinance as yf
from datetime import datetime
from typing import Dict, Optional, List
from taiwan_swap_spread import TaiwanSwapSpreadFetcher

# API 設定
FRED_API_KEY = '2ff0a0ce8f6eecfda11381253042a1a4'  # 請替換為您的 FRED API Key

# 數據儲存路徑
DATA_PATH = os.path.dirname(os.path.abspath(__file__))

# 指標對應設定
INDICATORS = {
    'CSRA': {
        'name': 'Credit Spread',
        'fred_series': 'BAMLC0A0CM',
        'unit': '%'
    },
    'Repo': {
        'name': 'Repo Rate (SOFR)',
        'fred_series': 'SOFR',
        'unit': '%'
    },
    'YC02': {
        'name': '2Y Treasury',
        'fred_series': 'DGS2',
        'unit': '%'
    },
    'YC10': {
        'name': '10Y Treasury',
        'fred_series': 'DGS10',
        'unit': '%'
    },
    'YC30': {
        'name': '30Y Treasury',
        'fred_series': 'DGS30',
        'unit': '%'
    }
}

class FixedIncomeFetcher:
    def __init__(self):
        self.session = requests.Session()
        self.previous_values = self.load_previous_values()
        self.taiwan_fetcher = TaiwanSwapSpreadFetcher()
    
    def load_previous_values(self) -> Dict[str, float]:
        """載入前次數值以計算變化"""
        prev_file = os.path.join(DATA_PATH, 'previous_values.json')
        if os.path.exists(prev_file):
            with open(prev_file, 'r') as f:
                return json.load(f)
        return {}
    
    def save_previous_values(self, values: Dict[str, float]):
        """儲存當前數值"""
        prev_file = os.path.join(DATA_PATH, 'previous_values.json')
        with open(prev_file, 'w') as f:
            json.dump(values, f)
    
    def fetch_fred_data(self, series_id: str) -> Optional[float]:
        """從 FRED 獲取數據"""
        try:
            url = f"https://api.stlouisfed.org/fred/series/observations"
            params = {
                'series_id': series_id,
                'api_key': FRED_API_KEY,
                'file_type': 'json',
                'limit': 1,
                'sort_order': 'desc'
            }
            
            response = self.session.get(url, params=params)
            if response.status_code == 200:
                data = response.json()
                observations = data.get('observations', [])
                if observations:
                    value = observations[0].get('value')
                    if value and value != '.':
                        return float(value)
        except Exception as e:
            print(f"Error fetching FRED data for {series_id}: {e}")
        return None
    
    def fetch_yahoo_data(self, symbol: str) -> Optional[float]:
        """從 Yahoo Finance 獲取數據"""
        try:
            ticker = yf.Ticker(symbol)
            info = ticker.info
            
            # 嘗試不同的價格欄位
            price = info.get('regularMarketPrice') or \
                   info.get('price') or \
                   info.get('previousClose')
            
            if price:
                return float(price)
                
            # 如果 info 沒有數據，嘗試獲取歷史數據
            hist = ticker.history(period="1d")
            if not hist.empty:
                return float(hist['Close'].iloc[-1])
                
        except Exception as e:
            print(f"Error fetching Yahoo data for {symbol}: {e}")
        return None
    
    def calculate_change(self, current: float, indicator_key: str) -> tuple:
        """計算變化量和變化率"""
        previous = self.previous_values.get(indicator_key, current)
        change = current - previous
        change_rate = (change / previous * 100) if previous != 0 else 0
        return change, change_rate
    
    def save_to_file(self, filename: str, value: float, change: float, change_rate: float):
        """儲存數據到檔案"""
        filepath = os.path.join(DATA_PATH, filename)
        # 使用民國年格式 (西元年 - 1911) 和只有日期
        date = datetime.now()
        roc_year = date.year - 1911
        date_str = f'"{roc_year:03d}{date.month:02d}{date.day:02d}"'
        
        # 格式化數據行 - 股票格式: 日期,開,高,低,收,成交量
        # 開高低收都用同樣的值，成交量設為0
        value_str = f' {value:.4f}'
        line = f'{date_str},{value_str},{value_str},{value_str},{value_str}, 0\n'
        
        # 追加到檔案
        with open(filepath, 'a', encoding='utf-8') as f:
            f.write(line)
    
    def fetch_all_indicators(self):
        """獲取所有指標"""
        current_values = {}
        
        print(f"\n[{datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] 開始獲取數據...")
        
        for key, config in INDICATORS.items():
            print(f"\n正在獲取 {config['name']}...", end='')
            
            value = None
            
            # FRED 數據
            if 'fred_series' in config:
                value = self.fetch_fred_data(config['fred_series'])
            
            # Yahoo Finance 數據
            elif 'yahoo_symbol' in config:
                value = self.fetch_yahoo_data(config['yahoo_symbol'])
            
            if value is not None:
                # 計算變化
                change, change_rate = self.calculate_change(value, key)
                
                # 儲存到檔案
                filename = f"{key}.txt"
                self.save_to_file(filename, value, change, change_rate)
                
                # 記錄當前值
                current_values[key] = value
                
                print(f" 成功! 值: {value:.4f} ({change:+.4f}, {change_rate:+.2f}%)")
            else:
                print(f" 失敗!")
        
        # 計算衍生指標
        self.calculate_derived_indicators(current_values)
        
        # 抓取台灣 Swap Spread
        print("\n正在獲取台灣 Swap Spread...")
        try:
            # 使用特定日期 (20250805) 或根據需要調整
            taiwan_spreads = self.taiwan_fetcher.fetch_and_calculate('20250805')
            if taiwan_spreads:
                print("台灣 Swap Spread 獲取成功!")
        except Exception as e:
            print(f"台灣 Swap Spread 獲取失敗: {e}")
        
        # 儲存當前值
        self.save_previous_values(current_values)
        
        # 計算風險評分
        risk_score = self.calculate_risk_score(current_values)
        print(f"\n風險評分: {risk_score}/100")
    
    def calculate_derived_indicators(self, values: Dict[str, float]):
        """計算衍生指標"""
        # Duration Spread (30Y - 2Y)
        if 'YC30' in values and 'YC02' in values:
            yc30 = values.get('YC30', 0)
            yc02 = values.get('YC02', 0)
            duration_spread = yc30 - yc02
            
            change, change_rate = self.calculate_change(duration_spread, 'DURA')
            self.save_to_file('DURA.txt', duration_spread, change, change_rate)
            values['DURA'] = duration_spread
            
            print(f"\nDuration Spread (30Y-2Y): {duration_spread:.4f} ({change:+.4f}, {change_rate:+.2f}%)")
        
        # Swap Spread (簡化計算：10Y + 0.3%)
        if 'YC10' in values:
            yc10 = values.get('YC10', 0)
            swap_spread = 0.3  # 簡化假設：30 bps
            
            change, change_rate = self.calculate_change(swap_spread, 'SAPS')
            self.save_to_file('SAPS.txt', swap_spread, change, change_rate)
            values['SAPS'] = swap_spread
            
            print(f"Swap Spread (模擬): {swap_spread:.4f} ({change:+.4f}, {change_rate:+.2f}%)")
    
    def calculate_risk_score(self, values: Dict[str, float]) -> int:
        """計算風險評分 (基於文章邏輯)"""
        score = 0
        
        # Repo Rate 異常 (>5%)
        if values.get('Repo', 0) > 5:
            score += 30
            print("⚠️  Repo Rate 異常升高!")
        
        # Swap Spread 擴大 (>30 bps)
        if values.get('SAPS', 0) > 0.3:
            score += 25
            print("⚠️  Swap Spread 擴大!")
        
        # Credit Spread 擴大 (>2%)
        if values.get('CSRA', 0) > 2:
            score += 25
            print("⚠️  Credit Spread 擴大!")
        
        return score

def main():
    """主程式"""
    fetcher = FixedIncomeFetcher()
    
    print("固定收益指標監控系統")
    print("=" * 50)
    print(f"數據將儲存到: {DATA_PATH}")
    print("=" * 50)
    
    # 立即執行一次
    fetcher.fetch_all_indicators()
    
    # 定時執行
    print("\n開始定時監控... (按 Ctrl+C 停止)")
    
    while True:
        try:
            # 每小時更新指標
            time.sleep(3600)  # 等待 3600 秒 (1小時)
            fetcher.fetch_all_indicators()
                
        except KeyboardInterrupt:
            print("\n\n程式已停止")
            break
        except Exception as e:
            print(f"\n錯誤: {e}")
            time.sleep(60)

if __name__ == "__main__":
    main()