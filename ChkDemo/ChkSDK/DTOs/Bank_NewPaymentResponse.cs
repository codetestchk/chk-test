using System;
namespace ChkSDK.DTOs
{
    public class Bank_NewPaymentResponse
    {
        public Guid BankTransactionID { get; set; }
        public TransactionStateEnum TransactionState { get; set; }
        public string Message { get; set; }
    }
}
