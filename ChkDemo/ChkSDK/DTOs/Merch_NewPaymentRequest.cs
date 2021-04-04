using System;
namespace ChkSDK.DTOs
{
    public class Merch_NewPaymentRequest
    {
        public Guid MerchantID { get; set; }
        public string CardNameOnCard { get; set; }
        public string CardNumber { get; set; }
        public int CardExpMonth { get; set; }
        public int CardExpYear { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyID { get; set; }
        public int CardCVV { get; set; }
        public string APIKey { get; set; }
    }
}
