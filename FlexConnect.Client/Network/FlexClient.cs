using FlexConnect.Shared.Logging;
using FlexConnect.Shared.MasterList;
using FlexConnect.Shared.Network;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

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
                    return;
                }

                if(!await RequestMasterListAsync())
                {
                    await _logger.LogAsync(LogLevel.Error, "Failed to receive master list.");
                    return;
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

        private async Task<bool> RequestMasterListAsync()
        {
            var packet = new PacketBuilder(OpCode.ReqList)
                        .Build();

            await PacketHandler.SendAsync(_client.GetStream(), packet);

            var opCode = await PacketHandler.ReadOpCodeAsync(_client.GetStream());

            if (opCode != OpCode.ReqList)
            {
                await _logger.LogAsync(LogLevel.Error, "OpCode was not ReqList during RequestMasterList.");
                return false;
            }

            var payloadSizeBytes = await PacketHandler.ReadAsync<int>(_client.GetStream());
            var payloadSize = BitConverter.ToInt32(payloadSizeBytes, 0);

            var payload = await PacketHandler.ReadAsync<byte[]>(_client.GetStream(), payloadSize);
            string serializedPayload = Encoding.UTF8.GetString(payload);

            var serverList = JsonSerializer.Deserialize<List<ServerInfo>>(serializedPayload);

            if(serverList == null)
            {
                await _logger.LogAsync(LogLevel.Error, "ServerList was null.");
                return false;
            }

            foreach(var server in serverList)
            {
                await _logger.LogAsync(LogLevel.Debug, $"Received server '{server.Name}':'{server.Description}' with realmlist '{server.Realmlist}'.");
            }

            return true;
        }
    }
}
