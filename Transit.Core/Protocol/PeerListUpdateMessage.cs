using System;
using System.Collections.Generic;

namespace Transit.Core.Protocol
{
    [Serializable]
    public class PeerListUpdateMessage : BaseMessage
    {
        public List<ClientInfo> Clients { get; set; } = new List<ClientInfo>();

        public PeerListUpdateMessage()
        {
            Type = MessageType.PeerListUpdate;
        }
    }

    public class ClientInfo
    {
        public string MachineName { get; set; }
        public string IpAddress { get; set; }
        public int ListeningPort { get; set; }
        public string Username { get; set; }
    }
}
