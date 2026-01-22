using System;
using System.Collections.Generic;

namespace Transit.Core.FileTransfer
{
    public class TransferSession
    {
        public string TransferId { get; set; }
        public string FileName { get; set; }
        public long TotalSize { get; set; }
        public long BytesTransferred { get; set; }
        public DateTime StartTime { get; set; }
        
        // Helper to track chunks if needed for reassembly
        // For simple sequential transfer, maybe just a file stream
    }
}
