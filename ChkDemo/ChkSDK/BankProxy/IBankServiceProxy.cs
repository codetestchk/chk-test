using System.Threading.Tasks;
using ChkSDK.DTOs;

namespace ChkSDK.BankProxy
{
    public interface IBankServiceProxy
    {
        Task<Bank_NewPaymentResponse> ProcessPayment(Bank_NewPaymentRequest paymentRequest);
    }
}
