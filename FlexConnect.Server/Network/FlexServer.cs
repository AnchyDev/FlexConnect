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

        public FlexServer(IPAddress ipAddress, int port)
        {
            _listener = new TcpListener(ipAddress, port);

            _cancelTokenSource = new CancellationTokenSource();
            _cancelToken = _cancelTokenSource.Token;
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
                var tcpClient = await _listener.AcceptTcpClientAsync();

                if (tcpClient != null)
                {
                    await HandleClientAsync(tcpClient).ConfigureAwait(false);
                }
            }
        }

        private async Task HandleClientAsync(TcpClient tcpClient)
        {
            // Handshake
            {
                byte[] payload = Guid.NewGuid().ToByteArray();

                var packet = new PacketBuilder(OpCode.Auth)
                    .Append<byte[]>(payload)
                    .Build();

                await PacketHandler.SendAsync(tcpClient.GetStream(), packet);

                var opCodeBytes = await PacketHandler.ReadAsync<int>(tcpClient.GetStream());
                var opCode = (OpCode)BitConverter.ToInt32(opCodeBytes, 0);

                if(opCode != OpCode.Auth)
                {
                    await DisconnectUser(tcpClient);
                    return;
                }
            }
        }

        private async Task DisconnectUser(TcpClient tcpClient)
        {
            await Task.Run(tcpClient.Close);
        }
    }
}
