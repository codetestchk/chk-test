using System;
using System.Linq;
using System.Threading.Tasks;
using ChkDatabase;
using ChkDatabase.Entites;
using ChkSDK.DTOs;
using ChkSDK.Helpers;
using ChkSDK.SettingsModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChkSDK.Services
{
    public interface IMerchantService
    {
        Task<bool> ValidateIDApiKey(Guid merchantID, string merchantAPIKey);
        Task<MerchantBankInfo> GetMerchantPaymentDetails(Guid merchantID);
        Task<NewMerchantInfo> CreateNewMerchant(string bankAcc, string bankSort, string merchName);
        Task<Merch_AuthenticationResponse> Authenticate(Merch_AuthenticationRequest authRequest);
        Task<bool> MerchantIDExists(Guid merchantID);
    }

    public class MerchantService : IMerchantService
    {
        private readonly ChkDbContext _dbContext;
        private readonly JwtSettings _jwtSettings;

        public MerchantService(ChkDbContext chkDbContext, IOptions<JwtSettings> jwtSettings)
        {
            _dbContext = chkDbContext;
            _jwtSettings = jwtSettings.Value;
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
            // Generation of api key is akin to a password. Client is expected to store this securely like a password.
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
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 20)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Generates JwtToken if ID and APIKey match that in database
        /// </summary>
        /// <param name="authRequest">ID and API Key</param>
        /// <returns>Returns null if authenticaiton fails</returns>
        public async Task<Merch_AuthenticationResponse> Authenticate(Merch_AuthenticationRequest authRequest)
        {
            var merch = await _dbContext.Merchants.SingleOrDefaultAsync(a => a.ID == authRequest.MerchantID && a.APIKey == authRequest.MerchantAPIKey);

            if(merch == null)
            {
                return null;
            }

            var authToken = AuthHelper.GenerateJWTToken(merch.ID, merch.Name, _jwtSettings.SecretKey, _jwtSettings.ExpiryHours);

            return new Merch_AuthenticationResponse()
            {
                MerchantID = merch.ID,
                MerchantNmae = merch.Name,
                Token = authToken
            };
        }

        public async Task<bool> MerchantIDExists(Guid merchantID)
        {
            return await _dbContext.Merchants.AnyAsync(a => a.ID == merchantID);
        }
    }
}
