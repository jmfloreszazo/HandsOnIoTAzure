import time
from azure.iot.device import IoTHubModuleClient

CONNECTION_STRING = "HostName=jmfzIotHub.azure-devices.net;DeviceId=iotdevicetestclassic;SharedAccessKey=SoMvAResp8zxp91D2HyimLElQ4SSpTXc0CojI9n06N0="


def create_client():
    # Instantiate client
    client = IoTHubModuleClient.create_from_connection_string(
        CONNECTION_STRING)

    def twin_patch_handler(twin_patch):
        print("Twin patch received:")
        print(twin_patch)

    try:
        client.on_twin_desired_properties_patch_received = twin_patch_handler
    except:
        client.shutdown()

    return client


def main():
    print("Starting the Python IoT Hub Device Twin device sample...")
    client = create_client()
    print("IoTHubModuleClient waiting for commands, press Ctrl-C to exit")

    try:
        print("Sending data as reported property...")
        reported_patch = {"sendFrequency": "5"}
        client.patch_twin_reported_properties(reported_patch)
        print("Reported properties updated")

        while True:
            time.sleep(1000000)
    except KeyboardInterrupt:
        print("IoT Hub Device Twin device sample stopped")
    finally:
        # Graceful exit
        print("Shutting down IoT Hub Client")
        client.shutdown()


if __name__ == '__main__':
    main()
