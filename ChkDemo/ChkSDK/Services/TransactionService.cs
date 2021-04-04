using System;
using System.Linq;
using System.Threading.Tasks;
using ChkDatabase;
using ChkDatabase.Entites;
using ChkSDK.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ChkSDK.Services
{
    public interface ITransactionService
    {
        Task UpdateTransactionStatus(Guid id, TransactionStateEnum transactionState, string message, Guid bankTransactionID);
        Task<Guid> CreateNewTransaction(Merch_NewPaymentRequest newTransaction);
    }

    public class TransactionService : ITransactionService
    {
        private readonly ChkDbContext _dbContext;

        public TransactionService(ChkDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Creates a new transaction into DB, generates ID
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
        }
    }
}
