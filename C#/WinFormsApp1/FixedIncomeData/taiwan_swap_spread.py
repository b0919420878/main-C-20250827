import json
import time
import requests
from typing import Dict, Optional
from datetime import datetime

class TaiwanSwapSpreadFetcher:
    def __init__(self):
        self.session = requests.Session()
        
    def fetch_taiwan_swap_data(self, date: str) -> Optional[Dict]:
        """
        抓取台灣 IRS Swap 資料
        date format: YYYYMMDD
        """
        try:
            # 台灣期貨交易所 API (假設的範例)
            url = "https://www.taifex.com.tw/cht/3/dlDailyMarketView"
            
            params = {
                'queryType': '2',
                'marketCode': '0',
                'commodity_id': 'IRS',
                'queryDate': f"{date[:4]}/{date[4:6]}/{date[6:8]}",
                'MarketCode': 'IRS'
            }
            
            response = self.session.get(url, params=params, timeout=10)
            
            if response.status_code == 200:
                # 解析資料（實際格式需根據 API 返回調整）
                data = self.parse_taiwan_data(response.text)
                return data
            else:
                print(f"Failed to fetch Taiwan swap data: HTTP {response.status_code}")
                return None
                
        except Exception as e:
            print(f"Error fetching Taiwan swap data: {e}")
            return None
    
    def parse_taiwan_data(self, raw_data: str) -> Dict:
        """解析台灣 IRS 資料"""
        try:
            # 這裡需要根據實際的資料格式進行解析
            # 以下為示例結構
            swap_rates = {
                '1Y': 1.45,
                '2Y': 1.52,
                '3Y': 1.58,
                '5Y': 1.65,
                '10Y': 1.75
            }
            
            return {
                'date': datetime.now().strftime('%Y-%m-%d'),
                'swap_rates': swap_rates,
                'currency': 'TWD'
            }
        except Exception as e:
            print(f"Error parsing Taiwan data: {e}")
            return {}
    
    def calculate_swap_spread(self, swap_rate: float, gov_yield: float) -> float:
        """計算 Swap Spread = Swap Rate - Government Yield"""
        return swap_rate - gov_yield
    
    def fetch_and_calculate(self, date: str) -> Optional[Dict]:
        """抓取並計算台灣 Swap Spread"""
        try:
            # 抓取 Swap 資料
            swap_data = self.fetch_taiwan_swap_data(date)
            
            if not swap_data:
                return None
            
            # 這裡需要加入政府公債殖利率資料
            # 暫時使用假設值
            gov_yields = {
                '1Y': 1.20,
                '2Y': 1.25,
                '3Y': 1.30,
                '5Y': 1.35,
                '10Y': 1.45
            }
            
            # 計算 Swap Spread
            swap_spreads = {}
            for tenor, swap_rate in swap_data.get('swap_rates', {}).items():
                if tenor in gov_yields:
                    spread = self.calculate_swap_spread(swap_rate, gov_yields[tenor])
                    swap_spreads[tenor] = {
                        'swap_rate': swap_rate,
                        'gov_yield': gov_yields[tenor],
                        'spread': round(spread, 4)
                    }
            
            result = {
                'date': swap_data['date'],
                'currency': 'TWD',
                'swap_spreads': swap_spreads
            }
            
            # 儲存結果
            self.save_results(result)
            
            return result
            
        except Exception as e:
            print(f"Error in fetch_and_calculate: {e}")
            return None
    
    def save_results(self, data: Dict):
        """儲存計算結果"""
        try:
            # 儲存詳細資料
            with open('taiwan_swap_detail.json', 'w', encoding='utf-8') as f:
                json.dump(data, f, ensure_ascii=False, indent=2)
            
            # 儲存簡化版本供快速查詢
            summary = {
                'date': data['date'],
                'spreads': {k: v['spread'] for k, v in data['swap_spreads'].items()}
            }
            
            with open('taiwan_swap_analysis.json', 'w', encoding='utf-8') as f:
                json.dump(summary, f, ensure_ascii=False, indent=2)
                
            print(f"Taiwan swap spread data saved successfully")
            
        except Exception as e:
            print(f"Error saving results: {e}")

if __name__ == "__main__":
    fetcher = TaiwanSwapSpreadFetcher()
    
    # 使用今天的日期
    today = datetime.now().strftime('%Y%m%d')
    
    print(f"Fetching Taiwan swap spread for {today}...")
    result = fetcher.fetch_and_calculate(today)
    
    if result:
        print("\nTaiwan Swap Spreads:")
        for tenor, data in result['swap_spreads'].items():
            print(f"{tenor}: {data['spread']:.4f} (Swap: {data['swap_rate']:.2f}%, Govt: {data['gov_yield']:.2f}%)")