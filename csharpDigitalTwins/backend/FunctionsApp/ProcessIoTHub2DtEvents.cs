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
using System;
using System.Net.Http;

namespace FunctionsApp
{
    public class ProcessIoTHub2DtEvents
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private static readonly string AdtServiceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");

        [FunctionName("ProcessIoTHub2DTEvents")]
        public async void Run([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
        {
            var credentials = new DefaultAzureCredential();
            DigitalTwinsClient client = new DigitalTwinsClient(new Uri(AdtServiceUrl), credentials,
                new DigitalTwinsClientOptions {Transport = new HttpClientTransport(HttpClient)});
            log.LogInformation($"ADT client connection created");

            if (eventGridEvent != null && eventGridEvent.Data != null)
            {
                log.LogInformation(eventGridEvent.Data.ToString());

                JObject message = (JObject) JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());
                string deviceId = (string) message["systemProperties"]["iothub-connection-device-id"];
                var bearingTemp = message["body"]["BearingTemp"];

                log.LogInformation($"Device:{deviceId} BearingTemp is: {bearingTemp}");

                var updateTwinData = new JsonPatchDocument();
                updateTwinData.AppendReplace("/BearingTemp", bearingTemp.Value<double>());
                await client.UpdateDigitalTwinAsync(deviceId, updateTwinData);
            }
        }
    }
}