using GHUB_Overlay;
using GHUB_Overlay.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace GHUB_Overlay
{
    public class WebSocket
    {
        public int time;
        public bool IsRunning { get; private set; }
        private static async Task<JObject> ReceiveMessage(ClientWebSocket client)
        {
            using (var memoryStream = new MemoryStream())
            {
                var buffer = new byte[1024];
                WebSocketReceiveResult result;

                do
                {
                    result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    memoryStream.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                var jsonString = Encoding.UTF8.GetString(memoryStream.ToArray());
                GHUBMsg ghubmsg = GHUBMsg.DeserializeJson(jsonString);

                return ghubmsg.Payload;
            }
        }
        public async Task WebSocketStart()
        {
            using (var client = new ClientWebSocket())
            {
                client.Options.SetRequestHeader("Origin", "file://");
                client.Options.SetRequestHeader("Pragma", "no-cache");
                client.Options.SetRequestHeader("Cache-Control", "no-cache");
                client.Options.SetRequestHeader("Sec-WebSocket-Extensions", "permessage-deflate; client_max_window_bits");
                client.Options.AddSubProtocol("json");
                /*
                var newDevice1 = new Device("d01", "ACTIVE", "displayName1", "deviceType");
                var newDevic1 = new Device("d02", "ACTIVE", "displayName2", "deviceType");
                var newDevic2 = new Device("d03", "ACTIVE", "displayName3", "deviceType");
                DeviceManager.devices.Add(newDevice1);
                DeviceManager.devices.Add(newDevic1);
                DeviceManager.devices.Add(newDevic2);
                var existingDevice1 = DeviceManager.devices.FirstOrDefault(d => d.id == "d01");
                var existingDevice2 = DeviceManager.devices.FirstOrDefault(d => d.id == "d02");
                var existingDevice3 = DeviceManager.devices.FirstOrDefault(d => d.id == "d03");
                existingDevice1.percentage = 10;
                existingDevice2.percentage = 49;
                existingDevice3.percentage = 70;
                existingDevice1.charging = false;
                existingDevice2.charging = false;
                existingDevice3.charging = false;
                */

                try
                {
                    await client.ConnectAsync(new Uri("ws://localhost:9010"), CancellationToken.None);
                    Console.WriteLine("Підключено до сервера.");

                    while (true)
                    {
                        var request = new
                        {
                            msgId = "",
                            verb = "Get",
                            path = "/devices/list"
                        };

                        var requestJson = JsonConvert.SerializeObject(request);
                        await client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(requestJson)), WebSocketMessageType.Text, true, CancellationToken.None);
                        var response = await ReceiveMessage(client);
                        //Console.WriteLine(response);
                        if (response != null)
                        {
                            if (response["deviceInfos"] == null)
                            {
                                Console.WriteLine("deviceInfos' not found in the response.");
                            }
                            else
                            {
                                foreach (var item in response["deviceInfos"])
                                {
                                    string deviceId = item["id"]?.ToString();
                                    string state = (string)item["state"]?.ToString();
                                    string displayName = (string)item["displayName"]?.ToString();
                                    string deviceType = (string)item["deviceType"]?.ToString();

                                    var existingDevice = DeviceManager.devices.FirstOrDefault(d => d.id == deviceId);
                                    if (existingDevice != null)
                                    {
                                        existingDevice.SetState(state);
                                        existingDevice.displayName = displayName;
                                        existingDevice.deviceType = deviceType;
                                    }
                                    else
                                    {
                                        var newDevice = new Device(deviceId, state, displayName, deviceType);
                                        DeviceManager.devices.Add(newDevice);
                                    }

                                    var batteryRequest = new
                                    {
                                        msgId = "",
                                        verb = "Get",
                                        path = $"/battery/{deviceId}/state"
                                    };

                                    var batteryRequestJson = JsonConvert.SerializeObject(batteryRequest);
                                    await client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(batteryRequestJson)), WebSocketMessageType.Text, true, CancellationToken.None);
                                    var batteryResponse = await ReceiveMessage(client);
                                    //Console.WriteLine(batteryResponse);
                                    if (batteryResponse != null)
                                    {
                                        if (existingDevice != null)
                                        {
                                            bool? charging = (bool?)batteryResponse["charging"] ?? false;
                                            int? percentage = batteryResponse["percentage"]?.Value<int?>();
                                            if(percentage != null)
                                            {
                                                existingDevice.charging = charging;
                                                existingDevice.percentage = percentage;
                                            }
                                            else
                                            {
                                                existingDevice.charging = existingDevice.charging;
                                                existingDevice.percentage = existingDevice.percentage;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        existingDevice.charging = existingDevice.charging;
                                        existingDevice.percentage = existingDevice.percentage;
                                    }
                                }
                            }
                        }
                        //DisplayDevices();
                        await Task.Delay(1000);
                        IsRunning = true;
                    }
                }
                catch (Exception ex)
                {
                    if (client.State == WebSocketState.Closed || client.State == WebSocketState.None)
                    {
                        await client.ConnectAsync(new Uri("ws://localhost:9010"), CancellationToken.None);
                    }
                    Console.WriteLine($"Помилка підключення: {ex.Message}");
                }
            }
        }
        public void DisplayDevices()
        {
            foreach (var device in DeviceManager.devices)
            {
                Console.WriteLine($"ID: {device.id}, State: {device.state}, Display Name: {device.displayName}, Device Type: {device.deviceType}, Percentage: {device.percentage}, Charging: {device.charging}");
            }
        }

        public async Task Stop()
        {
            IsRunning = false;
            await Task.Delay(1000);
        }
    }
}
