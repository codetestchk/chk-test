using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ChkSDK.DTOs;

namespace ChkSDK.BankProxy
{
    public class FakeBankServiceProxy : IBankServiceProxy
    {
        public async Task<Bank_NewPaymentResponse> ProcessPayment(Bank_NewPaymentRequest paymentRequest)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Wait for a period of time, to simulate real world.
            var rand = new Random();
            await Task.Delay(rand.Next(1000, 5000));

            // We do a random failure thing here, so we can simulate it failing every now and again
            // Failure 10% of the time
            if (rand.Next(10) == 5)
            {
                return new Bank_NewPaymentResponse()
                {
                    BankTransactionID = Guid.NewGuid(),
                    Message = $"Failed, took {sw.ElapsedMilliseconds}ms",
                    TransactionState = TransactionStateEnum.Failed
                };
            }
            else
            {
                return new Bank_NewPaymentResponse()
                {
                    BankTransactionID = Guid.NewGuid(),
                    Message = $"Successful, took {sw.ElapsedMilliseconds}ms",
                    TransactionState = TransactionStateEnum.Succeeded
                };
            }
        }
    }
}
