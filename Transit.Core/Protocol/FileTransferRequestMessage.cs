using System;

namespace Transit.Core.Protocol
{
    [Serializable]
    public class FileTransferRequestMessage : BaseMessage
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string SenderMachine { get; set; }

        public FileTransferRequestMessage()
        {
            Type = MessageType.FileTransferRequest;
        }
    }
}
