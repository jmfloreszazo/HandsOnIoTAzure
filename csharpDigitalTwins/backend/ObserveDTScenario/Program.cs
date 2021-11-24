using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace ObserveDTScenario
{
    internal class Program
    {
        private const string Device = "gearbox11";
        private const string PropName = "BearingTemp";

        private static void Main(string[] args)
        {
            Console.WriteLine("Starting observation...");

            Uri adtInstanceUrl;
            try
            {
                IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false)
                    .Build();
                adtInstanceUrl = new Uri(config["instanceUrl"]);
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is UriFormatException)
            {
                Console.WriteLine("Have you configured your ADT instance URL in appsettings.json?");
                return;
            }

            var credential = new DefaultAzureCredential();
            var client = new DigitalTwinsClient(adtInstanceUrl, credential);

            try
            {
                var res = client.GetDigitalTwin<object>(Device);
                if (res != null)
                    LogProperty(JsonSerializer.Serialize(res.Value), PropName);
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine($"Error {e.Status}: {e.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
        }

        public static void LogProperty(string res, string propName)
        {

            var obj = JsonSerializer.Deserialize<Dictionary<string, object>>(res);

            if (obj != null && !obj.TryGetValue(propName, out object value))
                value = "<property not found>";

            Console.WriteLine($"{propName}: {value}");
        }
    }
}
