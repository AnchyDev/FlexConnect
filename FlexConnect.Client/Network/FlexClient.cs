using FlexConnect.Shared.Logging;
using FlexConnect.Shared.Network;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;

namespace FlexConnect.Client.Network
{
    public class FlexClient
    {
        private TcpClient _client;
        private IPAddress _ipAddress;
        private int _port;

        private ILogger _logger;

        public FlexClient(IPAddress ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            _client = new TcpClient();

            _logger = new LoggerConsole();
        }

        public async Task ConnectAsync()
        {
            try
            {
                await _client.ConnectAsync(_ipAddress, _port);

                if(!await HandshakeAsync())
                {
                    await _logger.LogAsync(LogLevel.Error, "Failed handshake.");
                }

            }
            catch (Exception ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"{ex}");
            }

            await Task.Delay(-1);
        }

        private async Task<bool> HandshakeAsync()
        {
            var opCode = await PacketHandler.ReadOpCodeAsync(_client.GetStream());

            if (opCode != OpCode.Auth)
            {
                await _logger.LogAsync(LogLevel.Error, "OpCode was not Auth during authentication.");
                return false;
            }

            var lenBytes = await PacketHandler.ReadAsync<int>(_client.GetStream());
            var len = BitConverter.ToInt32(lenBytes);

            var handshakeBytes = await PacketHandler.ReadAsync<byte[]>(_client.GetStream(), len);

            var packet = new PacketBuilder(OpCode.Auth)
                .Append(handshakeBytes.Length)
                .Append(handshakeBytes)
                .Build();

            await PacketHandler.SendAsync(_client.GetStream(), packet);

            return true;
        }
    }
}
