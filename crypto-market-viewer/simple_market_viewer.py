#!/usr/bin/env python3
"""
簡化版市值顯示器 - 不需要 rich 套件
"""

import requests
import time
from datetime import datetime
import os

class SimpleMarketViewer:
    def __init__(self):
        self.api_url = "https://api.coingecko.com/api/v3"
        self.stablecoins = [
            'tether', 'usd-coin', 'binance-usd', 'dai', 'terrausd', 
            'trueusd', 'paxos-standard', 'neutrino', 'husd', 'gemini-dollar',
            'usdd', 'frax', 'tusd', 'usdc', 'usdt', 'busd', 'usdp'
        ]
    
    def clear_screen(self):
        """清除螢幕"""
        os.system('cls' if os.name == 'nt' else 'clear')
    
    def get_market_data(self):
        """獲取市場數據"""
        try:
            params = {
                'vs_currency': 'usd',
                'order': 'market_cap_desc',
                'per_page': 150,
                'page': 1,
                'sparkline': False
            }
            
            response = requests.get(f"{self.api_url}/coins/markets", params=params)
            all_coins = response.json()
            
            # 過濾穩定幣
            coins = []
            for coin in all_coins:
                if coin['id'] not in self.stablecoins:
                    coins.append(coin)
                if len(coins) >= 100:
                    break
            
            return coins
        except Exception as e:
            print(f"錯誤: {e}")
            return []
    
    def format_number(self, num):
        """格式化數字"""
        if num >= 1e12:
            return f"${num/1e12:.2f}T"
        elif num >= 1e9:
            return f"${num/1e9:.2f}B"
        elif num >= 1e6:
            return f"${num/1e6:.2f}M"
        elif num >= 1e3:
            return f"${num/1e3:.2f}K"
        else:
            return f"${num:.2f}"
    
    def display_market_data(self, coins):
        """顯示市場數據"""
        self.clear_screen()
        
        # 計算總市值
        total_market_cap = sum(coin['market_cap'] or 0 for coin in coins)
        
        # 顯示標題
        print("=" * 120)
        print(f"加密貨幣市值排行榜（排除穩定幣） - {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
        print(f"總市值: {self.format_number(total_market_cap)}")
        print("=" * 120)
        
        # 表頭
        print(f"{'排名':<4} {'代號':<8} {'名稱':<20} {'價格':<15} {'市值':<15} {'佔比':<8} {'24h%':<10} {'流通量':<20}")
        print("-" * 120)
        
        # 顯示前50個
        for i, coin in enumerate(coins[:50], 1):
            market_cap = coin['market_cap'] or 0
            dominance = (market_cap / total_market_cap * 100) if total_market_cap > 0 else 0
            price_change = coin.get('price_change_percentage_24h', 0) or 0
            
            # 價格格式化
            if coin['current_price'] < 0.01:
                price_str = f"${coin['current_price']:.6f}"
            elif coin['current_price'] < 1:
                price_str = f"${coin['current_price']:.4f}"
            else:
                price_str = f"${coin['current_price']:.2f}"
            
            # 漲跌符號
            change_symbol = "+" if price_change > 0 else ""
            
            print(f"{i:<4} {coin['symbol'].upper():<8} {coin['name'][:19]:<20} {price_str:<15} "
                  f"{self.format_number(market_cap):<15} {dominance:>6.2f}% "
                  f"{change_symbol}{price_change:>7.2f}% {coin['circulating_supply']:>18,.0f}")
        
        print("\n" + "=" * 120)
        
        # 顯示前10大幣種的詳細佔比
        print("\n前10大加密貨幣市值佔比:")
        print("-" * 50)
        
        cumulative = 0
        for i, coin in enumerate(coins[:10], 1):
            market_cap = coin['market_cap'] or 0
            dominance = (market_cap / total_market_cap * 100) if total_market_cap > 0 else 0
            cumulative += dominance
            
            bar_length = int(dominance * 0.5)  # 每1%顯示0.5個字符
            bar = "█" * bar_length
            
            print(f"{i:>2}. {coin['symbol'].upper():<5} {bar:<25} {dominance:>5.2f}% (累計: {cumulative:>5.2f}%)")
        
        print(f"\n按 Ctrl+C 退出 | 30秒後自動更新")
    
    def run(self):
        """主運行循環"""
        while True:
            try:
                coins = self.get_market_data()
                if coins:
                    self.display_market_data(coins)
                time.sleep(30)
            except KeyboardInterrupt:
                print("\n\n程式已退出")
                break
            except Exception as e:
                print(f"發生錯誤: {e}")
                time.sleep(5)

if __name__ == "__main__":
    viewer = SimpleMarketViewer()
    viewer.run()