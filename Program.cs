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
            var companyClient = new CompanyClient("http://localhost:8080");
            Console.WriteLine("Client is conncted with server");
            Console.WriteLine("Get Companies Details from API");
            var companies = companyClient.GetCompanies();
            foreach (var item in companies)
            {
                Console.WriteLine($"Company Name: {item.Name}");
            }
            Console.ReadLine();
        }
    }


    public class CompanyClient
    {
        string hostURL = string.Empty;
        public CompanyClient(string url)
        {
            hostURL = url;
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
            using(var httpClient = CreateClient())
            {
                response = httpClient.GetAsync(httpClient.BaseAddress).Result;
            }
            var result = response.Content.ReadAsAsync<IEnumerable<Company>>().Result;
            return result;
        }
    }
}
