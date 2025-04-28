using System.Text;
using System.Text.Json;

namespace XRPService.Services.XrplClient
{
    /// <summary>
    /// A simple client for interacting with the XRP Ledger
    /// </summary>
    public class XrplClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _serverUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public XrplClient(HttpClient httpClient, string serverUrl)
        {
            _httpClient = httpClient;
            _serverUrl = serverUrl;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        /// <summary>
        /// Sends a request to the XRP Ledger
        /// </summary>
        public async Task<T> SendRequest<T>(XrplRequestBase request, CancellationToken cancellationToken = default)
        {
            // Create the request object with method and parameters
            var requestObj = new 
            { 
                method = request.Method, 
                // Use the Parameters property from the request object
                @params = request.Parameters 
            };
            
            var jsonRequest = JsonSerializer.Serialize(requestObj, _jsonOptions);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(_serverUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<XrplResponse<T>>(jsonResponse, _jsonOptions);
            
            if (result == null)
            {
                throw new Exception("Failed to deserialize response");
            }
            
            if (result.Status == "error")
            {
                throw new XrplException(result.Error ?? "Unknown error", result.ErrorCode ?? 0, result.ErrorMessage ?? string.Empty);
            }
            
            return result.Result;
        }
        
        /// <summary>
        /// Gets account information
        /// </summary>
        public Task<AccountInfoResult> GetAccountInfo(string account, CancellationToken cancellationToken = default)
        {
            var request = new AccountInfoRequest { Account = account };
            return SendRequest<AccountInfoResult>(request, cancellationToken);
        }
        
        /// <summary>
        /// Gets transaction information
        /// </summary>
        public Task<TransactionResult> GetTransaction(string hash, CancellationToken cancellationToken = default)
        {
            var request = new TxRequest { Transaction = hash };
            return SendRequest<TransactionResult>(request, cancellationToken);
        }
        
        /// <summary>
        /// Submits a signed transaction
        /// </summary>
        public Task<SubmitResult> SubmitTransaction(string txBlob, CancellationToken cancellationToken = default)
        {
            var request = new SubmitRequest { TxBlob = txBlob };
            return SendRequest<SubmitResult>(request, cancellationToken);
        }
    }
}