using System;
using Transit.Core.Protocol;

namespace Transit.Core.FileTransfer
{
    [Serializable]
    public class FileChunk : BaseMessage
    {
        public string TransferId { get; set; } // Unique ID for the file transfer session
        public int ChunkIndex { get; set; }
        public bool IsLastChunk { get; set; }
        public byte[] Data { get; set; }

        public FileChunk()
        {
            Type = MessageType.FileChunk;
        }
    }
}
