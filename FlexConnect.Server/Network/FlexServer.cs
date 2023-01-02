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
            Console.WriteLine($"{tcpClient.Client.RemoteEndPoint} connected.");
        }
    }
}
