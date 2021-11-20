using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IoTCentralDevice
{
    internal class Program
    {
        private const string ProvisioningGlobalDeviceEndpoint = "global.azure-devices-provisioning.net";
        private const string ProvisioningIdScope = "[your value]";
        private const string DeviceId = "[your value]";
        private const string DevicePrimaryKey = "[your value]";
        private const double BaseTemperature = 20.0;

        private const string TestCommandDirectMethodName = "TestCommand";

        private static readonly Random Random = new Random();

        //TODO: You must do the same for rest of values
        private static readonly double _currentTemperature = BaseTemperature;
        private static double _targetTemperature = _currentTemperature;

        private static async Task Main(string[] args)
        {
            await Task.Delay(1000);

            var deviceRegistrationResult = await RegisterDevice();
            if (deviceRegistrationResult == null) return;

            using var deviceClient = NewDeviceClient(deviceRegistrationResult.AssignedHub);
            if (deviceClient == null) return;

            await ReadDesiredPropertiesFromTwin(deviceClient);
            await StartListeningForDirectMethod(deviceClient);
            await StartListeningForDesiredPropertyChanges(deviceClient);
            await SendReportedProperties(deviceClient);

            using var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var sendDeviceDataTask = SendDeviceDataUntilCancelled(deviceClient, cancellationToken);

            Console.ReadLine();
            Console.WriteLine("Shutting down...");

            cancellationTokenSource.Cancel();
            sendDeviceDataTask.Wait();
        }

        private static async Task<DeviceRegistrationResult> RegisterDevice()
        {
            try
            {
                Console.WriteLine($"Will register device {DeviceId}...");

                using var securityProvider = new SecurityProviderSymmetricKey(DeviceId,
                    DevicePrimaryKey, null);

                using var transportHandler = new ProvisioningTransportHandlerMqtt(TransportFallbackType.TcpOnly);

                var provisioningDeviceClient = ProvisioningDeviceClient.Create(
                    ProvisioningGlobalDeviceEndpoint, ProvisioningIdScope,
                    securityProvider, transportHandler);

                var deviceRegistrationResult = await provisioningDeviceClient.RegisterAsync();

                Console.WriteLine($"Device {DeviceId} registration result: {deviceRegistrationResult.Status}");

                if (deviceRegistrationResult.Status != ProvisioningRegistrationStatusType.Assigned)
                    throw new Exception($"Failed to register device {DeviceId}");

                Console.WriteLine($"Device {DeviceId} was assigned to hub '{deviceRegistrationResult.AssignedHub}'");
                Console.WriteLine();

                return deviceRegistrationResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"* ERROR * {ex.Message}");
            }

            return null;
        }

        private static DeviceClient NewDeviceClient(string assignedHub)
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine($"Will create client for device {DeviceId}...");

                var authenticationMethod =
                    new DeviceAuthenticationWithRegistrySymmetricKey(DeviceId, DevicePrimaryKey);

                var deviceClient = DeviceClient.Create(assignedHub,
                    authenticationMethod, TransportType.Mqtt_Tcp_Only);

                Console.WriteLine($"Successfully created client for device {DeviceId}");

                return deviceClient;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"* ERROR * {ex.Message}");
            }

            return null;
        }

        private static async Task ReadDesiredPropertiesFromTwin(DeviceClient deviceClient)
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine($"Will get twin for device {DeviceId}...");

                var twin = await deviceClient.GetTwinAsync();

                Console.WriteLine($"Successfully got twin for for device {DeviceId}:");
                Console.WriteLine(twin.ToJson(Formatting.Indented));

                ApplyDesiredProperties(twin.Properties.Desired);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"* ERROR * {ex.Message}");
            }
        }

        private static void ApplyDesiredProperties(TwinCollection desiredProperties)
        {
            try
            {
                if (desiredProperties == null) return;

                if (desiredProperties.Contains("TargetTemperature") && desiredProperties["TargetTemperature"] != null)
                    _targetTemperature = desiredProperties["TargetTemperature"];
            }
            catch (Exception ex)
            {
                Console.WriteLine($" ERROR {ex.Message}");
            }
        }

        private static async Task StartListeningForDirectMethod(DeviceClient deviceClient)
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine($"Will setup listener for direct method {TestCommandDirectMethodName}...");

                await deviceClient.SetMethodHandlerAsync(TestCommandDirectMethodName,
                    TestCommandDirectMethodCallback, deviceClient);

                Console.WriteLine($"Now listening for direct method {TestCommandDirectMethodName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" ERROR {ex.Message}");
            }
        }

        private static async Task<MethodResponse> TestCommandDirectMethodCallback(MethodRequest methodRequest,
            object userContext)
        {
            try
            {
                var deviceClient = (DeviceClient)userContext;

                Console.WriteLine();
                Console.WriteLine($"Direct method {methodRequest.Name} was invoked");

                var testingCommand = methodRequest.Data == null ? null : Encoding.UTF8.GetString(methodRequest.Data);

                if (string.IsNullOrEmpty(testingCommand))
                    throw new Exception($"Missing payload for direct method {methodRequest.Name}");

                Console.WriteLine($"Tesing command now (mode: {testingCommand})...");

                await Task.Delay(Random.Next(1000, 3001));

                Console.WriteLine("Done testing command");

                return new MethodResponse(200);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR {ex.Message}");
            }

            return new MethodResponse(500);
        }

        private static async Task StartListeningForDesiredPropertyChanges(DeviceClient deviceClient)
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("Will setup listener for desired property updates...");

                await deviceClient.SetDesiredPropertyUpdateCallbackAsync(DesiredPropertyUpdateCallback,
                    deviceClient);

                Console.WriteLine("Now listening for desired property updates");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR {ex.Message}");
            }
        }

        private static async Task DesiredPropertyUpdateCallback(TwinCollection desiredProperties, object userContext)
        {
            try
            {
                var deviceClient = (DeviceClient)userContext;

                Console.WriteLine();
                Console.WriteLine("Received desired property update:");
                Console.WriteLine(desiredProperties.ToJson(Formatting.Indented));

                ApplyDesiredProperties(desiredProperties);

                await SendReportedProperties(deviceClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"* ERROR * {ex.Message}");
            }
        }

        private static async Task SendReportedProperties(DeviceClient deviceClient)
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine($"Will send reported properties for device {DeviceId}...");

                var reportedProperties = new TwinCollection
                {
                    ["Location"] = "Location", //Your GPS
                    ["TargetTemperature"] = _targetTemperature
                };

                await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);

                Console.WriteLine($"Successfully sent reported properties for {DeviceId}:");
                Console.WriteLine(reportedProperties.ToJson(Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"* ERROR * {ex.Message}");
            }
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
                        CurrentTemperature =
                            BaseTemperature - (Random.Next(-1, 1) * 10)
                    };

                    var bodyJson = JsonConvert.SerializeObject(payload, Formatting.Indented);
                    var message = new Message(Encoding.UTF8.GetBytes(bodyJson))
                    {
                        ContentType = "application/json", ContentEncoding = "utf-8"
                    };

                    await deviceClient.SendEventAsync(message, cancellationToken);

                    Console.WriteLine($"Successfully sent message for device {DeviceId}");

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