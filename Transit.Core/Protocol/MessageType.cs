namespace Transit.Core.Protocol
{
    public enum MessageType
    {
        Register = 1,
        Heartbeat = 2,
        PeerListUpdate = 3,
        Text = 4,
        FileTransferRequest = 5,
        FileChunk = 6,
        FileTransferAck = 7,
        Disconnect = 8
    }
}
