namespace FlexConnect.Shared.Network
{
    public class PacketBuilder
    {
        private OpCode _opCode;

        private MemoryStream _mStreamHead;
        private BinaryWriter _bWriterHead;
        private MemoryStream _mStreamPayload;
        private BinaryWriter _bWriterPayload;

        public PacketBuilder(OpCode opCode) 
        {
            _opCode = opCode;

            _mStreamHead = new MemoryStream();
            _bWriterHead = new BinaryWriter(_mStreamHead);
            _mStreamPayload = new MemoryStream();
            _bWriterPayload = new BinaryWriter(_mStreamPayload);
 
            _bWriterHead.Write(BitConverter.GetBytes((int)_opCode));
        }

        public PacketBuilder Append<T>(T data)
        {
            switch(data)
            {
                case int i:
                    _bWriterPayload.Write(i);
                    break;
                case string s:
                    _bWriterPayload.Write(s);
                    break;
                case byte b:
                    _bWriterPayload.Write(b);
                    break;
                case byte[] ba:
                    _bWriterPayload.Write(ba);
                    break;
            }

            _bWriterPayload.Flush();
            return this;
        }

        public Packet Build()
        {
            _bWriterHead.Write(_mStreamPayload.Length);
            _bWriterHead.Write(_mStreamPayload.ToArray());
            _bWriterHead.Flush();

            return new Packet(_opCode, _mStreamHead.ToArray());
        }
    }
}
