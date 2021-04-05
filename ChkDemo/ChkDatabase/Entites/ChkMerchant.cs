using System;
namespace ChkDatabase.Entites
{
    public class ChkMerchant
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string BankAcc { get; set; }
        public string BankSort { get; set; }
        // API key is auto generated, it's like a password, it's used to authenticate along with the ID to provide an access bearer token
        public string APIKey { get; set; }
    }
}
