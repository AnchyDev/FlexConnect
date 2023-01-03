using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexConnect.Shared.Network
{
    public class Packet
    {
        public OpCode OpCode { get; private set; }
        public byte[] Payload { get; private set; }

        public Packet(OpCode opCode, byte[] payload)
        {
            OpCode = opCode;
            Payload = payload;
        }
    }
}
