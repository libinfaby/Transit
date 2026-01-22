using System;

namespace Transit.Core.Protocol
{
    [Serializable]
    public abstract class BaseMessage
    {
        public MessageType Type { get; set; }
        public Guid CommandId { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
