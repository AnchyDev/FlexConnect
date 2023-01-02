using FlexConnect.Client.Network;
using System.Net;

namespace FlexConnect.Client.TestClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var client = new FlexClient(IPAddress.Parse("127.0.0.1"), 4411);
            await client.ConnectAsync();
        }
    }
}