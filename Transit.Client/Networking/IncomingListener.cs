using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Transit.Core.Common;

namespace Transit.Client.Networking
{
    public class IncomingListener
    {
        private TcpListener _listener;
        private CancellationTokenSource _cts;
        private readonly int _port;

        public event Action<TcpClient> ClientConnected;

        public IncomingListener(int port)
        {
            _port = port;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            
            Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var client = await _listener.AcceptTcpClientAsync(_cts.Token);
                        ClientConnected?.Invoke(client);
                    }
                    catch { }
                }
            });
        }

        public void Stop()
        {
            _cts?.Cancel();
            _listener?.Stop();
        }
    }
}
