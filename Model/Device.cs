using System;

namespace GHUB_Overlay.Model
{
    public class Device
    {
        public string? id { get; set; }
        public State deviceState { get; private set; } = State.NOT_CONNECTED;
        public Type deviceType { get; private set; } = Type.NONE;

        public enum State
        {
            ACTIVE,
            NOT_CONNECTED,
            ABSENT
        }
        public enum Type
        {
            KEYBOARD,
            MOUSE,
            HEADSET,
            NONE
        }
        public string? displayName { get; set; }
        public int? percentage { get; set; }
        public bool? charging { get; set; }
        public Device(string? id, string? stateString, string? displayName, string? deviceType)
        {
            this.id = id;
            SetState(stateString);
            this.displayName = displayName;
            SetDeviceType(deviceType);
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
        public void SetDeviceType(string stateType)
        {
            if (Enum.TryParse(stateType, out Type parsedType))
            {
                deviceType = parsedType;
            }
            else
            {
                deviceType = Type.NONE;
            }
        }
    }
}
