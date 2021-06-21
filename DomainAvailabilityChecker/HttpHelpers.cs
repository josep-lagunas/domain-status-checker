using System;
using System.Net;
using System.Threading;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace DomainAvailabilityChecker
{
    public static class HttpHelpers
    {
        private static readonly IRestClient Client;
        private const int MaxRequestRetries = 3;
        private const double RetryWaitDelaySeconds = 1;
   
        private const string Url =
            "https://es.godaddy.com/domainfind/v1/search/exact?key=dpp_search&" +
            "partialQuery=%3C<DOMAIN>%3E&q=%3C<DOMAIN>%3E&req_id=1614996565310&" +
            "solution_set_ids=dpp-us-solution-tier1%2Cdpp-intl-solution-tier4%2Cdpp" +
            "-intl-solution-tier6%2Co365-solutionset-tier3%2Cdpp-us-solution-fixed-tier4&" +
            "itc=dpp_absol1&isc=NPEUR19ES";

        static HttpHelpers()
        {
           Client = new RestClient();
        }
        
        internal static bool DoRequest(string domain, out JObject jobject)
        {
            var url = new Uri(Url.Replace("<DOMAIN>", domain));

            var request = new RestRequest(url, Method.GET, DataFormat.Json);
            request.AddCookie("currency", "EUR");
            IRestResponse response;

            var retries = 0;
            do
            {
                response = Client.Execute(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    retries++;
                    //Console.WriteLine(
                    //    $"Problems consulting domain: {domain} ({response.StatusCode} " +
                    //    $"{response.StatusDescription}), retrying in {RetryWaitDelaySeconds * retries}s...");
                    Thread.Sleep(TimeSpan.FromSeconds(RetryWaitDelaySeconds * retries));
                }
            } while (response.StatusCode != HttpStatusCode.OK && retries < MaxRequestRetries);

            if (retries >= MaxRequestRetries)
            {
                Console.WriteLine(
                    $"Max requests retries ({retries}) reached, {domain} will be skipped.");
                jobject = default;
            }
            else
            {
                jobject = JObject.Parse(response.Content);
            }

            return retries < MaxRequestRetries;
        }
    }
}