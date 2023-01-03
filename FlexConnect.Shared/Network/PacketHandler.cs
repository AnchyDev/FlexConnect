using System.Net.Http;
using System.Net.Sockets;
using System.Reflection.Emit;

namespace FlexConnect.Shared.Network
{
    public class PacketHandler
    {
        public static async Task<byte[]> ReadAsync<T>(NetworkStream netStream, int length = 0)
        {
            Type readType = typeof(T);
            byte[] buffer;

            switch (readType.Name)
            {
                case "Int32":
                    buffer = new byte[4];
                    await netStream.ReadAsync(buffer, 0, sizeof(Int32));
                    return buffer;

                case "Byte[]":
                    buffer = new byte[length];
                    await netStream.ReadAsync(buffer, 0, length);
                    return buffer;
            }

            throw new NotImplementedException($"The type '{typeof(T)}' is not a valid type for ReadAsync<T>.");
        }

        public static async Task<OpCode> ReadOpCodeAsync(NetworkStream netStream)
        {
            var opCodeBytes = await ReadAsync<int>(netStream);
            var opCode = (OpCode)BitConverter.ToInt32(opCodeBytes, 0);

            return opCode;
        }

        public static async Task SendAsync(NetworkStream netStream, Packet packet)
        {
            await netStream.WriteAsync(packet.Payload);
            await netStream.FlushAsync();
        }
    }
}
