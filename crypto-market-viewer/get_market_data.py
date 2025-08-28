#!/usr/bin/env python3
import requests
import json

# 獲取全球市場數據
r = requests.get('https://api.coingecko.com/api/v3/global')
global_data = r.json()['data']

print('=== 全球加密貨幣市場概況 ===')
print(f'總市值: ${global_data["total_market_cap"]["usd"]:,.0f}')
print(f'BTC 主導率: {global_data["market_cap_percentage"]["btc"]:.2f}%')
print(f'ETH 主導率: {global_data["market_cap_percentage"]["eth"]:.2f}%')

# 獲取前20大加密貨幣（排除穩定幣）
r2 = requests.get('https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=30&page=1')
coins = r2.json()

stablecoins = ['tether', 'usd-coin', 'binance-usd', 'dai', 'trueusd', 'usdd']
filtered_coins = [coin for coin in coins if coin['id'] not in stablecoins][:20]

print('\n=== 市值前20大加密貨幣（排除穩定幣）===')

for i, coin in enumerate(filtered_coins, 1):
    cap = coin['market_cap'] or 0
    percentage = (cap / global_data['total_market_cap']['usd'] * 100) if cap > 0 else 0
    change_24h = coin['price_change_percentage_24h'] or 0
    change_str = f"+{change_24h:.2f}%" if change_24h > 0 else f"{change_24h:.2f}%"
    
    print(f'{i:2d}. {coin["symbol"].upper():<6} {coin["name"]:<25} ${coin["current_price"]:>10,.2f} ${cap:>15,.0f} ({percentage:5.2f}%) {change_str:>8}')

print('\n=== 前10大市值佔比 ===')
cumulative = 0
for i, coin in enumerate(filtered_coins[:10], 1):
    cap = coin['market_cap'] or 0
    percentage = (cap / global_data['total_market_cap']['usd'] * 100) if cap > 0 else 0
    cumulative += percentage
    bar = '█' * int(percentage)
    print(f'{i:2d}. {coin["symbol"].upper():<5} {bar:<35} {percentage:5.2f}% (累計: {cumulative:5.2f}%)')