using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Transit.Core.Common;
using Transit.Core.Networking;
using Transit.Core.Protocol;

namespace Transit.Client.Services
{
    public class ServerConnectionService : IDisposable
    {
        private TcpClient _client;
        private TcpConnection _connection;
        private CancellationTokenSource _cts;

        public event Action<BaseMessage> MessageReceived;
        public event Action Connected;
        public event Action Disconnected;

        public bool IsConnected => _connection?.IsConnected ?? false;

        public async Task ConnectAsync(string serverIp, RegisterMessage registrationInfo)
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(serverIp, AppConstants.DefaultServerPort);
                _connection = new TcpConnection(_client);
                
                _cts = new CancellationTokenSource();
                
                _connection.OnMessageReceived += (msg) => MessageReceived?.Invoke(msg);
                _connection.OnDisconnected += () => 
                {
                    Disconnected?.Invoke();
                    // Auto-reconnect logic could go here or in ViewModel
                };

                _connection.StartListening(_cts.Token);
                
                // Send Registration
                _connection.Send(registrationInfo);
                
                Connected?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.LogError("ConnectToServer", ex);
                throw;
            }
        }

        public void Send(BaseMessage msg)
        {
            if (IsConnected)
                _connection.Send(msg);
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _connection?.Dispose();
        }
    }
}
