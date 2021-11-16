from azure.iot.device import IoTHubDeviceClient, Message
from time import sleep

CONNECTION_STRING = "[Your Connection String]"
message_text = "{'Hello World!'}"


def connection_client():
    try:
        client = IoTHubDeviceClient.create_from_connection_string(
            CONNECTION_STRING)
        return client
    except KeyboardInterrupt:
        print("Stopped!")


def run_simulation(client):
    client = client
    while True:
        message = Message(message_text)
        print("Sending message: {}".format(message))
        client.send_message(message)
        print("Message successfully sent")
        sleep(10)


if __name__ == '__main__':
    print("Started simulated device")
    print("Press Ctrl-C to exit")
    run_simulation(connection_client())
