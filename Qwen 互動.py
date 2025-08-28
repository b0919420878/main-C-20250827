import torch
import os
import warnings
warnings.filterwarnings('ignore')

from transformers import (
    AutoModelForCausalLM,
    AutoTokenizer,
    BitsAndBytesConfig,
    TextStreamer
)
from peft import PeftModel, PeftConfig
import time

class QwenFinancialChat:
    """Qwen財報分析聊天機器人"""
    
    def __init__(self, model_path="./qwen-financial-model", base_model_name="Qwen/Qwen2.5-7B"):
        """
        初始化聊天機器人
        Args:
            model_path: 訓練好的模型路徑
            base_model_name: 基礎模型名稱
        """
        self.model_path = model_path
        self.base_model_name = base_model_name
        self.model = None
        self.tokenizer = None
        self.streamer = None
        
    def load_model(self):
        """載入模型"""
        print("="*60)
        print("載入財報分析AI模型...")
        print("="*60)
        
        try:
            # 4-bit量化配置
            bnb_config = BitsAndBytesConfig(
                load_in_4bit=True,
                bnb_4bit_use_double_quant=True,
                bnb_4bit_quant_type="nf4",
                bnb_4bit_compute_dtype=torch.float16
            )
            
            # 載入tokenizer
            print("載入tokenizer...")
            self.tokenizer = AutoTokenizer.from_pretrained(
                self.model_path,
                trust_remote_code=True,
                use_fast=False
            )
            
            # 設定pad token
            if self.tokenizer.pad_token is None:
                self.tokenizer.pad_token = self.tokenizer.eos_token
            
            # 載入基礎模型
            print("載入基礎模型（需要幾分鐘）...")
            base_model = AutoModelForCausalLM.from_pretrained(
                self.base_model_name,
                quantization_config=bnb_config,
                device_map="auto",
                trust_remote_code=True,
                torch_dtype=torch.float16,
                low_cpu_mem_usage=True
            )
            
            # 載入LoRA權重
            print("載入微調權重...")
            self.model = PeftModel.from_pretrained(
                base_model,
                self.model_path,
                torch_dtype=torch.float16
            )
            
            # 設定為評估模式
            self.model.eval()
            
            # 設定串流輸出（即時顯示生成的文字）
            self.streamer = TextStreamer(
                self.tokenizer,
                skip_prompt=True,
                skip_special_tokens=True
            )
            
            print("✅ 模型載入成功！")
            print("="*60)
            
            # 顯示GPU資訊
            if torch.cuda.is_available():
                gpu_name = torch.cuda.get_device_name(0)
                gpu_memory = torch.cuda.get_device_properties(0).total_memory / 1024**3
                print(f"使用GPU: {gpu_name} ({gpu_memory:.1f}GB)")
            else:
                print("使用CPU（速度較慢）")
            
            return True
            
        except Exception as e:
            print(f"❌ 模型載入失敗: {e}")
            return False
    
    def generate_response(self, prompt, max_length=512, temperature=0.3, top_p=0.9):
        """
        生成回應
        Args:
            prompt: 輸入問題
            max_length: 最大生成長度
            temperature: 溫度參數（0-1，越高越隨機）
            top_p: Top-p採樣參數
        """
        # 準備輸入
        inputs = self.tokenizer(
            prompt,
            return_tensors="pt",
            truncation=True,
            max_length=512
        ).to(self.model.device)
        
        # 生成回應
        with torch.no_grad():
            outputs = self.model.generate(
                **inputs,
                max_new_tokens=max_length,
                temperature=temperature,
                do_sample=True,
                top_p=top_p,
                pad_token_id=self.tokenizer.pad_token_id,
                eos_token_id=self.tokenizer.eos_token_id,
                streamer=self.streamer,  # 即時輸出
                repetition_penalty=1.2,  # 避免重複
            )
        
        # 解碼完整回應
        response = self.tokenizer.decode(outputs[0], skip_special_tokens=True)
        
        # 移除原始問題
        response = response.replace(prompt, "").strip()
        
        return response
    
    def chat(self):
        """互動式聊天"""
        print("\n" + "="*60)
        print("台股財報分析AI助手")
        print("="*60)
        print("輸入 'quit' 或 'exit' 結束對話")
        print("輸入 'help' 查看使用說明")
        print("="*60)
        
        # 預設問題範例
        examples = [
            "台積電的財務表現如何？",
            "請分析鴻海2024年第3季的營運狀況",
            "聯發科的獲利能力好嗎？",
            "哪些半導體股票值得投資？",
            "比較台積電和聯電的財務表現",
            "分析台達電的成長動能",
            "大立光的毛利率表現如何？",
        ]
        
        print("\n範例問題：")
        for i, example in enumerate(examples, 1):
            print(f"{i}. {example}")
        
        # 開始對話循環
        while True:
            print("\n" + "-"*60)
            user_input = input("\n你的問題: ").strip()
            
            # 檢查退出指令
            if user_input.lower() in ['quit', 'exit', 'bye', '結束']:
                print("\n感謝使用！再見！")
                break
            
            # 顯示說明
            if user_input.lower() == 'help':
                self.show_help()
                continue
            
            # 處理數字選擇（範例問題）
            if user_input.isdigit():
                idx = int(user_input) - 1
                if 0 <= idx < len(examples):
                    user_input = examples[idx]
                    print(f"選擇範例: {user_input}")
            
            # 空輸入
            if not user_input:
                print("請輸入問題！")
                continue
            
            # 生成回應
            print("\nAI回答: ", end="")
            start_time = time.time()
            
            try:
                response = self.generate_response(user_input)
                
                # 顯示生成時間
                elapsed_time = time.time() - start_time
                print(f"\n\n(生成時間: {elapsed_time:.1f}秒)")
                
            except Exception as e:
                print(f"\n❌ 生成失敗: {e}")
    
    def show_help(self):
        """顯示使用說明"""
        print("\n" + "="*60)
        print("使用說明")
        print("="*60)
        print("""
1. 直接輸入問題，AI會分析財報並回答
2. 輸入數字(1-7)選擇範例問題
3. 可以詢問的內容：
   - 個股財務分析（如：台積電的財務表現）
   - 產業比較（如：比較鴻海和廣達）
   - 投資建議（如：哪些股票值得投資）
   - 財務指標查詢（如：XX公司的ROE是多少）
   
4. 特殊指令：
   - quit/exit: 結束對話
   - help: 顯示此說明
   
5. 提示：
   - 問題越具體，回答越準確
   - 可以指定年度和季度
   - 可以要求比較多家公司
        """)
    
    def batch_analysis(self, companies):
        """批次分析多家公司"""
        print("\n批次分析模式")
        print("="*60)
        
        results = {}
        
        for company in companies:
            print(f"\n分析 {company}...")
            prompt = f"請詳細分析{company}的財務狀況並給出投資建議"
            
            try:
                response = self.generate_response(prompt, max_length=300)
                results[company] = response
                print(f"✅ {company} 分析完成")
            except Exception as e:
                results[company] = f"分析失敗: {e}"
                print(f"❌ {company} 分析失敗")
        
        # 儲存結果
        with open("batch_analysis_results.txt", "w", encoding="utf-8") as f:
            for company, analysis in results.items():
                f.write(f"\n{'='*60}\n")
                f.write(f"{company} 分析報告\n")
                f.write(f"{'='*60}\n")
                f.write(f"{analysis}\n")
        
        print(f"\n✅ 批次分析完成，結果已儲存至 batch_analysis_results.txt")
        
        return results

