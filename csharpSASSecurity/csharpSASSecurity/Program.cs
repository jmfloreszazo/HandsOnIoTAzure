using System;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Common.Exceptions;
using Newtonsoft.Json;
using Message = Microsoft.Azure.Devices.Client.Message;
using TransportType = Microsoft.Azure.Devices.Client.TransportType;

namespace csharpSASSecurity
{

    internal class Program
    {
        private static readonly string deviceId = "TestDevice";
        private static RegistryManager _registryManager;
        private static readonly string ConnectionString = ConfigurationManager.AppSettings["connectionString"];
        private static readonly string IotHubHostname = ConfigurationManager.AppSettings["iotHubHostname"];

        private static async Task Main(string[] args)
        {
            Console.WriteLine("Init at {0}.", DateTime.Now);

            _registryManager = RegistryManager.CreateFromConnectionString(ConnectionString);

            var simulatedDevice = new SimulatedDevice { Id = deviceId };

            Device device;

            try
            {
                device = await _registryManager.AddDeviceAsync(new Device(simulatedDevice.Id));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await _registryManager.GetDeviceAsync(simulatedDevice.Id);
            }

            Console.WriteLine("Created device {0} at {1}.", deviceId, DateTime.Now);

            simulatedDevice.SharedAccessKey = device.Authentication.SymmetricKey.PrimaryKey;

            var deviceClient = DeviceClient.Create(IotHubHostname,
                new DeviceAuthenticationWithRegistrySymmetricKey(simulatedDevice.Id, simulatedDevice.SharedAccessKey),
                TransportType.Mqtt);

            while (true)
            {
                var deviceTelemetryData = new { messageId = Guid.NewGuid().ToString(), deviceId = simulatedDevice.Id };

                var messageString = JsonConvert.SerializeObject(deviceTelemetryData);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                await deviceClient.SendEventAsync(message);

                Console.WriteLine("Message from {0} at {1}: {2}", simulatedDevice.Id, DateTime.Now, messageString);

                Thread.Sleep(5000);
            }
        }
    }
}