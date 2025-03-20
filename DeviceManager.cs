using GHUB_Overlay.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHUB_Overlay
{
    public class DeviceManager
    {
        public static List<Device> devices = new List<Device>();
        public static Dictionary<string, bool> deviceStates = new Dictionary<string, bool>();
        public static Dictionary<string, bool> deviceMainStates = new Dictionary<string, bool>();

    }

}
