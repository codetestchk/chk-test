using System;
using System.Linq;
using System.Threading.Tasks;
using ChkDatabase;
using ChkDatabase.Entites;
using ChkSDK.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ChkSDK.Services
{
    public interface IMerchantService
    {
        Task<bool> ValidateIDApiKey(Guid merchantID, string merchantAPIKey);
        Task<MerchantBankInfo> GetMerchantPaymentDetails(Guid merchantID);
        Task<NewMerchantInfo> CreateNewMerchant(string bankAcc, string bankSort, string merchName);
    }

    public class MerchantService : IMerchantService
    {
        private readonly ChkDbContext _dbContext;

        public MerchantService(ChkDbContext chkDbContext)
        {
            _dbContext = chkDbContext;
        }

        /// <summary>
        /// Retrieves just the sort/bank account information for a merchant ID, must exist, throws exception if not
        /// </summary>
        /// <param name="merchantID"></param>
        /// <returns></returns>
        public async Task<MerchantBankInfo> GetMerchantPaymentDetails(Guid merchantID)
        {
            var merchantObj = await _dbContext.Merchants.SingleAsync(a => a.ID == merchantID);
            return new MerchantBankInfo() { BankAcc = merchantObj.BankAcc, BankSort = merchantObj.BankSort };
        }

        /// <summary>
        /// Validates that a provided merchant ID and API key are in existence in the DB, used as a sort of auth.
        /// </summary>
        /// <param name="merchantID"></param>
        /// <param name="merchantAPIKey"></param>
        /// <returns></returns>
        public async Task<bool> ValidateIDApiKey(Guid merchantID, string merchantAPIKey)
        {
            return await _dbContext.Merchants.AnyAsync(a => a.ID == merchantID && a.APIKey == merchantAPIKey);
        }

        /// <summary>
        /// Generates ID and API Key and adds new merchant to database
        /// </summary>
        /// <param name="newMerchantRequest"></param>
        /// <returns>APIKey and ID</returns>
        public async Task<NewMerchantInfo> CreateNewMerchant(string bankAcc, string bankSort, string merchName)
        {
            string apiKey = GenerateRandomAPIKey();
            Guid merchantID = Guid.NewGuid();

            _dbContext.Merchants.Add(new ChkMerchant()
            {
                APIKey = apiKey,
                ID = merchantID,
                BankAcc = bankAcc,
                BankSort = bankSort,
                Name = merchName
            });

            await _dbContext.SaveChangesAsync();

            return new NewMerchantInfo()
            {
                APIKey = apiKey,
                ID = merchantID
            };
        }

        private string GenerateRandomAPIKey()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 20)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
