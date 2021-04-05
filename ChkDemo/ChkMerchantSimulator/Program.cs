using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using ChkSDK;
using ChkSDK.DTOs;
using Microsoft.Extensions.Configuration;


namespace ChkMerchantSimulator
{
    /// <summary>
    /// Purpose of this application is to act like a merchant ( many merchants ), receiving orders, asking for orders etc. It simulates the Merchant side of things
    /// Console applicaiton which connects to the Gateway application API endpoint
    /// Setups up merchants first
    /// Then picks a random merchant to simulate an order from to the gateway
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json", false, true)
                                .Build();

            MerchantSimulatorSettings merchantSimulatorSettings = new MerchantSimulatorSettings();
            configuration.GetSection("MerchantSimulatorSettings").Bind(merchantSimulatorSettings);

            int numberOfMerchants = 5;
            NewMerchantInfo[] merchants = new NewMerchantInfo[numberOfMerchants];
            SetupMerchants(merchantSimulatorSettings, merchants);

            Random rnd = new Random();

            while (true)
            {
                Console.WriteLine("Press enter to do a post, type 'end' to exit");
                string input = Console.ReadLine();
                if (input == "exit")
                {
                    return;
                }

                var randomMerchant = merchants[rnd.Next(numberOfMerchants)];
                var bearerToken = GetBearerToken(merchantSimulatorSettings, randomMerchant);
                var postResult = PostRandomPayment(merchantSimulatorSettings, randomMerchant, bearerToken);
                if (postResult.Success)
                {
                    Console.WriteLine($"Getting existing Payment {postResult.TransactionID}");
                    var result = GetTransaction(merchantSimulatorSettings, postResult.TransactionID, randomMerchant, bearerToken);
                    if (result != null)
                    {
                        Console.WriteLine($"Get Existing Payment - SUCCESS - Payment ID: {postResult.TransactionID} State: {result.StateID}, Message: {result.StateMessage}, Card Number: {result.CardNumber}");
                    }
                    else
                    {
                        Console.WriteLine($"Get Existing Payment - FAILED - Payment not found for transactionID: {postResult.TransactionID}");
                    }
                }

            }
        }

        private static void SetupMerchants(MerchantSimulatorSettings merchantSimulatorSettings, NewMerchantInfo[] merchants)
        {
            Console.WriteLine("Setting up merchants");
            for (int i = 0; i < merchants.Length; i++)
            {
                merchants[i] = GenerateNewMerchant(merchantSimulatorSettings.Gateway);
            }
            Console.WriteLine("Setting up merchants - COMPLETE");
            Console.WriteLine();
        }


        private static PostNewTransactionResult PostRandomPayment(MerchantSimulatorSettings simSettings, NewMerchantInfo merchantToUse, string bearerToken)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(simSettings.Gateway);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                Merch_NewPaymentRequest paymentRequest = new Merch_NewPaymentRequest()
                {
                    Amount = 15.23M,
                    CardCVV = 123,
                    CardExpMonth = 11,
                    CardExpYear = 2022,
                    CardNameOnCard = "N J S",
                    CardNumber = GenerateRandomNumOfLength(16),
                    CurrencyID = (int)CurrencyEnum.EUR,
                    MerchantID = merchantToUse.ID
                };

                try
                {
                    Console.WriteLine("Posting new payment...");
                    HttpResponseMessage response = client.PostAsJsonAsync("payment/PostNewPayment", paymentRequest).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Merch_NewPaymentResponse respObj = response.Content.ReadAsAsync<Merch_NewPaymentResponse>().Result;
                        Console.WriteLine($"Post new payment SUCCESS - Payment ID: {respObj.TransactionID} State: {respObj.TransactionState}, Message: {respObj.Message}");
                        return new PostNewTransactionResult() { Success = true, TransactionID = respObj.TransactionID };
                    }
                    else
                    {
                        Console.WriteLine($"Post new payment FAILED - Unsuccessful post, status code was : {response.StatusCode} message was : {response.Content}");
                        return new PostNewTransactionResult() { Success = false, TransactionID = Guid.Empty };
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Post new payment FAILED - Exception occured during post : {ex.Message}");
                    return new PostNewTransactionResult() { Success = false, TransactionID = Guid.Empty };
                }
            }
        }

        private static string GetBearerToken(MerchantSimulatorSettings simSettings, NewMerchantInfo merchantInfo)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(simSettings.Gateway);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                Merch_AuthenticationRequest authenticationRequest = new Merch_AuthenticationRequest()
                {
                    MerchantAPIKey = merchantInfo.APIKey,
                    MerchantID = merchantInfo.ID
                };

                try
                {
                    HttpResponseMessage response = client.PostAsJsonAsync("merchant/Authenticate", authenticationRequest).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Merch_AuthenticationResponse respObj = response.Content.ReadAsAsync<Merch_AuthenticationResponse>().Result;
                        Console.WriteLine($"Authenticated");
                        return respObj.Token;
                    }
                    else
                    {
                        Console.WriteLine($"Authentication status code was : {response.StatusCode} message was : {response.Content}");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception occured during Authentication : {ex.Message}");
                    return null;
                }
            }
        }

        private static NewMerchantInfo GenerateNewMerchant(string gatewayApiEndpoint)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(gatewayApiEndpoint);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                Merch_NewMerchantRequest newMerch = new Merch_NewMerchantRequest()
                {
                    BankAcc = GenerateRandomBank(),
                    BankSort = GenerateRandomSort(),
                    Name = "Merchant Name " + GenerateRandomNumOfLength(5)
                };

                HttpResponseMessage response = client.PostAsJsonAsync("merchant/PostNewMerchant", newMerch).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Merch_NewMerchantResponse respObj = response.Content.ReadAsAsync<Merch_NewMerchantResponse>().Result;
                    Console.WriteLine($"New Merchant ID: {respObj.ID} API Key: {respObj.APIKey}");
                    return new NewMerchantInfo()
                    {
                        APIKey = respObj.APIKey,
                        ID = respObj.ID
                    };
                }
                else
                {
                    throw new Exception($"Unsuccessful new merchant creation, status code was : {response.StatusCode} message was : {response.Content}");
                }

            }
        }

        private static Merch_GetPaymentResponse GetTransaction(MerchantSimulatorSettings simSettings, Guid transactionID, NewMerchantInfo merchantInfo, string bearerToken)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(simSettings.Gateway);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);


                Merch_GetPaymentRequest getPaymentRequest = new Merch_GetPaymentRequest()
                {
                    PaymentID = transactionID
                };

                try
                {
                    HttpResponseMessage response = client.PostAsJsonAsync("payment/GetExistingPayment", getPaymentRequest).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return response.Content.ReadAsAsync<Merch_GetPaymentResponse>().Result;
                    }
                    else
                    {
                        Console.WriteLine($"Get Existing Payment FAILED - Unsuccessful get transaction id: {transactionID}, status code was : {response.StatusCode} message was : {response.Content}");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Get Existing Payment FAILED - Exception occured during get for transaction id, ex message : {ex.Message}");
                    return null;
                }
            }
        }

        private static string GenerateRandomBank()
        {
            return GenerateRandomNumOfLength(9);
        }

        private static string GenerateRandomSort()
        {
            string sortNumberOnly = GenerateRandomNumOfLength(6);

            return sortNumberOnly.Substring(0, 2) + '-' +
                sortNumberOnly.Substring(2, 2) + '-' +
                sortNumberOnly.Substring(4, 2);
        }

        private static string GenerateRandomNumOfLength(int length)
        {
            Random random = new Random();
            const string chars = "1234567890";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    public class PostNewTransactionResult
    {
        public bool Success { get; set; }
        public Guid TransactionID { get; set; }
    }

    public class MerchantSimulatorSettings
    {
        public string Gateway { get; set; }
    }
}
