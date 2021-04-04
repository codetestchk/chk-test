using System;
namespace ChkDatabase.Entites
{
    public class ChkMerchant
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string BankAcc { get; set; }
        public string BankSort { get; set; }
        public string APIKey { get; set; }
    }
}
