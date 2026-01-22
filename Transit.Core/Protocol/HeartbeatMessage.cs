using System;

namespace Transit.Core.Protocol
{
    [Serializable]
    public class HeartbeatMessage : BaseMessage
    {
        public string MachineName { get; set; }

        public HeartbeatMessage()
        {
            Type = MessageType.Heartbeat;
        }
    }
}
