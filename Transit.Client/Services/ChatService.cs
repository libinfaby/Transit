using System;
using Transit.Core.Protocol;

namespace Transit.Client.Services
{
    public class ChatService
    {
        private readonly ServerConnectionService _server;

        public event Action<TextMessage> MessageReceived;

        public ChatService(ServerConnectionService server)
        {
            _server = server;
            _server.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(BaseMessage msg)
        {
            if (msg is TextMessage txt)
            {
                MessageReceived?.Invoke(txt);
            }
        }

        public void SendMessage(string content, string targetMachine = null)
        {
            var msg = new TextMessage
            {
                Content = content,
                TargetMachine = targetMachine,
                SenderMachine = Environment.MachineName // Or get from config
            };
            _server.Send(msg);
        }
    }
}
