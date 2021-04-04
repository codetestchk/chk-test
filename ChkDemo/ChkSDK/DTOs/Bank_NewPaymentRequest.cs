namespace ChkSDK.DTOs
{
    public class Bank_NewPaymentRequest
    {
        public SourcePaymentInfo SourcePaymentInfo { get; set; }
        public MerchantBankInfo DestinationMerchantInfo { get; set; }
    }

    public class SourcePaymentInfo
    {
        public string CardNameOnCard { get; set; }
        public string CardNumber { get; set; }
        public int CardExpMonth { get; set; }
        public int CardExpYear { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyID { get; set; }
        public int CardCVV { get; set; }
    }
}
