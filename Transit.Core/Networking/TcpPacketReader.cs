using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Transit.Core.Protocol;
using Transit.Core.FileTransfer;

namespace Transit.Core.Networking
{
    public class TcpPacketReader
    {
        private readonly NetworkStream _stream;

        public TcpPacketReader(NetworkStream stream)
        {
            _stream = stream;
        }

        public BaseMessage ReadMessage()
        {
            // Read length prefix
            var lengthBuffer = new byte[4];
            int bytesRead = 0;
            while (bytesRead < 4)
            {
                int read = _stream.Read(lengthBuffer, bytesRead, 4 - bytesRead);
                if (read == 0) throw new IOException("Connection closed");
                bytesRead += read;
            }

            int length = BitConverter.ToInt32(lengthBuffer, 0);

            // Read payload
            var payloadBuffer = new byte[length];
            bytesRead = 0;
            while (bytesRead < length)
            {
                int read = _stream.Read(payloadBuffer, bytesRead, length - bytesRead);
                if (read == 0) throw new IOException("Connection closed prematurely during payload read");
                bytesRead += read;
            }

            var json = Encoding.UTF8.GetString(payloadBuffer);
            
            // Deserialize based on MessageType property? 
            // We need to know the type first. 
            // Simple approach: Deserialize to BaseMessage (or JsonNode) to get Type, then re-deserialize to concrete.
            // Or better: Include a type header? 
            // For now, let's just peek the Type using a lightweight parse or just parse to JsonElement first.
            
            using (var doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;
                if (root.TryGetProperty("Type", out var typeProp))
                {
                    int typeInt = typeProp.GetInt32();
                    var type = (MessageType)typeInt;
                    
                    switch (type)
                    {
                        case MessageType.Register: return JsonSerializer.Deserialize<RegisterMessage>(json);
                        case MessageType.Heartbeat: return JsonSerializer.Deserialize<HeartbeatMessage>(json);
                        case MessageType.Text: return JsonSerializer.Deserialize<TextMessage>(json);
                        case MessageType.PeerListUpdate: return JsonSerializer.Deserialize<PeerListUpdateMessage>(json);
                        case MessageType.FileTransferRequest: return JsonSerializer.Deserialize<FileTransferRequestMessage>(json);
                        case MessageType.FileChunk: return JsonSerializer.Deserialize<FileChunk>(json);
                        case MessageType.Disconnect: return null; // Or specific DisconnectMessage if needed
                        // For generic/unknown:
                        default: return null; // Or throw
                    }
                }
            }
            return null;
        }
    }
}
