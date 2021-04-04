using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ChkSDK.DTOs;

namespace ChkSDK.BankProxy
{
    public class RealBankServiceProxy : IBankServiceProxy
    {
        private readonly string _apiEndpoint;
        public RealBankServiceProxy(string apiEndpoint)
        {
            _apiEndpoint = apiEndpoint;
        }

        public async Task<Bank_NewPaymentResponse> ProcessPayment(Bank_NewPaymentRequest paymentRequest)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiEndpoint);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.PostAsJsonAsync("payment/", paymentRequest);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsAsync<Bank_NewPaymentResponse>(); ;
            }
        }
    }
}
