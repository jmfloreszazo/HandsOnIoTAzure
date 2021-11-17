from azure.iot.device import IoTHubDeviceClient, Message
from time import sleep

CONNECTION_STRING = "[Your Connection String]"
RECEIVED_MESSAGES = 0
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
    client.on_message_received = message_handler
    while True:
        message = Message(message_text)
        print("Sending message: {}".format(message))
        client.send_message(message)
        print("Message successfully sent")
        sleep(10)


def message_handler(message):
    global RECEIVED_MESSAGES
    RECEIVED_MESSAGES += 1
    print("")
    print("Message received:")

    for property in vars(message).items():
        print("    {}".format(property))

    print("Total calls received: {}".format(RECEIVED_MESSAGES))


if __name__ == '__main__':
    print("Started simulated device")
    print("Press Ctrl-C to exit")
    run_simulation(connection_client())
