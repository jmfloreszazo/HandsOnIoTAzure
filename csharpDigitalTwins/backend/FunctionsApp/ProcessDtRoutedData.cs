using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FunctionsApp
{
    public static class ProcessDtRoutedData
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private static readonly string AdtServiceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");

        [FunctionName("ProcessDTRoutedData")]
        public static async Task Run([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
        {
            log.LogInformation("Init Function");
            DigitalTwinsClient client;
            try
            {
                var credentials = new DefaultAzureCredential();
                client = new DigitalTwinsClient(new Uri(AdtServiceUrl), credentials,
                    new DigitalTwinsClientOptions { Transport = new HttpClientTransport(HttpClient) });
                log.LogInformation($"ADT client connection created with Modules: {client.GetModels().Count()} and TODO");
            }
            catch (Exception e)
            {
                log.LogError($"Error ADT client connection failed: {e}");
                return;
            }
        }
    }
}