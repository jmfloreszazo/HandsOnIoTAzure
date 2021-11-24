using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace IoTCentralDevice
{
    internal class Program
    {
        private const string ConnectionString =
            "[your device connection string]";

        private const double BaseTemperature = 100.0;

        private static readonly Random Random = new Random();

        private static async Task Main(string[] args)
        {
            await Task.Delay(1000);

            var deviceClient = DeviceClient.CreateFromConnectionString(ConnectionString);

            if (deviceClient == null) return;

            using var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var sendDeviceDataTask = SendDeviceDataUntilCancelled(deviceClient, cancellationToken);

            Console.ReadLine();
            Console.WriteLine("Shutting down...");

            cancellationTokenSource.Cancel();
            sendDeviceDataTask.Wait();
        }

        private static async Task SendDeviceDataUntilCancelled(DeviceClient deviceClient,
            CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var payload = new
                    {
                        BearingTemp = BaseTemperature - Random.Next(-1, 1) * 100,
                    };

                    var bodyJson = JsonConvert.SerializeObject(payload, Formatting.Indented);
                    var message = new Message(Encoding.UTF8.GetBytes(bodyJson))
                    {
                        ContentType = "application/json", ContentEncoding = "utf-8"
                    };

                    await deviceClient.SendEventAsync(message, cancellationToken);

                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR {ex.Message}");
            }
        }
    }
}