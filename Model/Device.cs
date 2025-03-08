using System;

namespace GHUB_Overlay.Model
{
    public class Device
    {
        public string? id { get; set; }
        public State deviceState { get; private set; } = State.NOT_CONNECTED;

        public enum State
        {
            ACTIVE,
            NOT_CONNECTED,
            ABSENT
        }

        public string? displayName { get; set; }
        public string? deviceType { get; set; }
        public int? percentage { get; set; }
        public bool? charging { get; set; }
        public Device(string? id, string? stateString, string? displayName, string? deviceType)
        {
            this.id = id;
            SetState(stateString);
            this.displayName = displayName;
            this.deviceType = deviceType;
        }

        public void SetState(string stateString)
        {
            if (Enum.TryParse(stateString, out State parsedState))
            {
                deviceState = parsedState;
            }
            else
            {
                deviceState = State.NOT_CONNECTED;
            }
        }
    }
}
