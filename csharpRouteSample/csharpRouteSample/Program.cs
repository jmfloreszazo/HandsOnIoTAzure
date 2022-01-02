using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using csharpSASSecurity;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Common.Exceptions;
using Newtonsoft.Json;
using Message = Microsoft.Azure.Devices.Client.Message;
using TransportType = Microsoft.Azure.Devices.Client.TransportType;

namespace csharpRouteSample
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

            var simulatedDevices = new List<SimulatedDevice>();

            for (var i = 1; i <= 10; i++)
            {
                var simulatedDevice = new SimulatedDevice { Id = deviceId + i };

                Device device;

                try
                {
                    device = await _registryManager.AddDeviceAsync(new Device(simulatedDevice.Id));
                }
                catch (DeviceAlreadyExistsException)
                {
                    device = await _registryManager.GetDeviceAsync(simulatedDevice.Id);
                }

                Console.WriteLine("Created device {0} at {1}.", simulatedDevice.Id, DateTime.Now);

                simulatedDevice.SharedAccessKey = device.Authentication.SymmetricKey.PrimaryKey;

                simulatedDevices.Add(simulatedDevice);
            }

            var rnd = new Random();

            while (true)
            {
                var id = rnd.Next(1, 10);

                var item = simulatedDevices.Find(x => Equals(x.Id, deviceId + id));

                if (item != null)
                {
                    var deviceClient = DeviceClient.Create(IotHubHostname,
                        new DeviceAuthenticationWithRegistrySymmetricKey(item.Id,
                            item.SharedAccessKey),
                        TransportType.Http1);

                    var deviceTelemetryData = new
                        { messageId = Guid.NewGuid().ToString(), deviceId = item.Id, value = id };

                    var messageString = JsonConvert.SerializeObject(deviceTelemetryData);
                    var message = new Message(Encoding.ASCII.GetBytes(messageString));
                    message.ContentType = "application/json";
                    message.ContentEncoding = "utf-8";
                    message.Properties.Add("testPropertyDeviceId", id.ToString());
                    await deviceClient.SendEventAsync(message);

                    Console.WriteLine("Message from {0} at {1}: {2}", item.Id, DateTime.Now, messageString);

                    Thread.Sleep(1000);
                }
            }
        }
    }
}