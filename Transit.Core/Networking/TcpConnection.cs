using System;
using System.Net.Sockets;
using System.Threading;
using Transit.Core.Protocol;

namespace Transit.Core.Networking
{
    public class TcpConnection : IDisposable
    {
        private readonly TcpClient _client;
        private readonly TcpPacketReader _reader;
        private readonly TcpPacketWriter _writer;

        public event Action<BaseMessage> OnMessageReceived;
        public event Action OnDisconnected;

        public bool IsConnected => _client != null && _client.Connected;

        public TcpConnection(TcpClient client)
        {
            _client = client;
            var stream = _client.GetStream();
            _reader = new TcpPacketReader(stream);
            _writer = new TcpPacketWriter(stream);
        }

        public void StartListening(CancellationToken ct)
        {
            new Thread(() =>
            {
                try
                {
                    while (!ct.IsCancellationRequested && IsConnected)
                    {
                        var msg = _reader.ReadMessage();
                        if (msg != null)
                        {
                            OnMessageReceived?.Invoke(msg);
                        }
                    }
                }
                catch
                {
                    // Connection lost or error
                    Disconnect();
                }
            }).Start();
        }

        public void Send(BaseMessage message)
        {
            try
            {
                _writer.WriteMessage(message);
            }
            catch
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            try { _client?.Close(); } catch { }
            OnDisconnected?.Invoke();
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
