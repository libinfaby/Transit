using System;

namespace Transit.Core.Protocol
{
    [Serializable]
    public class TextMessage : BaseMessage
    {
        public string Content { get; set; }
        public string TargetMachine { get; set; } // null or empty for broadcast/group
        public string SenderMachine { get; set; }

        public TextMessage()
        {
            Type = MessageType.Text;
        }
    }
}
