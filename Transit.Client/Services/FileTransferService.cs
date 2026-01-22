using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Transit.Client.Networking;
using Transit.Core.Common;
using Transit.Core.FileTransfer;
using Transit.Core.Networking;
using Transit.Core.Protocol;

namespace Transit.Client.Services
{
    public class FileTransferService
    {
        private readonly IncomingListener _listener;
        // Active transfers
        private readonly ConcurrentDictionary<string, TransferSession> _transfers = new ConcurrentDictionary<string, TransferSession>();

        public event Action<TransferSession> TransferStarted;
        public event Action<TransferSession, double> TransferProgress;
        public event Action<TransferSession> TransferCompleted;
        public event Action<TransferSession, string> TransferFailed;

        public string DownloadDirectory { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LanShareDownloads");

        public FileTransferService(IncomingListener listener)
        {
            _listener = listener;
            _listener.ClientConnected += OnClientConnected;
            Directory.CreateDirectory(DownloadDirectory);
        }

        private void OnClientConnected(TcpClient client)
        {
            Task.Run(() => HandleIncomingTransfer(client));
        }

        private void HandleIncomingTransfer(TcpClient client)
        {
            var connection = new TcpConnection(client);
            TransferSession session = null;
            FileStream fs = null;

            connection.OnMessageReceived += (msg) =>
            {
                if (msg is FileTransferRequestMessage req)
                {
                    // Accept automatically for now, or emit event to ask user
                    session = new TransferSession
                    {
                        TransferId = req.CommandId.ToString(),
                        FileName = req.FileName,
                        TotalSize = req.FileSize,
                        StartTime = DateTime.Now
                    };
                    
                    var path = Path.Combine(DownloadDirectory, session.FileName);
                    // Handle duplicate names...
                    fs = new FileStream(path, FileMode.Create);
                    
                    _transfers.TryAdd(session.TransferId, session);
                    TransferStarted?.Invoke(session);
                }
                else if (msg is FileChunk chunk)
                {
                    if (session != null && fs != null)
                    {
                         fs.Write(chunk.Data, 0, chunk.Data.Length);
                         session.BytesTransferred += chunk.Data.Length;
                         
                         double progress = (double)session.BytesTransferred / session.TotalSize;
                         TransferProgress?.Invoke(session, progress);

                         if (chunk.IsLastChunk)
                         {
                             fs.Close();
                             TransferCompleted?.Invoke(session);
                             connection.Disconnect(); // Done
                         }
                    }
                }
            };

            connection.StartListening(CancellationToken.None);
        }

        public async Task SendFileAsync(string ipAddress, int port, string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var session = new TransferSession
            {
                TransferId = Guid.NewGuid().ToString(),
                FileName = fileInfo.Name,
                TotalSize = fileInfo.Length,
                StartTime = DateTime.Now
            };

            try
            {
                var client = new TcpClient();
                await client.ConnectAsync(ipAddress, port);
                using var connection = new TcpConnection(client);
                
                // Send Request
                var req = new FileTransferRequestMessage
                {
                    FileName = fileInfo.Name,
                    FileSize = fileInfo.Length,
                    SenderMachine = Environment.MachineName
                };
                connection.Send(req);

                // Start sending chunks
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var buffer = new byte[AppConstants.FileChunkSize];
                int bytesRead;
                int chunkIndex = 0;
                
                // Wait small delay? Or assumed accepted.
                // ideally wait for ACK. Assuming ACK for now or immediate send.
                // Simple implementation: Send immediately.

                while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    var chunk = new FileChunk
                    {
                        TransferId = session.TransferId,
                        ChunkIndex = chunkIndex++,
                        Data = new byte[bytesRead]
                    };
                    Array.Copy(buffer, chunk.Data, bytesRead);
                    chunk.IsLastChunk = (fs.Position == fs.Length);
                    
                    connection.Send(chunk);
                    
                    session.BytesTransferred += bytesRead;
                    TransferProgress?.Invoke(session, (double)session.BytesTransferred / session.TotalSize);
                }
                
                TransferCompleted?.Invoke(session);
            }
            catch (Exception ex)
            {
                TransferFailed?.Invoke(session, ex.Message);
            }
        }
    }
}
