using Newtonsoft.Json;
using Owin.SelfHosted.API.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Owin.SelfHosted.API.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Client is Up and Running");
            Console.WriteLine("Client is conncted with server");
            Run().Wait();
            Console.ReadLine();
        }

        static async Task Run()
        {

            var authenticationClient = new AuthenticationClient("http://localhost:8080");
            IDictionary<string, string> _tokenDictionary = null;
            string _accessToken = string.Empty;

            try
            {
                _tokenDictionary = await authenticationClient.GetTokenDictionary("john@example.com", "password");
                _accessToken = _tokenDictionary["access_token"];
                foreach (var item in _tokenDictionary)
                {
                    Console.WriteLine($"Key: {item.Key}, Value: {item.Value}");
                }

                Console.WriteLine("Get Companies Details from API");
                var companyClient = new CompanyClient("http://localhost:8080", _accessToken);
                var companies = companyClient.GetCompanies();
                foreach (var item in companies)
                {
                    Console.WriteLine($"Company Name: {item.Name}");
                }
            }
            catch (AggregateException ex)
            {
                // If it's an aggregate exception, an async error occurred:
                Console.WriteLine(ex.InnerExceptions[0].Message);
                Console.WriteLine("Press the Enter key to Exit...");
                Console.ReadLine();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                return;
            }
        }
    }


    public class CompanyClient
    {
        string hostURL = string.Empty;
        string accessToken = string.Empty;
        public CompanyClient(string url, string accessToken)
        {
            this.hostURL = url;
            this.accessToken = accessToken;
        }

        private void SetAuthentication(HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        }

        public HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(new Uri(hostURL), "api/companies");
            return client;
        }

        public IEnumerable<Company> GetCompanies()
        {
            HttpResponseMessage response;
            using (var httpClient = CreateClient())
            {
                SetAuthentication(httpClient);
                response = httpClient.GetAsync(httpClient.BaseAddress).Result;
            }
            var result = response.Content.ReadAsAsync<IEnumerable<Company>>().Result;
            return result;
        }
    }


    public class AuthenticationClient
    {
        string hostURL = string.Empty;

        public AuthenticationClient(string url)
        {
            hostURL = url;
        }

        public async Task<IDictionary<string, string>> GetTokenDictionary(string username, string password)
        {
            HttpResponseMessage responseMessage;
            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            };

            var content = new FormUrlEncodedContent(pairs);


            using (var client = new HttpClient())
            {
                var tokenEndPoint = new Uri(new Uri(hostURL), "Token");
                responseMessage = await client.PostAsync(tokenEndPoint, content);
            }

            var responseContent = await responseMessage.Content.ReadAsStringAsync();

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new Exception($"Error: {responseContent}");
            }

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
        }
    }
}
