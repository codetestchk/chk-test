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
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json", false, true)
                                .Build();

            MerchantSettings merchantSettings = new MerchantSettings();
            configuration.GetSection("MerchantSettings").Bind(merchantSettings);

            Console.WriteLine("Type how many merchants you want to set up in the backend, press enter");
            int numberOfMerchants = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Setting up merchants");
            NewMerchantInfo[] merchants = new NewMerchantInfo[numberOfMerchants];
            for (int i = 0; i < numberOfMerchants; i++)
            {
                merchants[i] = GenerateNewMerchant(merchantSettings.Gateway);
            }

            Random rnd = new Random();

            while (true)
            {
                Console.WriteLine("press enter to do a post, type end to exit");
                string input = Console.ReadLine();
                if (input == "exit")
                {
                    return;
                }

                // TODO ability to ask for a previous order

                Console.WriteLine("doing post");
                PostRandomPayment(merchantSettings, numberOfMerchants, merchants, rnd);
            }
        }

        private static void PostRandomPayment(MerchantSettings merchantSettings, int numberOfMerchants, NewMerchantInfo[] merchants, Random rnd)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(merchantSettings.Gateway);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var randomMerchant = merchants[rnd.Next(numberOfMerchants)];
                Console.WriteLine($"using merchant ID {randomMerchant.ID}");

                Merch_NewPaymentRequest paymentRequest = new Merch_NewPaymentRequest()
                {
                    Amount = 15.23M,
                    APIKey = randomMerchant.APIKey,
                    CardCVV = 123,
                    CardExpMonth = 11,
                    CardExpYear = 2022,
                    CardNameOnCard = "N J S",
                    CardNumber = GenerateRandomNumOfLength(16),
                    CurrencyID = (int)CurrencyEnum.EUR,
                    MerchantID = randomMerchant.ID
                };

                try
                {
                    HttpResponseMessage response = client.PostAsJsonAsync("payment/", paymentRequest).Result;
                    Console.WriteLine("Response status code :" + response.StatusCode);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Merch_NewPaymentResponse respObj = response.Content.ReadAsAsync<Merch_NewPaymentResponse>().Result;
                        Console.WriteLine($"Payment ID: {respObj.TransactionID} State: {respObj.TransactionState}, Message: {respObj.Message}");
                    }
                    else
                    {
                        Console.WriteLine($"Unsuccessful post, status code was : {response.StatusCode} message was : {response.Content}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception occured during post : {ex.Message}");
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

                HttpResponseMessage response = client.PostAsJsonAsync("merchant/", newMerch).Result;
                Console.WriteLine("Response status code :" + response.StatusCode);
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

    public class MerchantSettings
    {
        public string Gateway { get; set; }
    }
}
