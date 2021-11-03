from azure.servicebus import ServiceBusClient, ServiceBusMessage

CONNECTION_STR = "Endpoint=sb://pythontest.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=w/L2gXxBWCn75ILpDYewKsJNGtgxVcNUptpy6UffHL4="
QUEUE_NAME = "testQueue"


def send_single_message(sender):
    message = ServiceBusMessage("Single Message")
    sender.send_messages(message)
    print("Sent a single message")


def send_a_list_of_messages(sender):
    messages = [ServiceBusMessage("Message in list") for _ in range(5)]
    sender.send_messages(messages)
    print("Sent a list of 5 messages")


def send_batch_message(sender):
    batch_message = sender.create_message_batch()
    for _ in range(10):
        try:
            batch_message.add_message(ServiceBusMessage(
                "Message inside a ServiceBusMessageBatch"))
        except ValueError:
            break
    sender.send_messages(batch_message)
    print("Sent a batch of 10 messages")


servicebus_client = ServiceBusClient.from_connection_string(
    conn_str=CONNECTION_STR, logging_enable=True)

with servicebus_client:
    sender = servicebus_client.get_queue_sender(queue_name=QUEUE_NAME)
    with sender:
        send_single_message(sender)
        send_a_list_of_messages(sender)
        send_batch_message(sender)

print("Done sending messages. Now you can review in Azure Portal.")
print("----------------------------------------------------------")

input("Press Enter to continue...")

with servicebus_client:
    receiver = servicebus_client.get_queue_receiver(
        queue_name=QUEUE_NAME, max_wait_time=5)
    with receiver:
        for msg in receiver:
            print("Received: " + str(msg))
            receiver.complete_message(msg)
