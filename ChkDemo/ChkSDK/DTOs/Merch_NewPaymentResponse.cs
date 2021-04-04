using System;

namespace ChkSDK.DTOs
{
    public class Merch_NewPaymentResponse
    {
        public Guid TransactionID { get; set; }
        public TransactionStateEnum TransactionState { get; set; }
        public string Message { get; set; }
    }
}
