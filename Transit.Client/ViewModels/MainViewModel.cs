using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Transit.Client.Networking;
using Transit.Client.Services;
using Transit.Core.Common;
using Transit.Core.FileTransfer;
using Transit.Core.Protocol;

namespace Transit.Client.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ServerConnectionService _serverService;
        private readonly ChatService _chatService;
        private readonly FileTransferService _fileTransferService;
        private readonly IncomingListener _incomingListener;

        [ObservableProperty]
        private string _username;
        
        [ObservableProperty]
        private string _machineName;

        [ObservableProperty]
        private string _serverIp = "127.0.0.1";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotConnected))]
        private bool _isConnected;

        public bool IsNotConnected => !IsConnected;

        [ObservableProperty]
        private ObservableCollection<ClientInfo> _onlineClients = new ObservableCollection<ClientInfo>();

        [ObservableProperty]
        private ClientInfo _selectedClient;

        [ObservableProperty]
        private ObservableCollection<string> _chatMessages = new ObservableCollection<string>();

        [ObservableProperty]
        private string _messageInput;
        
        [ObservableProperty]
        private string _transferStatus;

        public MainViewModel()
        {
            _machineName = Environment.MachineName;
            _username = Environment.UserName;

            // Initialize Services
            _serverService = new ServerConnectionService();
            _chatService = new ChatService(_serverService);
            
            // P2P Listener
            _incomingListener = new IncomingListener(AppConstants.DefaultClientPort);
            _incomingListener.Start();
            
            _fileTransferService = new FileTransferService(_incomingListener);

            // Events
            _serverService.Connected += () => IsConnected = true;
            _serverService.Disconnected += () => IsConnected = false;
            _serverService.MessageReceived += OnServerMessageReceived;
            
            _chatService.MessageReceived += (msg) => 
            {
                Application.Current.Dispatcher.Invoke(() => 
                    ChatMessages.Add($"[{msg.SenderMachine}]: {msg.Content}"));
            };

            _fileTransferService.TransferStarted += (session) => TransferStatus = $"Receiving {session.FileName}...";
            _fileTransferService.TransferProgress += (session, progress) => TransferStatus = $"Transferring {session.FileName}: {progress:P0}";
            _fileTransferService.TransferCompleted += (session) => TransferStatus = $"Completed {session.FileName}";
        }

        private void OnServerMessageReceived(BaseMessage msg)
        {
            if (msg is PeerListUpdateMessage update)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnlineClients.Clear();
                    foreach (var client in update.Clients)
                    {
                        if (client.MachineName != _machineName) // Exclude self
                             OnlineClients.Add(client);
                    }
                });
            }
        }

        [RelayCommand]
        public async Task Connect()
        {
            try
            {
                var reg = new RegisterMessage
                {
                    Username = Username,
                    MachineName = MachineName,
                    IpAddress = NetworkingUtils.GetLocalIpAddress(), // Need a helper
                    ListeningPort = AppConstants.DefaultClientPort,
                    OfficeId = "MainOffice"
                };
                
                await _serverService.ConnectAsync(ServerIp, reg);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}");
            }
        }

        [RelayCommand]
        public void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(MessageInput)) return;

            string target = SelectedClient?.MachineName; // Or null for broadcast if none selected
            _chatService.SendMessage(MessageInput, target);
            ChatMessages.Add($"[Me -> {target ?? "All"}]: {MessageInput}");
            MessageInput = "";
        }

        [RelayCommand]
        public async Task SendFile()
        {
            if (SelectedClient == null)
            {
                MessageBox.Show("Select a client to send file to.");
                return;
            }

            var dialog = new Microsoft.Win32.OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                TransferStatus = $"Sending {dialog.FileName}...";
                await _fileTransferService.SendFileAsync(SelectedClient.IpAddress, SelectedClient.ListeningPort, dialog.FileName);
            }
        }
    }

    public static class NetworkingUtils
    {
        public static string GetLocalIpAddress()
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }
    }
}
