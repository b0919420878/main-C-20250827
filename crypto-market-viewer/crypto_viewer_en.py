#!/usr/bin/env python3
"""
Crypto Market Cap Viewer - English Version
Fixed encoding issues for Windows
"""

import requests
import time
from datetime import datetime
import os

class CryptoMarketViewer:
    def __init__(self):
        self.api_url = "https://api.coingecko.com/api/v3"
        self.stablecoins = [
            'tether', 'usd-coin', 'binance-usd', 'dai', 'terrausd', 
            'trueusd', 'paxos-standard', 'neutrino', 'husd', 'gemini-dollar',
            'usdd', 'frax', 'tusd', 'usdc', 'usdt', 'busd', 'usdp'
        ]
    
    def clear_screen(self):
        """Clear screen"""
        os.system('cls' if os.name == 'nt' else 'clear')
    
    def get_market_data(self):
        """Get market data"""
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
            
            # Filter out stablecoins
            coins = []
            for coin in all_coins:
                if coin['id'] not in self.stablecoins:
                    coins.append(coin)
                if len(coins) >= 100:
                    break
            
            return coins
        except Exception as e:
            print(f"Error: {e}")
            return []
    
    def format_number(self, num):
        """Format numbers"""
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
        """Display market data"""
        self.clear_screen()
        
        # Calculate total market cap
        total_market_cap = sum(coin['market_cap'] or 0 for coin in coins)
        
        # Display header
        print("=" * 140)
        print(f"CRYPTO MARKET CAP RANKING (Stablecoins Excluded) - {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
        print(f"Total Market Cap: {self.format_number(total_market_cap)}")
        print("=" * 140)
        
        # Table header
        print(f"{'Rank':<5} {'Symbol':<8} {'Name':<25} {'Price':<15} {'Market Cap':<15} {'%':<8} {'24h%':<10} {'Circulating Supply':<20}")
        print("-" * 140)
        
        # Display top 50
        for i, coin in enumerate(coins[:50], 1):
            market_cap = coin['market_cap'] or 0
            dominance = (market_cap / total_market_cap * 100) if total_market_cap > 0 else 0
            price_change = coin.get('price_change_percentage_24h', 0) or 0
            
            # Format price
            if coin['current_price'] < 0.01:
                price_str = f"${coin['current_price']:.6f}"
            elif coin['current_price'] < 1:
                price_str = f"${coin['current_price']:.4f}"
            else:
                price_str = f"${coin['current_price']:.2f}"
            
            # Price change symbol
            change_symbol = "+" if price_change > 0 else ""
            
            print(f"{i:<5} {coin['symbol'].upper():<8} {coin['name'][:24]:<25} {price_str:<15} "
                  f"{self.format_number(market_cap):<15} {dominance:>6.2f}% "
                  f"{change_symbol}{price_change:>7.2f}% {coin['circulating_supply']:>18,.0f}")
        
        print("\n" + "=" * 140)
        
        # Display top 10 market cap distribution
        print("\nTop 10 Cryptocurrencies Market Cap Distribution:")
        print("-" * 60)
        
        cumulative = 0
        for i, coin in enumerate(coins[:10], 1):
            market_cap = coin['market_cap'] or 0
            dominance = (market_cap / total_market_cap * 100) if total_market_cap > 0 else 0
            cumulative += dominance
            
            bar_length = int(dominance * 0.5)
            bar = "#" * bar_length
            
            print(f"{i:>2}. {coin['symbol'].upper():<5} {bar:<35} {dominance:>5.2f}% (Total: {cumulative:>5.2f}%)")
        
        print(f"\nPress Ctrl+C to exit | Auto refresh in 30 seconds")
    
    def run(self):
        """Main loop"""
        while True:
            try:
                coins = self.get_market_data()
                if coins:
                    self.display_market_data(coins)
                time.sleep(30)
            except KeyboardInterrupt:
                print("\n\nProgram terminated")
                break
            except Exception as e:
                print(f"Error occurred: {e}")
                time.sleep(5)

if __name__ == "__main__":
    viewer = CryptoMarketViewer()
    viewer.run()