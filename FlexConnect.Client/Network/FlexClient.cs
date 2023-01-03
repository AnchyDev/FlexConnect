using FlexConnect.Shared.Network;
using System.Net;
using System.Net.Sockets;

namespace FlexConnect.Client.Network
{
    public class FlexClient
    {
        private TcpClient _client;
        private IPAddress _ipAddress;
        private int _port;

        public FlexClient(IPAddress ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            _client = new TcpClient();
        }

        public async Task ConnectAsync()
        {
            await _client.ConnectAsync(_ipAddress, _port);
            
            while(true)
            {
                var intBytes = await PacketHandler.ReadAsync<int>(_client.GetStream());
                Console.WriteLine("Found " + (OpCode)BitConverter.ToInt32(intBytes));
            }
        }
    }
}
