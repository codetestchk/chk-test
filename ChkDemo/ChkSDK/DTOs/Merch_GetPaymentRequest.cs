using System;
namespace ChkSDK.DTOs
{
    public class Merch_GetPaymentRequest
    {
        public Guid PaymentID { get; set; }
        public Guid MerchantID { get; set; }
        public string APIKey { get; set; }
    }
}
