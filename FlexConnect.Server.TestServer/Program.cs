using FlexConnect.Server.Network;
using System.Net;

namespace FlexConnect.Server.TestServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var server = new FlexServer(IPAddress.Any, 4411);
            await server.StartAsync();
        }
    }
}