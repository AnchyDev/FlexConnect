﻿using FlexConnect.Shared.Logging;
using FlexConnect.Shared.Network;
using System.Net;
using System.Net.Sockets;

namespace FlexConnect.Server.Network
{
    public class FlexServer
    {
        private TcpListener _listener;

        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _cancelToken;

        private ILogger _logger;

        public FlexServer(IPAddress ipAddress, int port)
        {
            _listener = new TcpListener(ipAddress, port);

            _cancelTokenSource = new CancellationTokenSource();
            _cancelToken = _cancelTokenSource.Token;

            _logger = new LoggerConsole();
        }

        public async Task StartAsync()
        {
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
                // Handshake
                {
                    await _logger.LogAsync(LogLevel.Info, $"Client '{tcpClient.Client.RemoteEndPoint}' connected.");

                    byte[] payload = Guid.NewGuid().ToByteArray();

                    var packet = new PacketBuilder(OpCode.Auth)
                        .Append<byte[]>(payload)
                        .Build();

                    await PacketHandler.SendAsync(tcpClient.GetStream(), packet);

                    var opCode = await PacketHandler.ReadOpCodeAsync(tcpClient.GetStream());

                    if (opCode != OpCode.Auth)
                    {
                        await _logger.LogAsync(LogLevel.Error, $"Client '{tcpClient.Client.RemoteEndPoint}' tried sending opcode {opCode} instead of {OpCode.Auth} before Auth was complete. Disconnecting.");
                        await DisconnectUser(tcpClient);
                        return;
                    }

                    var lenBytes = await PacketHandler.ReadAsync<int>(tcpClient.GetStream());
                    var len = BitConverter.ToInt32(lenBytes);
                    var responsePayload = await PacketHandler.ReadAsync<byte[]>(tcpClient.GetStream(), len);

                    // FIXME: We are receing the whole packet as payload
                    if(payload != responsePayload)
                    {
                        await _logger.LogAsync(LogLevel.Error, $"Client '{tcpClient.Client.RemoteEndPoint}' failed to match handshake guid. Disconnecting.");
                        await DisconnectUser(tcpClient);
                        return;
                    }

                    await _logger.LogAsync(LogLevel.Debug, $"Client '{tcpClient.Client.RemoteEndPoint}' passed handshake.");
                }
            }
            catch(Exception ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"{ex}");
            }
        }

        private async Task DisconnectUser(TcpClient tcpClient)
        {
            await Task.Run(tcpClient.Close);
        }
    }
}
