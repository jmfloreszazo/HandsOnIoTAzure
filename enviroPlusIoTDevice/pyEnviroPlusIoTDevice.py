import sys
from time import sleep
from bme280 import BME280
from enviroplus import gas
from subprocess import PIPE, Popen, check_output
from azure.iot.hub import IoTHubRegistryManager
from azure.iot.device import IoTHubDeviceClient, Message

HOST_NAME = ".azure-devices.net"
CONNECTION_STRING = "HostName=" + HOST_NAME + ";SharedAccessKeyName=iothubowner;SharedAccessKey=[your SharedAccessKey]"

print("Press Ctrl+C to exit!")

bme280 = BME280()

def readValues():
    values = {}
    cpu_temp = getCpuTemperature()
    raw_temp = bme280.get_temperature()
    comp_temp = raw_temp - ((cpu_temp - raw_temp) / comp_factor)
    values["temperature"] = "{:.2f}".format(comp_temp)
    values["pressure"] = "{:.2f}".format(bme280.get_pressure() * 100)
    values["humidity"] = "{:.2f}".format(bme280.get_humidity())
    data = gas.read_all()
    values["oxidised"] = "{:.2f}".format(data.oxidising / 1000)
    values["reduced"] = "{:.2f}".format(data.reducing / 1000)
    values["nh3"] = "{:.2f}".format(data.nh3 / 1000)
    return values

def getCpuTemperature():
    process = Popen(['vcgencmd', 'measure_temp'], stdout=PIPE, universal_newlines=True)
    output, _error = process.communicate()
    return float(output[output.index('=') + 1:output.rindex("'")])

def getSerialNumber():
    with open('/proc/cpuinfo', 'r') as f:
        for line in f:
            if line[0:6] == 'Serial':
                return line.split(":")[1].strip()

def checkWifi():
    if check_output(['hostname', '-I']):
        return True
    else:
        return False

def sendToIotHub(values, client):
    message = Message([{"value_type": key, "value": val} for key, val in values.items()])
    client.send_message(message)
    return True

def getClient(id):
    try:
        iothubRegistryManager = IoTHubRegistryManager.from_connection_string(CONNECTION_STRING)
        if (iothubRegistryManager.get_device(id)):
            return iothubRegistryManager.get_device(id)
    except:
        primaryKey = "s5r3pbY8RFwWkxKGZpjc7No99HaOtUUm1TqZHFXCBs4="
        secondaryKey = "Wh6y1/a2PDj4uAvbezN55XV6TvEdSuJxcq65TrzcsG0="
        deviceState = "enabled"
        iotEdge = False
        newDevice = iothubRegistryManager.create_device_with_sas(
            id, primaryKey, secondaryKey, deviceState, iotEdge
        )
        return newDevice

def connectionClient(device):
    try:
        client = IoTHubDeviceClient.create_from_symmetric_key(
            device.authentication.symmetric_key.primary_key, HOST_NAME, device.device_id
            )
        return client
    except KeyboardInterrupt:
        print("Stopped!")

comp_factor = 2.25

id = getSerialNumber()

haveWifi = checkWifi()

print("Raspberry Pi serial: {}".format(getSerialNumber()))
print("Wi-Fi: {}\n".format("connected & send data" if haveWifi else "disconnected & exit program"))

if haveWifi:
    device = getClient(id)
else:    
    sys.exit()

if device:
    client = connectionClient(device)
else:
    print("Error with connectionClient!")
    sys.exit()

while True:
    try:
        values = readValues()
        print(values)
        message = Message(str(values))
        client.send_message(message)
        sleep(5)            
    except Exception as e:
        print(e)