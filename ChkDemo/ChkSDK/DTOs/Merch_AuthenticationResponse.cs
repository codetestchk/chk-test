using System;
namespace ChkSDK.DTOs
{
    public class Merch_AuthenticationResponse
    {
        public Guid MerchantID { get; set; }
        public string Token { get; set; }
        public string MerchantNmae { get; set; }
    }
}
