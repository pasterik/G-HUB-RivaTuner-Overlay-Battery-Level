using GHUB_Overlay.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GHUB_Overlay.RivatunerFolder.Rivatuner
{
    public class Rivatuner
    {
        [DllImport("kernel32")]
        private unsafe static extern void* LoadLibrary(string dllname);
        [DllImport("kernel32")]
        private unsafe static extern void FreeLibrary(void* handle);
        private sealed unsafe class LibraryUnloader
        {
            internal LibraryUnloader(void* handle)
            {
                this.handle = handle;
            }

            ~LibraryUnloader()
            {
                if (handle != null)
                    FreeLibrary(handle);
            }

            private void* handle;
        }
        private static readonly LibraryUnloader unloader;


        static Rivatuner()
        {
            try
            {
                if (!IsRivaRunning())
                {
                    RunRiva();
                }

                string path = nint.Size == 4 ? @"x86/rivatuner.dll" : @"x64/rivatuner.dll";
                Console.WriteLine($"Attempting to load DLL from: {Path.GetFullPath(path)}");

                unsafe
                {
                    void* handle = LoadLibrary(path);

                    if (handle == null)
                    {
                        Console.WriteLine($"DLL not found at: {Path.GetFullPath(path)}");
                        throw new DllNotFoundException($"Unable to find the native RivaTuner library at path: {Path.GetFullPath(path)}");
                    }

                    unloader = new LibraryUnloader(handle);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RivaTuner static constructor: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        public static bool IsRivaRunning()
        {
            Process[] pname = Process.GetProcessesByName("RTSS");
            return pname.Length > 0;
        }

        public static void RunRiva()
        {
            FileInfo f = new FileInfo(@"C:\Program Files (x86)\RivaTuner Statistics Server\RTSS.exe");
            if (f.Exists)
            {
                try
                {
                    Process.Start(f.FullName);
                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error starting RivaTuner: " + ex.Message);
                }
            }
        }
        [DllImport("rivatuner", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool print(string text);
        private void PrintDeviceInfo()
        {
            var trueDevice = DeviceManager.deviceStates.Where(c => c.Value).Select(c => c.Key).ToList();
            var selectDevice = DeviceManager.devices.Where(c => trueDevice.Contains(c.id)).ToList();
            string text = string.Empty;
            if (selectDevice != null)
            {
                foreach (var item in selectDevice)
                {
                    if (DeviceManager.deviceStates[item.id] && item.deviceState == Device.State.ACTIVE)
                    {
                        text += "<P2><C=99A8FE>" + item.displayName + " " + "<C>" + item.percentage.ToString() + "<S=60>" + " " + "%" + "<S>" + "\n";
                    }
                    else
                    {
                        text += "<P2><C=FF0000>" + item.displayName + " " + "<C>" + "\n";
                    }
                    print(text);
                }
            }
            else
            {
                print(string.Empty);
            }
        }
        public async Task PeriodicPrintDeviceInfo()
        {
            while (true)
            {
                PrintDeviceInfo();
                await Task.Delay(100);
            }
        }
    }
}
