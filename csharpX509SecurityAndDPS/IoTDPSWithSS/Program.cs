using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

namespace IoTDPSWithSC
{
    class Program
    {
        private const string IoTHubEndpoint = "[your iot domain].azure-devices.net";
        private static DeviceClient _deviceClient;

        static async Task Main(string[] args)
        {
            var primaryPfx =
                new X509Certificate2(
                    @"C:\temp\certificate.cer",
                    "12345");
            var primaryAuth = new DeviceAuthenticationWithX509Certificate("testdevice2", primaryPfx);

            _deviceClient = DeviceClient.Create(IoTHubEndpoint, primaryAuth, TransportType.Amqp_Tcp_Only);

            string messageBody = JsonSerializer.Serialize(
                new { Salutation = "Hello Wordl!, DateTime = ", DateTime.Now });
            using var message = new Message(Encoding.ASCII.GetBytes(messageBody))
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
            };

            await _deviceClient.SendEventAsync(message);

            _deviceClient.Dispose();
        }
    }
}