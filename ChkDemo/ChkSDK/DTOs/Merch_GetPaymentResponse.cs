using System;

namespace ChkSDK.DTOs
{
    public class Merch_GetPaymentResponse
    {
        public string CardNameOnCard { get; set; }
        public string CardNumber { get; set; }
        public int CardExpMonth { get; set; }
        public int CardExpYear { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyID { get; set; }
        // Maps to TransactionStateEnum
        public int StateID { get; set; }
        public DateTime DateTimeStateLastUpdated { get; set; }
        public string StateMessage { get; set; }
    }
}
