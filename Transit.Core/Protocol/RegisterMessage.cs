using System;

namespace Transit.Core.Protocol
{
    [Serializable]
    public class RegisterMessage : BaseMessage
    {
        public string Username { get; set; }
        public string MachineName { get; set; }
        public string IpAddress { get; set; }
        public int ListeningPort { get; set; }
        public string OfficeId { get; set; }

        public RegisterMessage()
        {
            Type = MessageType.Register;
        }
    }
}
