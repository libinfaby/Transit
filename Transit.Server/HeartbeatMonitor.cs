using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Transit.Core.Common;

namespace Transit.Server
{
    public class HeartbeatMonitor : BackgroundService
    {
        private readonly ClientRegistry _registry;

        public HeartbeatMonitor(ClientRegistry registry)
        {
            _registry = registry;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var timeout = TimeSpan.FromSeconds(30); // 30s timeout

                foreach (var client in _registry.GetAllClients())
                {
                    if (now - client.LastHeartbeat > timeout)
                    {
                        Logger.Log($"Client {client.ClientId} timed out.");
                        _registry.Remove(client.ClientId);
                        // Broadcast removal?
                    }
                }

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
