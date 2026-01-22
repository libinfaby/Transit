using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Transit.Core.Common;
using Transit.Core.Networking;
using Transit.Core.Protocol;

namespace Transit.Server
{
    public class ServerHost : BackgroundService
    {
        private readonly TcpListener _listener;
        private readonly ClientRegistry _registry;

        public ServerHost(ClientRegistry registry)
        {
            _registry = registry;
            _listener = new TcpListener(IPAddress.Any, AppConstants.DefaultServerPort);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _listener.Start();
            Logger.Log($"Server started on port {AppConstants.DefaultServerPort}");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync(stoppingToken);
                    HandleNewClient(tcpClient, stoppingToken);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    Logger.LogError("AcceptClient", ex);
                }
            }
            
            _listener.Stop();
        }

        private void HandleNewClient(TcpClient tcpClient, CancellationToken ct)
        {
           Task.Run(() => 
           {
               var connection = new TcpConnection(tcpClient);
               
               // Temporary storage until registered
               string clientId = null;

               connection.OnMessageReceived += (msg) =>
               {
                   if (msg.Type == MessageType.Register)
                   {
                       var regMsg = (RegisterMessage)msg;
                       clientId = regMsg.MachineName; // Or create a unique ID
                       _registry.Register(clientId, regMsg, connection);
                       Logger.Log($"Client Registered: {clientId} at {regMsg.IpAddress}");
                       
                       // Acknowledge or Broadcast?
                       // Accessing _registry to broadcast to all
                       BroadcastPeerList();
                   }
                   else if (msg.Type == MessageType.Heartbeat)
                   {
                       if (clientId != null)
                           _registry.UpdateHeartbeat(clientId);
                   }
               };

               connection.OnDisconnected += () =>
               {
                   if (clientId != null)
                   {
                       _registry.Remove(clientId);
                       Logger.Log($"Client Disconnected: {clientId}");
                       BroadcastPeerList();
                   }
               };

               connection.StartListening(ct);

           }, ct);
        }

        private void BroadcastPeerList()
        {
            // Simple broadcast: Send updated list to everyone
            // TODO: Create PeerListUpdateMessage
            // For now, let's just log or send dummy
        }
    }
}
