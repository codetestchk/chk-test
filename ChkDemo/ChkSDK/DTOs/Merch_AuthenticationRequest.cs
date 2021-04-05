using System;
namespace ChkSDK.DTOs
{
    public class Merch_AuthenticationRequest
    {
        public Guid MerchantID { get; set; }
        public string MerchantAPIKey { get; set; }
    }
}
