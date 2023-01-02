using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FlexConnect.Server.Network
{
    public class FlexServer
    {
        private TcpListener _listener;

        public FlexServer(IPAddress ipAddress, int port)
        {
            _listener = new TcpListener(ipAddress, port);
        }

        public async Task StartAsync()
        {
            await Task.Run(_listener.Start);
            await AcceptClientsAsync();
        }

        private async Task AcceptClientsAsync()
        {
            var tcpClient = await _listener.AcceptTcpClientAsync();

            if(tcpClient != null)
            {
                await HandleClientAsync(tcpClient).ConfigureAwait(false);
            }
        }

        private async Task HandleClientAsync(TcpClient tcpClient)
        {
            Console.WriteLine($"{tcpClient.Client.RemoteEndPoint} connected.");
        }
    }
}
