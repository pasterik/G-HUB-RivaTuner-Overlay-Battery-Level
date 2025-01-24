using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;


namespace GHUB_Overlay
{
    public partial class MainWindow : Window
    {
        private WebSocket webs;
        string activeDeviceId = null;
        public string? ToolTipText { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        private async Task PeriodicPrintDeviceInfo()
        {
            while (true)
            {
                PrintDeviceInfo();
                await Task.Delay(100);
            }
        }

        public async Task Start()
        {
            //IScreen screen = new GHUB_Overlay.ScreenFolder.Screen();
            var webs = new WebSocket();
            webs.WebSocketStart();
            bool start = true;

            await Task.Delay(2000);
            _ = PeriodicPrintDeviceInfo();

            while (start)
            {
                if (webs.IsRunning)
                {

                    SetWebSocket(webs);
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

        public void SetWebSocket(WebSocket websocket)
        {
            webs = websocket;
        }
        private void PrintDeviceInfo()
        {
            string firstTrueDevice =  DeviceManager.deviceStates.Where(c => c.Value).Select(c => c.Key).FirstOrDefault()!;
            var selectDevice = DeviceManager.devices.FirstOrDefault(c => c.id == firstTrueDevice);
            if (selectDevice != null) {
                if (DeviceManager.deviceStates[selectDevice.id] && selectDevice.state == true)
                {
                    string text = "<P2><C=99A8FE>" + selectDevice.displayName + " " + "<C>" + selectDevice.percentage.ToString() + "<S=60>" + " " + "%" + "<S>";
                    Rivatuner.print(text);
                }
                else
                {
                    Rivatuner.print(String.Empty);
                }
            }
            else
            {
                Rivatuner.print(String.Empty);
            }
        }
        private async void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (webs != null && webs.IsRunning)
            {
                await webs.Stop();
            }
            MyNotifyIcon.Dispose();
            Rivatuner.print(String.Empty);
            Application.Current.Shutdown(); 
        }

        private void CreateTrayMenu()
        {
            var contextMenu = new ContextMenu();
            
            var devicesMenuItem = new MenuItem
            {
                Header = "Девайси",

            };

            foreach (var item in DeviceManager.devices)
            {
                var menuItem = new MenuItem
                {
                    Header = item.displayName,
                    IsChecked = DeviceManager.deviceStates.ContainsKey(item.id) && DeviceManager.deviceStates[item.id],
                    Tag = item.id
                };

                menuItem.Click += DeviceMenuItem_Click;
                devicesMenuItem.Items.Add(menuItem);
            }

            contextMenu.Items.Add(devicesMenuItem);

            var exitMenuItem = new MenuItem
            {
                Header = "Вихід"
            };
            exitMenuItem.Click += Exit_Click;

            contextMenu.Items.Add(exitMenuItem);

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
                        DeviceManager.deviceStates[activeDeviceId] = false;
                        UpdateTrayMenu();
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



        private void UpdateTrayMenu()
        {
            CreateTrayMenu();
        }
    }
}
