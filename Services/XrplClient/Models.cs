using System.Text.Json.Serialization;

namespace XRPService.Services.XrplClient
{
    /// <summary>
    /// Base class for all XRPL requests
    /// </summary>
    public abstract class XrplRequestBase
    {
        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;
        
        [JsonPropertyName("params")]
        public object[]? Parameters { get; set; }
    }
    
    /// <summary>
    /// Response from the XRPL API
    /// </summary>
    public class XrplResponse<T>
    {
        [JsonPropertyName("result")]
        public T Result { get; set; } = default!;
        
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        
        [JsonPropertyName("error")]
        public string? Error { get; set; }
        
        [JsonPropertyName("error_code")]
        public int? ErrorCode { get; set; }
        
        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }
    }
    
    /// <summary>
    /// Request for account_info command
    /// </summary>
    public class AccountInfoRequest : XrplRequestBase
    {
        public AccountInfoRequest()
        {
            Method = "account_info";
        }
        
        [JsonPropertyName("account")]
        public string Account { get; set; } = string.Empty;
        
        [JsonIgnore]
        public new object[] Parameters => new object[] { new { account = Account, strict = true, ledger_index = "current" } };
    }
    
    /// <summary>
    /// Result for account_info command
    /// </summary>
    public class AccountInfoResult
    {
        [JsonPropertyName("account_data")]
        public AccountData AccountData { get; set; } = new();
        
        [JsonPropertyName("ledger_index")]
        public int LedgerIndex { get; set; }
    }
    
    /// <summary>
    /// Account data structure
    /// </summary>
    public class AccountData
    {
        [JsonPropertyName("Account")]
        public string Account { get; set; } = string.Empty;
        
        [JsonPropertyName("Balance")]
        public string Balance { get; set; } = string.Empty;
        
        [JsonPropertyName("Sequence")]
        public int Sequence { get; set; }
        
        [JsonPropertyName("OwnerCount")]
        public int OwnerCount { get; set; }
    }
    
    /// <summary>
    /// Request for tx command
    /// </summary>
    public class TxRequest : XrplRequestBase
    {
        public TxRequest()
        {
            Method = "tx";
        }
        
        [JsonPropertyName("transaction")]
        public string Transaction { get; set; } = string.Empty;
        
        [JsonIgnore]
        public new object[] Parameters => new object[] { new { transaction = Transaction } };
    }
    
    /// <summary>
    /// Result for tx command
    /// </summary>
    public class TransactionResult
    {
        [JsonPropertyName("hash")]
        public string Hash { get; set; } = string.Empty;
        
        [JsonPropertyName("ledger_index")]
        public int LedgerIndex { get; set; }
        
        [JsonPropertyName("meta")]
        public object? Meta { get; set; }
        
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        
        [JsonPropertyName("validated")]
        public bool Validated { get; set; }
    }
    
    /// <summary>
    /// Request for submit command
    /// </summary>
    public class SubmitRequest : XrplRequestBase
    {
        public SubmitRequest()
        {
            Method = "submit";
        }
        
        [JsonPropertyName("tx_blob")]
        public string TxBlob { get; set; } = string.Empty;
        
        [JsonIgnore]
        public new object[] Parameters => new object[] { new { tx_blob = TxBlob } };
    }
    
    /// <summary>
    /// Result for submit command
    /// </summary>
    public class SubmitResult
    {
        [JsonPropertyName("tx_json")]
        public TxJson TxJson { get; set; } = new();
        
        [JsonPropertyName("engine_result")]
        public string EngineResult { get; set; } = string.Empty;
        
        [JsonPropertyName("engine_result_code")]
        public int EngineResultCode { get; set; }
        
        [JsonPropertyName("engine_result_message")]
        public string EngineResultMessage { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Transaction JSON
    /// </summary>
    public class TxJson
    {
        [JsonPropertyName("hash")]
        public string Hash { get; set; } = string.Empty;
    }
}