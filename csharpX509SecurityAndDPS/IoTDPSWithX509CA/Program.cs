using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;

namespace IoTDPSWithX509CA
{
    internal class Program
    {
        private const string DpsGlobalDeviceEndpoint = "[your dps domain].azure-devices-provisioning.net";
        private const string DpsIdScope = "[your id scope]";

        private const string DeviceCertificatePassword = @"12345";
        private const string Device1CertificateFileName = @"C:\temp\certs\DemoDevice1.pfx";

        private static readonly ConsoleColor DefaultConsoleForegroundColor = Console.ForegroundColor;

        private static async Task Main(string[] args)
        {
            var device1RegistrationId = "DemoDevice1";
            var device1Certificate = LoadProvisioningCertificate(Device1CertificateFileName, DeviceCertificatePassword);

            await RegisterDevice(device1RegistrationId, device1Certificate);

            await SendDeviceData(device1RegistrationId, device1Certificate);

        }

        private static async Task RegisterDevice(string deviceRegistrationId, X509Certificate2 deviceCertificate)
        {
            try
            {
                using var securityProvider = new SecurityProviderX509Certificate(deviceCertificate);
                using var transportHandler = new ProvisioningTransportHandlerAmqp(TransportFallbackType.TcpOnly);

                var provisioningDeviceClient = ProvisioningDeviceClient.Create(
                    DpsGlobalDeviceEndpoint, DpsIdScope,
                    securityProvider, transportHandler);

                var deviceRegistrationResult = await provisioningDeviceClient.RegisterAsync();

                ConsoleWriteLine(
                    $"[{deviceRegistrationId}] Device registration result: {deviceRegistrationResult.Status}");

                if (string.IsNullOrEmpty(deviceRegistrationResult.AssignedHub))
                {
                    //TODO
                }
                else
                {
                    var deviceConnectionInfoFileName = GetDeviceConnectionInfoFileName(deviceRegistrationId);
                    var deviceConnectionInfoJson = JsonConvert.SerializeObject(new DeviceConnectionInfo
                    {
                        AssignedHub = deviceRegistrationResult.AssignedHub,
                        DeviceId = deviceRegistrationResult.DeviceId
                    });
                    await File.WriteAllTextAsync(deviceConnectionInfoFileName, deviceConnectionInfoJson);
                }
            }
            catch (Exception ex)
            {
                //TODO
            }
        }

        private static async Task SendDeviceData(string deviceRegistrationId, X509Certificate2 deviceCertificate)
        {
            try
            {
                var deviceConnectionInfoFileName = GetDeviceConnectionInfoFileName(deviceRegistrationId);
                if (!File.Exists(deviceConnectionInfoFileName)) return;

                var deviceConnectionInfoJson = await File.ReadAllTextAsync(deviceConnectionInfoFileName);
                var deviceConnectionInfo =
                    JsonConvert.DeserializeObject<DeviceConnectionInfo>(deviceConnectionInfoJson);

                var deviceAuthentication = new DeviceAuthenticationWithX509Certificate(
                    deviceConnectionInfo.DeviceId, deviceCertificate);

                using var deviceClient = DeviceClient.Create(deviceConnectionInfo.AssignedHub,
                    deviceAuthentication, TransportType.Amqp_Tcp_Only);

                var payload = new { deviceRegistrationId, message = $"Hello World! - {DateTime.UtcNow}" };
                var bodyJson = JsonConvert.SerializeObject(payload);
                var message = new Message(Encoding.UTF8.GetBytes(bodyJson))
                {
                    ContentType = "application/json", ContentEncoding = "utf-8"
                };

                await deviceClient.SendEventAsync(message);
            }
            catch (Exception ex)
            {
                //TODO
            }
        }

        private static string GetDeviceConnectionInfoFileName(string deviceRegistrationId)
        {
            return Path.Combine(Environment.CurrentDirectory, deviceRegistrationId + ".json");
        }

        private static X509Certificate2 LoadProvisioningCertificate(string certificateFileName,
            string certificatePassword)
        {
            var certificateCollection = new X509Certificate2Collection();

            certificateCollection.Import(certificateFileName, certificatePassword, X509KeyStorageFlags.UserKeySet);

            X509Certificate2 certificate = null;

            foreach (var element in certificateCollection)
                if (element != null && certificate == null && element.HasPrivateKey)
                    certificate = element;
                else
                    element?.Dispose();

            return certificate;
        }

        private static void ConsoleWriteLine(string message = null, ConsoleColor? foregroundColor = null)
        {
            Console.ForegroundColor = foregroundColor ?? DefaultConsoleForegroundColor;
            Console.WriteLine(message);
        }

        private class DeviceConnectionInfo
        {
            public string AssignedHub { get; set; }
            public string DeviceId { get; set; }
        }
    }
}