# ========================================
# 簡化版快速啟動
# ========================================

class SimpleQwenChat:
    """簡化版聊天（如果完整版載入失敗）"""
    
    def __init__(self, model_path="./qwen-financial-model"):
        self.model_path = model_path
        
    def quick_chat(self):
        """快速聊天模式"""
        print("\n簡化版財報AI助手")
        print("="*60)
        
        # 這裡可以連接到API或使用更簡單的模型
        print("模擬模式：回應為預設答案")
        
        responses = {
            "台積電": "台積電是台灣半導體龍頭，財務表現優秀，ROE約20%，值得長期投資。",
            "鴻海": "鴻海是全球最大代工廠，營收穩定但毛利率較低，適合穩健型投資人。",
            "聯發科": "聯發科在5G晶片市場表現出色，成長動能強勁。",
        }
        
        while True:
            user_input = input("\n你的問題: ").strip()
            
            if user_input.lower() in ['quit', 'exit']:
                print("再見！")
                break
            
            # 簡單的關鍵字匹配
            answered = False
            for key, response in responses.items():
                if key in user_input:
                    print(f"\n回答: {response}")
                    answered = True
                    break
            
            if not answered:
                print("\n回答: 抱歉，我需要更多訓練才能回答這個問題。")

# ========================================
# 主程式
# ========================================

def main():
    """主程式"""
    
    print("\n" + "="*70)
    print("台股財報分析AI系統")
    print("="*70)
    
    # 選擇模式
    print("\n選擇模式：")
    print("1. 互動式對話")
    print("2. 批次分析")
    print("3. 簡化版（如果模型載入失敗）")
    
    choice = input("\n請選擇 (1/2/3): ").strip()
    
    if choice == "3":
        # 簡化版
        simple_chat = SimpleQwenChat()
        simple_chat.quick_chat()
        
    else:
        # 完整版
        chat_bot = QwenFinancialChat(
            model_path="./qwen-financial-model",
            base_model_name="Qwen/Qwen2.5-7B"
        )
        
        # 載入模型
        if not chat_bot.load_model():
            print("\n改用簡化版...")
            simple_chat = SimpleQwenChat()
            simple_chat.quick_chat()
            return
        
        if choice == "1":
            # 互動式對話
            chat_bot.chat()
            
        elif choice == "2":
            # 批次分析
            companies = [
                "台積電", "聯電", "聯發科", "瑞昱",
                "鴻海", "廣達", "緯創", "和碩",
                "台達電", "大立光", "玉晶光"
            ]
            chat_bot.batch_analysis(companies)
        
        else:
            # 預設為互動式
            chat_bot.chat()

if __name__ == "__main__":
    main()