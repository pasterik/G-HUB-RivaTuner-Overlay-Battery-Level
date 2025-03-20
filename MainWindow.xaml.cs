using GHUB_Overlay.Model;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GHUB_Overlay.RivatunerFolder.Rivatuner;
using System.Net.WebSockets;
using System.Windows.Shapes;

namespace GHUB_Overlay
{
    public partial class MainWindow : Window
    {
        string activeDeviceId = null;
        string ToolTipText = null;
        private  Rivatuner _rivatuner;
        private WebSocket _webSocket;

        public MainWindow()
        {
            InitializeComponent();
            UpdateTrayIconFromFile();
            CreateTrayMenu();
            StartMonitoring();
            Start();
            _webSocket.Start();
        }
        private async Task StartMonitoring()
        {
            while (true)
            {
                UpdateTrayIconFromFile();
                await Task.Delay(1000); 
            }
        }
        private void UpdateTrayIconFromFile()
        {
            var trueDevice = DeviceManager.deviceStates.Where(c => c.Value).Select(c => c.Key).ToList();
            var selectDevice = DeviceManager.devices.Where(c => trueDevice.Contains(c.id)).ToList();
            if (selectDevice.Count() > 0)
            {
                string text = null;
                var deviceInfo = new List<string>();
                foreach (var item in selectDevice)
                {
                    string typeico = GetTypeToolTipText(item);
                    if (item.deviceState == Device.State.ACTIVE)
                    {
                        deviceInfo.Add(typeico + item.displayName + " " + item.percentage + "%");
                    }
                    else
                    {
                        deviceInfo.Add(typeico + item.displayName + " " + item.deviceState);
                    }

                }
                text = string.Join("\n", deviceInfo);
                MyNotifyIcon.ToolTipText = text;
            }
            else 
            {
                MyNotifyIcon.ToolTipText = "GHUB Overlay";
            }

            if (selectDevice.Count() == 1)
            {
                string path = GetImgIcon(selectDevice.FirstOrDefault());
                MyNotifyIcon.Icon = new System.Drawing.Icon(path);
            }
            else
            {
                string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "battery_charging_dark.ico");
                if (File.Exists(iconPath))
                {
                    MyNotifyIcon.Icon = new System.Drawing.Icon(iconPath);
                }
            }
        }
        private string GetImgIcon(Device device)
        {
            string iconPath = null;
            if (device.percentage >= 81 && device.percentage <= 100)
            {
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "battery_100_dark.ico");
            }
            else if (device.percentage >= 61 && device.percentage <= 80)
            {
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "battery_75_dark.ico");
            }
            else if (device.percentage >= 41 && device.percentage <= 60)
            {
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "battery_50_dark.ico");
            }
            else if (device.percentage >= 21 && device.percentage <= 40)
            {
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "battery_25_dark.ico");
            }
            else
            {
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "battery_0_dark.ico");
            }

        }
        private string GetTypeToolTipText(Device device)
        {
            string text = string.Empty;
            if (device.charging == true)
            {
                text = "⚡";
            }
            else if(device.deviceType == Device.Type.HEADSET)
            {
                text = "🎧";
            }
            else if (device.deviceType == Device.Type.KEYBOARD)
            {
                text = "⌨️";
            }
            else if (device.deviceType == Device.Type.MOUSE)
            {
                text = "🖱️";
            }
            else
            {
                text = "⚠️";
            }
            return text;
        }
        public void StartTask()
        {
            _webSocket = new WebSocket();
            _rivatuner = new Rivatuner(); 
        }


        public async Task Start()
        {
            StartTask();
            bool start = true;

            await Task.Delay(2000);
            await _rivatuner.PeriodicPrintDeviceInfo();
            while (start)
            {
                if (_webSocket.IsRunning)
                {
                    
                    CreateTrayMenu();
                    if (!Rivatuner.IsRivaRunning())
                    {
                        Console.WriteLine("RivaTuner is not running. Starting it...");
                        Rivatuner.RunRiva();
                    }
                    else
                    {
                        Console.WriteLine("RivaTuner is already running.");
                    }
                    start = false;
                }
                else
                {
                    Console.WriteLine("Loding.....");
                    await Task.Delay(2000);
                }
            }
        }

     

        private async void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (_webSocket != null && _webSocket.IsRunning)
            {
                await _webSocket.Stop();
            }
            MyNotifyIcon.Dispose();
            Rivatuner.print(String.Empty);
            Application.Current.Shutdown(); 
        }
        

        private void CreateTrayMenu()
        {
            var contextMenu = new ContextMenu();

            contextMenu.Opened += (sender, args) =>
            {
                contextMenu.Items.Clear();

                var devicesMenuItem = new MenuItem
                {
                    Header = "Пристрої",
                    
                };

                foreach (var item in DeviceManager.devices)
                {
                    if (item.deviceState != Device.State.ABSENT)
                    {
                        var menuItem = new MenuItem
                        {
                            Header = item.displayName,
                            IsChecked = DeviceManager.deviceStates.ContainsKey(item.id) && DeviceManager.deviceStates[item.id],
                            Tag = item.id,
                            StaysOpenOnClick = true,
                        };
                        menuItem.Click += DeviceMenuItem_Click;
                        devicesMenuItem.Items.Add(menuItem);

                    }
                }

                contextMenu.Items.Add(devicesMenuItem);

                var SettingMenuItem = new MenuItem
                {
                    Header = "Налаштування"
                };
                SettingMenuItem.Click += SettingMenuItem_Click;

                contextMenu.Items.Add(SettingMenuItem);

                var exitMenuItem = new MenuItem
                {
                    Header = "Вихід"
                };
                exitMenuItem.Click += Exit_Click;

                contextMenu.Items.Add(exitMenuItem);
            };

            MyNotifyIcon.ContextMenu = contextMenu;
        }
        private Setting settingWindow; 

        private void SettingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (settingWindow == null || !settingWindow.IsVisible)
            {
                settingWindow = new Setting();
                settingWindow.Closed += (s, args) => settingWindow = null; 
                settingWindow.Show();

            }
            else
            {
                settingWindow.Activate(); 
            }
        }


        private void DeviceMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                var deviceId = (string)menuItem.Tag;

                
                bool isEnabled = !menuItem.IsChecked; 
                DeviceManager.deviceStates[deviceId] = isEnabled; 

                if (isEnabled)
                {
                    /*
                    if (activeDeviceId != null && activeDeviceId != deviceId)
                    {
                        //DeviceManager.deviceStates[activeDeviceId] = false;
                        CreateTrayMenu();
                    }*/

                    activeDeviceId = deviceId;
                }
                /*else if (activeDeviceId == deviceId)
                {
                    activeDeviceId = null;
                }*/
                else
                {
                    activeDeviceId = null;
                }
                menuItem.IsChecked = isEnabled; 

            }
        }
    }
}
