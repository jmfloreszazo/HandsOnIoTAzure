import sys
from time import sleep
from bme280 import BME280
from enviroplus import gas
from subprocess import PIPE, Popen, check_output
from azure.iot.device import IoTHubDeviceClient, Message

CONNECTION_STRING = "[Your Connection String]"

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

def registerClient(id):
    try:
        client = IoTHubDeviceClient.create_from_connection_string(connectionString)
        return client
    except KeyboardInterrupt:
        print("Stopped!")

def connectionClient(connectionString, id):
    try:
        client = IoTHubDeviceClient.create_from_connection_string(connectionString)
        return client
    except KeyboardInterrupt:
        print("Stopped!")

comp_factor = 2.25

id = getSerialNumber()

haveWifi = checkWifi()

print("Raspberry Pi serial: {}".format(getSerialNumber()))
print("Wi-Fi: {}\n".format("connected & send data" if haveWifi else "disconnected & exit program"))

connectionClient = ""

if haveWifi:
    print("registrar device y crearlo si no existe")
else:
    sys.exit()

client = connectionClient(connectionClient)

while True:
    try:
        values = readValues()
        print(values)
        message = Message([{"value_type": key, "value": val} for key, val in values.items()])
        client.send_message(message)
        sleep(5)            
    except Exception as e:
        print(e)