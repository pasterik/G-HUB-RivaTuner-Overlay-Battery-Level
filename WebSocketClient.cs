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
        private ClientWebSocket client = new ClientWebSocket();
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
        public async Task Start()
        {
            try
            {
                await Connect();
                while (IsRunning)
                {
                    await RequestDevices();
                    await Task.Delay(200);
                    Console.WriteLine($"RequestDevices.....");
                }
            }
            catch (Exception ex)
            {
                await Reconnect();
                await Task.Delay(200);
            }
        }

        private async Task Connect()
        {
            if (client.State == WebSocketState.Open)
                return;

            client.Options.SetRequestHeader("Origin", "file://");
            client.Options.SetRequestHeader("Pragma", "no-cache");
            client.Options.SetRequestHeader("Cache-Control", "no-cache");
            client.Options.SetRequestHeader("Sec-WebSocket-Extensions", "permessage-deflate; client_max_window_bits");
            client.Options.AddSubProtocol("json");

            await client.ConnectAsync(new Uri("ws://localhost:9010"), CancellationToken.None);
            Console.WriteLine("Підключено до сервера.");
            IsRunning = true;
        }

        private async Task Reconnect()
        {

            while (true)
            {
                Console.WriteLine($"Перепідключення...");
                client.Dispose();
                client = new ClientWebSocket();
                await Start();
            }
        }

        private async Task RequestDevices()
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
            if (response == null)
            {
                Console.WriteLine("No Data");
                return;
            }
            else {
                if (response?["deviceInfos"] == null)
                {
                    Console.WriteLine("Дані про пристрої не знайдено.");
                    return;
                }
            }
           
            foreach (var item in response["deviceInfos"])
            {
                string deviceId = item["id"]?.ToString();
                string state = item["state"]?.ToString();
                string displayName = item["displayName"]?.ToString();
                string deviceType = item["deviceType"]?.ToString();
                UpdateOrCreateDevice(deviceId, state, displayName, deviceType);
                await RequestBatteryStatus(deviceId);
            }
        }
        private void UpdateOrCreateDevice(string deviceId, string state, string displayName, string deviceType)
        {
            var existingDevice = DeviceManager.devices.FirstOrDefault(d => d.id == deviceId);
            if (existingDevice != null)
            {
                existingDevice.SetState(state);
                existingDevice.displayName = displayName;
                existingDevice.SetDeviceType(deviceType);
            }
            else
            {
                var newDevice = new Device(deviceId, state, displayName, deviceType);
                DeviceManager.devices.Add(newDevice);
            }
        }

        private async Task RequestBatteryStatus(string deviceId)
        {
            var batteryRequest = new
            {
                msgId = "",
                verb = "Get",
                path = $"/battery/{deviceId}/state"
            };
            var batteryRequestJson = JsonConvert.SerializeObject(batteryRequest);
            await client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(batteryRequestJson)), WebSocketMessageType.Text, true, CancellationToken.None);
            var batteryResponse = await ReceiveMessage(client);
            Console.WriteLine(batteryResponse);
            UpdateBatteryStatus(deviceId, batteryResponse);
        }
        private void UpdateBatteryStatus(string deviceId, JObject? batteryResponse)
        {
            var existingDevice = DeviceManager.devices.FirstOrDefault(d => d.id == deviceId);
            if (batteryResponse != null)
            {
                if (existingDevice != null)
                {
                    bool? charging = (bool?)batteryResponse["charging"] ?? false;
                    int? percentage = batteryResponse["percentage"]?.Value<int?>();
                    if (percentage != null)
                    {
                        existingDevice.charging = charging;
                        existingDevice.percentage = percentage;
                    }
                    else
                    {
                        if (existingDevice.deviceState == Device.State.ACTIVE)
                        {
                            existingDevice.charging = existingDevice.charging;
                            existingDevice.percentage = existingDevice.percentage;
                        }
                        else
                        {
                            existingDevice.charging = false;
                            existingDevice.percentage = null;
                        }
                    }
                }
            }
            else
            {
                if (existingDevice.deviceState == Device.State.ACTIVE)
                {
                    existingDevice.charging = existingDevice.charging;
                    existingDevice.percentage = existingDevice.percentage;
                }
                else
                {
                    existingDevice.charging = false;
                    existingDevice.percentage = null;
                }
            }
        }
        
        public void DisplayDevices()
        {
            foreach (var device in DeviceManager.devices)
            {
                Console.WriteLine($"ID: {device.id}, State: {device.deviceState}, Display Name: {device.displayName}, Device Type: {device.deviceType}, Percentage: {device.percentage}, Charging: {device.charging}");
            }
        }


        public async Task Stop()
        {
            IsRunning = false;
            client.Abort();
            await Task.Delay(500);
        }
    }
}
