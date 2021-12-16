using Znet.Serialization;

namespace Znet.Messages
{
    public interface IMessagePacker
    {
        public void PackMessage<T>(ref ZWriter _writer, T _message, out ushort _packetLength) where T : ZNetworkMessage;
        public bool Unpack(ref ZReader _reader, byte[] _buffer, out int _hash);
    }
}