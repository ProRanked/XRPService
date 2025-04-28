using System.Text.Json.Serialization;

namespace XRPService.Services.XrplClient
{
    /// <summary>
    /// Represents a payment transaction on the XRP Ledger
    /// </summary>
    public class PaymentTransaction
    {
        [JsonPropertyName("TransactionType")]
        public string TransactionType { get; set; } = "Payment";
        
        [JsonPropertyName("Account")]
        public string Account { get; set; } = string.Empty;
        
        [JsonPropertyName("Destination")]
        public string Destination { get; set; } = string.Empty;
        
        [JsonPropertyName("Amount")]
        public string Amount { get; set; } = string.Empty;
        
        [JsonPropertyName("Sequence")]
        public int Sequence { get; set; }
        
        [JsonPropertyName("Fee")]
        public string Fee { get; set; } = "12";  // Default fee in drops
        
        [JsonPropertyName("LastLedgerSequence")]
        public int? LastLedgerSequence { get; set; }
        
        [JsonPropertyName("Memos")]
        public List<Memo>? Memos { get; set; }
        
        /// <summary>
        /// Converts XRP amount to drops (1 XRP = 1,000,000 drops)
        /// </summary>
        public static string XrpToDrops(decimal xrp)
        {
            return Convert.ToInt64(xrp * 1_000_000).ToString();
        }
        
        /// <summary>
        /// Adds a memo to the transaction
        /// </summary>
        public void AddMemo(string memo)
        {
            Memos ??= new List<Memo>();
            
            var memoObj = new Memo
            {
                MemoField = new MemoDataField
                {
                    MemoData = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(memo))
                }
            };
            
            Memos.Add(memoObj);
        }
    }
    
    /// <summary>
    /// Memo structure for XRP transactions
    /// </summary>
    public class Memo
    {
        [JsonPropertyName("Memo")]
        public MemoDataField MemoField { get; set; } = new();
    }
    
    /// <summary>
    /// Memo data structure
    /// </summary>
    public class MemoDataField
    {
        [JsonPropertyName("MemoData")]
        public string MemoData { get; set; } = string.Empty;
    }
}