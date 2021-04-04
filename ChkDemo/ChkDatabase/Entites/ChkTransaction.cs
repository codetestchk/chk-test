using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChkDatabase.Entites
{
    public class ChkTransaction
    {
        public Guid ID { get; set; }
        public Guid MerchantID { get; set; }
        public string CardNameOnCard { get; set; }
        public string CardNumber { get; set; }
        public int CardExpMonth { get; set; }
        public int CardExpYear { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal Amount { get; set; }
        public int CurrencyID { get; set; }
        public int CardCVV { get; set; }
        // Maps to TransactionStateEnum
        public int StateID { get; set; }
        public DateTime DateTimeStateLastUpdated { get; set; }
        // Used incase the bank gives us a string message back, e.g. on failure
        public string StateMessage { get; set; }

        public virtual ChkMerchant Merchant { get; set; }
        // Bank will have it's own internal transaction ID
        public Guid BankTransactionID { get; set; }
    }
}
