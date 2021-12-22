using System;
using System.Text;
using Znet.Messages.Packet;

namespace Znet.Serialization
{
    public class ZWriter
    {
        public byte[] Buffer
        {
            get
            {
                byte[] _returnedBuffer = new byte[_initialWritePosition + _currentWritePosition];

                for(int i = 0; i < _initialWritePosition + _currentWritePosition; i++)
                {
                    _returnedBuffer[i] = _writerBuffer[i];
                }
                return _returnedBuffer;
            }
        }

        //MTU - Header size
        private byte[] _writerBuffer = new byte[Packet.PacketMaxSize];
        private int _currentWritePosition = 0;
        private int _initialWritePosition = 0;

        public void Init(int _writerPosition)
        {
            _initialWritePosition = _writerPosition;
            _currentWritePosition = _initialWritePosition;
        }

        public void WriteByte(byte _byte)
        {
            _writerBuffer[_currentWritePosition] = _byte;
            _currentWritePosition++;
        }

        public void WriteBytes(byte[] _bytes)
        {
            for(int i = 0; i < _bytes.Length; i++)
            {
                WriteByte(_bytes[i]);
            }
        }

        private void WriteBytesInBuffer(byte[] _content, ref byte[] _buffer)
        {
            for(int i = 0; i < _content.Length; i++)
            {
                Console.WriteLine($"Write byte {_content[i]} in buffer");
                _buffer[_currentWritePosition] = _content[i];
                _currentWritePosition++;
            }
        }

        public void WriteUInt16(UInt16 _value) => WriteBytes(BitConverter.GetBytes(_value));
        public void WriteInt32(Int32 _value) => WriteBytes(BitConverter.GetBytes(_value));
        internal void WriteUInt64(UInt64 _value) => WriteBytes(BitConverter.GetBytes(_value));

        public UInt16 WritePacketLength()
        {
            UInt16 _packetLength = (UInt16)_currentWritePosition;
            byte[] _weightArray = BitConverter.GetBytes(_packetLength);
            _writerBuffer[_initialWritePosition + 0] = _weightArray[0];
            _writerBuffer[_initialWritePosition + 1] = _weightArray[1];
            return _packetLength;
        }

        public void WritePacket(Packet _packet, ref byte[] _buffer)
        {
            Console.WriteLine($"Write packet id{_packet.header.ID}. Current write position: " + _currentWritePosition);
            WriteBytesInBuffer(BitConverter.GetBytes((UInt16)_packet.header.ID), ref _buffer);
            WriteByte((byte)_packet.header.Type);
            WriteBytesInBuffer(BitConverter.GetBytes(_packet.header.PayloadSize), ref _buffer);

            WriteBytesInBuffer(_packet.data, ref _buffer);
        }
    }
}