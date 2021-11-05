import requests
from plotly.graph_objs import Bar
from plotly import offline

url = "https://pyapirestcalltest-<YOUR ID>.restdb.io/rest/requests"

headers = {
    'content-type': "application/json",
    'x-apikey': "<YOUR API KEY>",
    'cache-control': "no-cache"
}

response = requests.request("GET", url, headers=headers)

print("Show data in text format.")
print("-------------------------")

print(response.text)

responseJSON = response.json()

print("Show total value.")
print("-----------------")

totalValue = 0

for item in responseJSON:
    totalValue += item["value"]

print(totalValue)

print("Create Bar chart.")
print("-----------------")

names, values = [], []

for item in responseJSON:
    names.append(item["name"])
    values.append(item["value"])

data = [{
    "type": "bar",
    "x": names,
    "y": values, }]

myLayout = {
    "title": "Price of companies in $ millions",
    "xaxis": {"title": "Company"},
    "yaxis": {"title": "Value"}, }

fig = {
    "data": data,
    "layout": myLayout}

offline.plot(fig, filename='pySampleReport.html')
