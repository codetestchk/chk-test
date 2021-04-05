using System;
using System.Threading.Tasks;
using ChkDatabase;
using ChkDatabase.Entites;
using ChkSDK.DTOs;
using ChkSDK.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ChkSDK.Services
{
    public interface ITransactionService
    {
        Task UpdateTransactionStatus(Guid id, TransactionStateEnum transactionState, string message, Guid bankTransactionID);
        Task<Guid> CreateNewTransaction(Merch_NewPaymentRequest newTransaction);
        Task<Merch_GetPaymentResponse> GetTransaction(Guid transactionID, Guid merchantID);
    }

    public class TransactionService : ITransactionService
    {
        private readonly ChkDbContext _dbContext;

        public TransactionService(ChkDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Creates a new transaction into DB, generates ID, begins with state 'processing'
        /// </summary>
        /// <param name="newTransaction"></param>
        /// <returns>Returns transaction ID within Chkout payments, for future reference by the merchant</returns>
        public async Task<Guid> CreateNewTransaction(Merch_NewPaymentRequest newTransaction)
        {
            Guid transactionGuid = Guid.NewGuid();
            _dbContext.Transactions.Add(new ChkTransaction()
            {
                Amount = newTransaction.Amount,
                CardCVV = newTransaction.CardCVV,
                CardExpMonth = newTransaction.CardExpMonth,
                CardExpYear = newTransaction.CardExpYear,
                CardNameOnCard = newTransaction.CardNameOnCard,
                CardNumber = newTransaction.CardNumber,
                CurrencyID = newTransaction.CurrencyID,
                DateTimeStateLastUpdated = DateTime.UtcNow,
                ID = transactionGuid,
                MerchantID = newTransaction.MerchantID,
                StateID = (int)TransactionStateEnum.Processing,
                StateMessage = ""
            });
            await _dbContext.SaveChangesAsync();
            return transactionGuid;
        }

        public async Task UpdateTransactionStatus(Guid id, TransactionStateEnum transactionState, string message, Guid bankTransactionID)
        {
            var transactionDbObj = await _dbContext.Transactions.SingleAsync(a => a.ID == id);
            transactionDbObj.StateID = (int)transactionState;
            transactionDbObj.StateMessage = message;
            transactionDbObj.DateTimeStateLastUpdated = DateTime.UtcNow;
            transactionDbObj.BankTransactionID = bankTransactionID;
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Get existing trasnaction
        /// </summary>
        /// <param name="transactionID">The transaction guid that we want info on</param>
        /// <param name="merchantID">Only get transactions for this merchant provided key</param>
        /// <returns>Will return NULL if not found, OR merchant ID does not have access to this transaction</returns>
        public async Task<Merch_GetPaymentResponse> GetTransaction(Guid transactionID, Guid merchantID)
        {
            var record = await _dbContext.Transactions.SingleOrDefaultAsync(a => a.ID == transactionID && a.Merchant.ID == merchantID);
            if(record != null)
            {
                return new Merch_GetPaymentResponse()
                {
                    Amount = record.Amount,
                    CardExpMonth = record.CardExpMonth,
                    CardExpYear = record.CardExpYear,
                    CardNameOnCard = record.CardNameOnCard,
                    CurrencyID = record.CurrencyID,
                    DateTimeStateLastUpdated = record.DateTimeStateLastUpdated,
                    StateID = record.StateID,
                    StateMessage = record.StateMessage,
                    CardNumber = CardDataHelper.HideCardNumber(record.CardNumber)
                };
            }
            else
            {
                return null;
            }
        }
    }
}
