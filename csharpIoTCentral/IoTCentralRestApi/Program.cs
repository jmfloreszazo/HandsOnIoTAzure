using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace IoTCentralRestApi
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Read current temperature telemetry value...");

            string restUriGet =
                "https://[your domain].azureiotcentral.com//api/devices/[your device]/telemetry/[your telemetry]?api-version=1.0";

            var apiKey =
                "SharedAccessSignature [Your SAS value]";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", apiKey);

            var res = await client.GetAsync(restUriGet);

            var jsonString = await res.Content.ReadAsStringAsync();

            Console.WriteLine($"Sent {res.StatusCode} {jsonString}");
        }
    }
}