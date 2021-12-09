using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace IoTDPSWithSAS
{
    class Program
    {
        private const string DpsGlobalDeviceEndpoint = "[your dps domain].azure-devices-provisioning.net";
        private const string DpsIdScope = "[your id scope]";

        private const string EnrollmentDevicePrimaryKey = "[your pk1]";
        private const string EnrollmentDeviceSecondaryKey = "[your pk2]";

        private static readonly Random Random = new Random();

        static async Task Main(string[] args)
        {
            var deviceRegistrationId = $"TestDevice1";

            using var securityProvider = new SecurityProviderSymmetricKey(deviceRegistrationId,
                ComputeKeyHash(EnrollmentDevicePrimaryKey, deviceRegistrationId),
                ComputeKeyHash(EnrollmentDeviceSecondaryKey, deviceRegistrationId));

            using var transportHandler = new ProvisioningTransportHandlerAmqp(TransportFallbackType.TcpOnly);

            var provisioningDeviceClient = ProvisioningDeviceClient.Create(
                globalDeviceEndpoint: DpsGlobalDeviceEndpoint, idScope: DpsIdScope, securityProvider: securityProvider,
                transport: transportHandler);

            var deviceRegistrationResult = await provisioningDeviceClient.RegisterAsync();

            if (!string.IsNullOrEmpty(deviceRegistrationResult.AssignedHub))
            {
                Console.WriteLine($"'{deviceRegistrationId}' assigned hub: '{deviceRegistrationResult.AssignedHub}'");
            }
        }

        private static string ComputeKeyHash(string key, string payload)
        {
            using var hmac = new HMACSHA256(Convert.FromBase64String(key));

            return Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload)));
        }
    }
}