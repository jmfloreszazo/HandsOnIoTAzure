using System;
using System.Net.Http;
using System.Text;
using Azure;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace sendTemeletryIoTCentral2DT
{
    public static class Function1
    {
        // In your local.setting.json
        //{
        //    "IsEncrypted": false,
        //    "Values": {
        //        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        //        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        //        "ServiceBusConnection": "[Your Service Bus Connection String]"
        //    }
        //}

        // In CMD:
        // az login
        // az account set --subscription [you subscription id]

    private static readonly HttpClient HttpClient = new HttpClient();
        private const string AdtServiceUrl = "https://[your digital twin url].digitaltwins.azure.net";

        [FunctionName("Function1")]
        public static void Run(
            [ServiceBusTrigger("jmfztestiotcentral", Connection = "ServiceBusConnection")]
            Message message, ILogger log)
        {
            var sensorId = message.UserProperties["iotcentral-device-id"].ToString();
            var body = Encoding.ASCII.GetString(message.Body, 0, message.Body.Length);
            var bodyProperty = (JObject)JsonConvert.DeserializeObject(body);
            var temperatureToken = bodyProperty["telemetry"]["DeviceTemperature"];
            var temperature = temperatureToken.Value<float>();
            log.LogInformation($"Sensor Id:{sensorId}");
            log.LogInformation($"Sensor Device Temperature:{temperature}");
            var credentials = new DefaultAzureCredential();
            var client = new DigitalTwinsClient(new Uri(AdtServiceUrl), credentials,
                new DigitalTwinsClientOptions { Transport = new HttpClientTransport(HttpClient) });

            UpdateDigitalTwinProperty(client, sensorId, "DeviceTemperature", temperature);
        }

        public static void UpdateDigitalTwinProperty(DigitalTwinsClient client, string twinId, string property,
            object value)
        {
            var patch = new JsonPatchDocument();
            patch.AppendAdd("/" + property, value);
            client.UpdateDigitalTwin(twinId, patch);

        }
    }
}
