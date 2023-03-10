using FlexConnect.Server.Database;
using FlexConnect.Shared.Logging;
using FlexConnect.Shared.Network;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace FlexConnect.Server.Network
{
    public class FlexServer
    {
        private TcpListener _listener;

        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _cancelToken;

        private ILogger _logger;

        private DatabaseHandler _db;

        public FlexServer(IPAddress ipAddress, int port)
        {
            _listener = new TcpListener(ipAddress, port);

            _cancelTokenSource = new CancellationTokenSource();
            _cancelToken = _cancelTokenSource.Token;

            _logger = new LoggerConsole();

            _db = new DatabaseHandler("root", "root", "testdb", "127.0.0.1");
        }

        public async Task StartAsync()
        {
            await _db.ConnectAsync();
            await Task.Run(_listener.Start);
            await AcceptClientsAsync();
        }

        public async Task StopAsync()
        {
            await Task.Run(_cancelTokenSource.Cancel);
            await Task.Run(_listener.Stop);
        }

        private async Task AcceptClientsAsync()
        {
            while (!_cancelToken.IsCancellationRequested)
            {
                try
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync();

                    if (tcpClient != null)
                    {
                        await HandleClientAsync(tcpClient).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    await _logger.LogAsync(LogLevel.Error, $"{ex}");
                }
            }
        }

        private async Task HandleClientAsync(TcpClient tcpClient)
        {
            try
            {
                if(!await HandshakeAsync(tcpClient))
                {
                    await _logger.LogAsync(LogLevel.Error, $"Client '{tcpClient.Client.RemoteEndPoint}' failed handshake. Disconnecting.");
                    await DisconnectUserAsync(tcpClient);
                }

                await HandlePacketsAsync(tcpClient);
            }
            catch(Exception ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"{ex}");
            }
        }

        private async Task HandlePacketsAsync(TcpClient tcpClient)
        {
            var opCode = await PacketHandler.ReadOpCodeAsync(tcpClient.GetStream());

            switch(opCode)
            {
                case OpCode.ReqList:
                    
                    var masterList = await _db.GetServerInfoAsync();

                    string serializedList = JsonSerializer.Serialize(masterList);
                    byte[] listPayload = Encoding.UTF8.GetBytes(serializedList);

                    var packet = new PacketBuilder(OpCode.ReqList)
                        .Append(listPayload.Length)
                        .Append(listPayload)
                        .Build();

                    await PacketHandler.SendAsync(tcpClient.GetStream(), packet);
                    break;
            }
        }

        private async Task<bool> HandshakeAsync(TcpClient tcpClient)
        {
            await _logger.LogAsync(LogLevel.Info, $"Client '{tcpClient.Client.RemoteEndPoint}' connected.");

            byte[] payload = Guid.NewGuid().ToByteArray();

            var packet = new PacketBuilder(OpCode.Auth)
                .Append<int>(payload.Length)
                .Append<byte[]>(payload)
                .Build();

            await PacketHandler.SendAsync(tcpClient.GetStream(), packet);

            var opCode = await PacketHandler.ReadOpCodeAsync(tcpClient.GetStream());

            if (opCode != OpCode.Auth)
            {
                await _logger.LogAsync(LogLevel.Error, $"Client '{tcpClient.Client.RemoteEndPoint}' tried sending opcode {opCode} instead of {OpCode.Auth} before Auth was complete. Disconnecting.");
                await DisconnectUserAsync(tcpClient);
                return false;
            }

            var lenBytes = await PacketHandler.ReadAsync<int>(tcpClient.GetStream());
            var len = BitConverter.ToInt32(lenBytes);
            var responsePayload = await PacketHandler.ReadAsync<byte[]>(tcpClient.GetStream(), len);

            if (new Guid(payload).CompareTo(new Guid(responsePayload)) != 0)
            {
                await _logger.LogAsync(LogLevel.Error, $"Client '{tcpClient.Client.RemoteEndPoint}' failed to match handshake guid. Disconnecting.");
                await DisconnectUserAsync(tcpClient);
                return false;
            }

            await _logger.LogAsync(LogLevel.Debug, $"Client '{tcpClient.Client.RemoteEndPoint}' passed handshake.");
            return true;
        }

        private async Task DisconnectUserAsync(TcpClient tcpClient)
        {
            await Task.Run(tcpClient.Close);
        }
    }
}
