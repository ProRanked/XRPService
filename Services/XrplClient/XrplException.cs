namespace XRPService.Services.XrplClient
{
    /// <summary>
    /// Exception thrown when an error occurs in the XRPL API
    /// </summary>
    public class XrplException : Exception
    {
        /// <summary>
        /// The error code from the XRPL API
        /// </summary>
        public int ErrorCode { get; }
        
        /// <summary>
        /// The detailed error message from the XRPL API
        /// </summary>
        public string ErrorMessage { get; }
        
        public XrplException(string error, int errorCode, string errorMessage)
            : base($"XRPL Error: {error} (Code: {errorCode})")
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
    }
}