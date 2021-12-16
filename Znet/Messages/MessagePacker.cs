using System;
using Znet.Serialization;
using Znet.Utils;

namespace Znet.Messages
{
    public class MessagePacker : IMessagePacker
    {
        public void PackMessage<T>(ref ZWriter _writer, T _message, out ushort _length) where T : ZNetworkMessage
        {
            int _hash = _message.GetType().ToString().GetFixedHashCode();
            _writer.Init(Datagram.HeaderSize);
            _writer.WriteUInt16(2);
            _writer.WriteInt32(_hash);
            _message.Serialize(ref _writer);
            _length = _writer.WritePacketLength();
        }

        public bool Unpack(ref ZReader _reader, byte[] _buffer, out int _hash)
        {
            _reader.Init(_buffer, 12);
            _reader.ReadUInt16();
            _hash = _reader.ReadInt32();
            return _hash != -1;
        }
    }
}