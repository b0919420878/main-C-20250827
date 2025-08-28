#!/usr/bin/env python3
"""
加密貨幣市值即時顯示器
獨立運行的視覺化程式
"""

import requests
import pandas as pd
import time
from datetime import datetime
from typing import Dict, List
import os
import sys

# 視覺化相關
try:
    from rich.console import Console
    from rich.table import Table
    from rich.live import Live
    from rich.panel import Panel
    from rich.layout import Layout
    from rich.progress import Progress, SpinnerColumn, TextColumn
    from rich.text import Text
except ImportError:
    print("請先安裝 rich: pip install rich")
    sys.exit(1)

console = Console()

class CryptoMarketViewer:
    def __init__(self):
        self.console = Console()
        self.coingecko_api = "https://api.coingecko.com/api/v3"
        self.refresh_interval = 30  # 30秒更新一次
        
    def get_market_data(self, limit: int = 100) -> List[Dict]:
        """獲取市值數據"""
        try:
            # 穩定幣列表
            stablecoins = [
                'tether', 'usd-coin', 'binance-usd', 'dai', 'terrausd', 
                'trueusd', 'paxos-standard', 'neutrino', 'husd', 'gemini-dollar',
                'usdd', 'frax', 'tusd', 'usdc', 'usdt', 'busd', 'usdp',
                'tether-gold', 'pax-gold'
            ]
            
            # 獲取前200個幣以確保有足夠的非穩定幣
            params = {
                'vs_currency': 'usd',
                'order': 'market_cap_desc',
                'per_page': 200,
                'page': 1,
                'sparkline': False,
                'price_change_percentage': '24h,7d'
            }
            
            response = requests.get(f"{self.coingecko_api}/coins/markets", params=params)
            response.raise_for_status()
            all_coins = response.json()
            
            # 檢查響應是否為列表
            if not isinstance(all_coins, list):
                self.console.print(f"[red]API響應格式錯誤: {type(all_coins)}[/red]")
                return []
            
            # 過濾掉穩定幣
            coins = []
            for coin in all_coins:
                if isinstance(coin, dict) and coin.get('id') not in stablecoins and len(coins) < limit:
                    coins.append(coin)
            
            return coins
        except Exception as e:
            self.console.print(f"[red]獲取數據錯誤: {e}[/red]")
            return []
    
    def calculate_market_stats(self, coins: List[Dict]) -> Dict:
        """計算市場統計"""
        total_market_cap = sum(coin['market_cap'] or 0 for coin in coins)
        total_volume = sum(coin['total_volume'] or 0 for coin in coins)
        
        # 計算各幣種市值佔比
        for coin in coins:
            if coin['market_cap']:
                coin['market_dominance'] = (coin['market_cap'] / total_market_cap) * 100
            else:
                coin['market_dominance'] = 0
        
        return {
            'total_market_cap': total_market_cap,
            'total_volume': total_volume,
            'coins': coins
        }
    
    def create_market_table(self, stats: Dict) -> Table:
        """創建市場數據表格"""
        table = Table(title=f"加密貨幣市值排行 (排除穩定幣)\n更新時間: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
        
        # 添加列
        table.add_column("排名", style="cyan", no_wrap=True)
        table.add_column("代號", style="magenta")
        table.add_column("名稱", style="blue")
        table.add_column("價格 (USD)", justify="right")
        table.add_column("市值 (USD)", justify="right", style="green")
        table.add_column("市值佔比", justify="right", style="yellow")
        table.add_column("流通量", justify="right")
        table.add_column("24h %", justify="right")
        table.add_column("7d %", justify="right")
        table.add_column("24h 交易量", justify="right")
        
        # 添加數據行
        for i, coin in enumerate(stats['coins'][:50], 1):  # 顯示前50個
            price_change_24h = coin.get('price_change_percentage_24h', 0) or 0
            price_change_7d = coin.get('price_change_percentage_7d_in_currency', 0) or 0
            
            # 根據漲跌設置顏色
            change_24h_color = "green" if price_change_24h > 0 else "red"
            change_7d_color = "green" if price_change_7d > 0 else "red"
            
            # 格式化市值顯示
            if coin.get('market_cap'):
                if coin['market_cap'] >= 1e12:
                    market_cap_str = f"${coin['market_cap']/1e12:.2f}T"
                elif coin['market_cap'] >= 1e9:
                    market_cap_str = f"${coin['market_cap']/1e9:.2f}B"
                elif coin['market_cap'] >= 1e6:
                    market_cap_str = f"${coin['market_cap']/1e6:.2f}M"
                else:
                    market_cap_str = f"${coin['market_cap']:,.0f}"
            else:
                market_cap_str = "N/A"
            
            # 格式化價格顯示
            if coin.get('current_price'):
                if coin['current_price'] < 0.01:
                    price_str = f"${coin['current_price']:.6f}"
                elif coin['current_price'] < 1:
                    price_str = f"${coin['current_price']:.4f}"
                else:
                    price_str = f"${coin['current_price']:,.2f}"
            else:
                price_str = "N/A"
            
            # 格式化交易量
            if coin.get('total_volume'):
                if coin['total_volume'] >= 1e9:
                    volume_str = f"${coin['total_volume']/1e9:.2f}B"
                elif coin['total_volume'] >= 1e6:
                    volume_str = f"${coin['total_volume']/1e6:.2f}M"
                else:
                    volume_str = f"${coin['total_volume']:,.0f}"
            else:
                volume_str = "N/A"
            
            table.add_row(
                str(i),
                coin.get('symbol', 'N/A').upper(),
                coin.get('name', 'N/A')[:20],  # 限制名稱長度
                price_str,
                market_cap_str,
                f"{coin.get('market_dominance', 0):.2f}%",
                f"{coin.get('circulating_supply', 0):,.0f}" if coin.get('circulating_supply') else "N/A",
                f"[{change_24h_color}]{price_change_24h:+.2f}%[/{change_24h_color}]",
                f"[{change_7d_color}]{price_change_7d:+.2f}%[/{change_7d_color}]",
                volume_str
            )
        
        return table
    
    def create_summary_panel(self, stats: Dict) -> Panel:
        """創建摘要面板"""
        total_cap = stats['total_market_cap']
        total_vol = stats['total_volume']
        
        # 計算前10大幣種的總佔比
        top10_dominance = sum(coin['market_dominance'] for coin in stats['coins'][:10])
        
        # BTC 和 ETH 的市值佔比
        btc_dominance = next((coin['market_dominance'] for coin in stats['coins'] if coin['symbol'] == 'btc'), 0)
        eth_dominance = next((coin['market_dominance'] for coin in stats['coins'] if coin['symbol'] == 'eth'), 0)
        
        summary_text = f"""
[bold cyan]市場總覽（排除穩定幣）[/bold cyan]
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

💰 [bold]總市值:[/bold] ${total_cap:,.0f}
📊 [bold]24h 交易量:[/bold] ${total_vol:,.0f}
🏆 [bold]BTC 主導率:[/bold] {btc_dominance:.2f}%
🥈 [bold]ETH 主導率:[/bold] {eth_dominance:.2f}%
📈 [bold]前10大幣種佔比:[/bold] {top10_dominance:.2f}%

[dim]按 Ctrl+C 退出[/dim]
"""
        
        return Panel(summary_text, title="市場摘要", border_style="green")
    
    def create_top_gainers_losers(self, stats: Dict) -> Panel:
        """創建漲跌幅排行"""
        coins = stats['coins']
        
        # 排序找出漲幅最大的
        gainers = sorted(
            [c for c in coins if c.get('price_change_percentage_24h', 0) > 0],
            key=lambda x: x.get('price_change_percentage_24h', 0),
            reverse=True
        )[:5]
        
        # 排序找出跌幅最大的
        losers = sorted(
            [c for c in coins if c.get('price_change_percentage_24h', 0) < 0],
            key=lambda x: x.get('price_change_percentage_24h', 0)
        )[:5]
        
        text = "[bold green]📈 24h 漲幅榜[/bold green]\n"
        for coin in gainers:
            change = coin.get('price_change_percentage_24h', 0)
            text += f"{coin['symbol'].upper():<6} [green]+{change:.2f}%[/green]\n"
        
        text += "\n[bold red]📉 24h 跌幅榜[/bold red]\n"
        for coin in losers:
            change = coin.get('price_change_percentage_24h', 0)
            text += f"{coin['symbol'].upper():<6} [red]{change:.2f}%[/red]\n"
        
        return Panel(text, title="漲跌排行", border_style="yellow")
    
    def run(self):
        """運行顯示器"""
        with Live(refresh_per_second=1) as live:
            while True:
                try:
                    # 獲取數據
                    coins = self.get_market_data(limit=100)
                    if not coins:
                        time.sleep(10)
                        continue
                    
                    # 計算統計
                    stats = self.calculate_market_stats(coins)
                    
                    # 創建佈局
                    layout = Layout()
                    layout.split_column(
                        Layout(self.create_summary_panel(stats), size=10),
                        Layout(name="main"),
                        Layout(self.create_top_gainers_losers(stats), size=15)
                    )
                    
                    # 主表格
                    layout["main"].update(self.create_market_table(stats))
                    
                    # 更新顯示
                    live.update(layout)
                    
                    # 等待下次更新
                    time.sleep(self.refresh_interval)
                    
                except KeyboardInterrupt:
                    break
                except Exception as e:
                    self.console.print(f"[red]錯誤: {e}[/red]")
                    time.sleep(5)

def main():
    """主程序"""
    console.clear()
    console.print("[bold cyan]加密貨幣市值即時監控系統[/bold cyan]")
    console.print("[dim]資料來源: CoinGecko API[/dim]\n")
    
    viewer = CryptoMarketViewer()
    
    try:
        viewer.run()
    except KeyboardInterrupt:
        console.print("\n[yellow]程式已停止[/yellow]")
    except Exception as e:
        console.print(f"\n[red]發生錯誤: {e}[/red]")

if __name__ == "__main__":
    main()