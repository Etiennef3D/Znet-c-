using System;
using System.Collections.Generic;

namespace Znet.Serialization
{
    public class ZReader
    {
        private const int BUFFER_MAX_SIZE = 1400 - 12;
        private byte[] _buffer = new byte[BUFFER_MAX_SIZE];
        private int m_CurrentReadPosition;

        private byte[] m_UInt16Buffer = new byte[2];
        private byte[] m_Int32Buffer = new byte[4];
        private byte[] m_Int64Buffer = new byte[8];

        private List<object> _objects;

        public void Init(byte[] _buffer, int _readingPos = 0)
        {
            Console.WriteLine($"Initializing reader at position: {_readingPos}");
            this._buffer = _buffer;
            _objects = new List<object>();
            m_CurrentReadPosition = _readingPos;
        }

        public byte ReadByte()
        {
            byte _result = _buffer[m_CurrentReadPosition];
            m_CurrentReadPosition++;
            return _result;
        }

        public UInt16 ReadUInt16()
        {
            Array.Copy(_buffer, m_CurrentReadPosition, m_UInt16Buffer, 0, 2);
            UInt16 _result = BitConverter.ToUInt16(m_UInt16Buffer, 0);
            m_CurrentReadPosition += 2;
            _objects.Add(_result);
            return _result;
        }

        public UInt64 ReadUInt64()
        {
            Array.Copy(_buffer, m_CurrentReadPosition, m_Int64Buffer, 0, 8);
            UInt64 _result = BitConverter.ToUInt16(m_Int64Buffer, 0);
            m_CurrentReadPosition += 8;
            _objects.Add(_result);
            return _result;
        }

        public Int32 ReadInt32()
        {
            Array.Copy(_buffer, m_CurrentReadPosition, m_Int32Buffer, 0, 4);
            Int32 _result = BitConverter.ToInt32(m_Int32Buffer, 0);
            m_CurrentReadPosition += 4;
            _objects.Add(_result);
            return _result;
        }
    }
}
