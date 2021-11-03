# 1 - Create resources

az group create --name HandOnPythonIoTAzure --location westeurope

az servicebus namespace create --resource-group HandOnPythonIoTAzure --name pythonTest --location westeurope

az servicebus queue create --resource-group HandOnPythonIoTAzure --namespace-name pythonTest --name testQueue

# 2 - Get endpoint

az servicebus namespace authorization-rule keys list --resource-group HandOnPythonIoTAzure --namespace-name pythonTest --name RootManageSharedAccessKey --query primaryConnectionString --output tsv

# 3 - Install library

pip install azure-servicebus

# 4 - Execute example

Change endpoint

# 5 - Review messages in Azure Service Bus

You must go to Service Bus Explorer

https://azure.microsoft.com/es-es/updates/sesrvice-bus-explorer/

# 6 - Delete resources

az group delete --name HandOnPythonIoTAzure --no-wait
