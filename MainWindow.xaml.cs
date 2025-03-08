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

namespace GHUB_Overlay
{
    public partial class MainWindow : Window
    {
        string activeDeviceId = null;
        string ToolTipText = null;
        private  Rivatuner _rivatuner;
        private  WebSocket _webSocket;

        public MainWindow()
        {
            InitializeComponent();
            CreateTrayMenu();
            Start();
        }
        public void StartTask()
        {
            _webSocket = new WebSocket();
            _rivatuner = new Rivatuner(); 
        }


        public async Task Start()
        {
            StartTask();
            _webSocket.WebSocketStart();
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
                    Header = "Девайси",
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
                        };
                        menuItem.Click += DeviceMenuItem_Click;
                        devicesMenuItem.Items.Add(menuItem);
                    }
                }

                contextMenu.Items.Add(devicesMenuItem);

                var exitMenuItem = new MenuItem
                {
                    Header = "Вихід"
                };
                exitMenuItem.Click += Exit_Click;

                contextMenu.Items.Add(exitMenuItem);
            };

            MyNotifyIcon.ContextMenu = contextMenu;
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
                    if (activeDeviceId != null && activeDeviceId != deviceId)
                    {
                        //DeviceManager.deviceStates[activeDeviceId] = false;
                        CreateTrayMenu();
                    }

                    activeDeviceId = deviceId;
                }
                else if (activeDeviceId == deviceId)
                {
                    activeDeviceId = null;
                }

                menuItem.IsChecked = isEnabled; 

            }
        }
    }
}
