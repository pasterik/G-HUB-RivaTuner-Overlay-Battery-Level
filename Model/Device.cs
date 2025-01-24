using System;

namespace GHUB_Overlay.Model
{
    public class Device
    {
        public string? id { get; set; }
        public string? _stateString { get; set; }

        public bool? state
        {
            get
            {
                return _stateString == "ACTIVE";
            }
        }

        public string? displayName { get; set; }
        public string? deviceType { get; set; }
        public int? percentage { get; set; }
        public bool? charging { get; set; }
        public Device(string? id, string? stateString, string? displayName, string? deviceType)
        {
            this.id = id;
            this._stateString = stateString;
            this.displayName = displayName;
            this.deviceType = deviceType;
        }

        public void SetState(string stateString)
        {
            this._stateString = stateString;
        }
    }
}
