using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Text.Json;
using Transit.Core.Protocol;

namespace Transit.Core.Networking
{
    public class TcpPacketWriter
    {
        private readonly NetworkStream _stream;

        public TcpPacketWriter(NetworkStream stream)
        {
            _stream = stream;
        }

        public void WriteMessage(BaseMessage message)
        {
            // Serialize message to JSON (simple for now)
            // In a production generic system, maybe binary or protobuf
            // keeping it simple with System.Text.Json
            var json = JsonSerializer.Serialize(message, message.GetType());
            var bytes = Encoding.UTF8.GetBytes(json);
            
            // Write length prefix (4 bytes)
            var length = BitConverter.GetBytes(bytes.Length);
            _stream.Write(length, 0, length.Length);
            
            // Write payload
            _stream.Write(bytes, 0, bytes.Length);
            _stream.Flush();
        }
    }
}
