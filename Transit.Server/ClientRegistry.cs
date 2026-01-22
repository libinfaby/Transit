using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Transit.Core.Networking;
using Transit.Core.Protocol;

namespace Transit.Server
{
    public class ClientRegistry
    {
        // Using ConcurrentDictionary for thread safety
        private readonly ConcurrentDictionary<string, ConnectedClient> _clients = new ConcurrentDictionary<string, ConnectedClient>();

        public void Register(string clientId, RegisterMessage info, TcpConnection connection)
        {
            var client = new ConnectedClient
            {
                ClientId = clientId, // could be MachineName or unique ID
                Info = info,
                Connection = connection,
                LastHeartbeat = DateTime.Now
            };

            _clients.AddOrUpdate(clientId, client, (k, v) => client);
        }

        public void Remove(string clientId)
        {
            if (_clients.TryRemove(clientId, out var client))
            {
                client.Connection.Disconnect();
            }
        }

        public void UpdateHeartbeat(string clientId)
        {
            if (_clients.TryGetValue(clientId, out var client))
            {
                client.LastHeartbeat = DateTime.Now;
            }
        }

        public IEnumerable<ConnectedClient> GetAllClients()
        {
            return _clients.Values;
        }

        public ConnectedClient GetClient(string clientId)
        {
            _clients.TryGetValue(clientId, out var client);
            return client;
        }
    }

    public class ConnectedClient
    {
        public string ClientId { get; set; }
        public RegisterMessage Info { get; set; }
        public TcpConnection Connection { get; set; }
        public DateTime LastHeartbeat { get; set; }
    }
}
