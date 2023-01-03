using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FlexConnect.Shared.Network
{
    public class PacketHandler
    {
        public static async Task<byte[]> ReadAsync<T>(NetworkStream netStream)
        {
            switch(Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Int32:
                    byte[] buffer = new byte[4];
                    await netStream.ReadAsync(buffer, 0, sizeof(Int32));
                    return buffer;
            }

            throw new NotImplementedException($"The type '{typeof(T)}' is not a valid type for ReadAsync<T>.");
        }

        public static async Task SendAsync(NetworkStream netStream, Packet packet)
        {
            await netStream.WriteAsync(packet.Payload);
            await netStream.FlushAsync();
        }
    }
}
