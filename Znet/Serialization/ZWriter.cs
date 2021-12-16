﻿using System;

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
                    _returnedBuffer[i] = _buffer[i];
                }
                return _returnedBuffer;
            }
        }

        //MTU - Header size
        private const int BUFFER_MAX_SIZE = 1400 - 12;
        private byte[] _buffer = new byte[BUFFER_MAX_SIZE];
        private int _currentWritePosition = 0;
        private int _initialWritePosition = 0;
        
        public void Init(int _writerPosition)
        {
            _initialWritePosition = _writerPosition;
            _currentWritePosition = _initialWritePosition;
        }
        public void WriteHeader(UInt16 _datagramID, UInt16 _lastAcks, UInt64 _previousAck)
        {
            int _lastWritePosition = _currentWritePosition;
            _currentWritePosition = 0;
            WriteUInt16(_datagramID);
            WriteUInt16(_lastAcks);
            WriteUInt64(_previousAck);
            _currentWritePosition = _lastWritePosition;
        }

        private void WriteByte(byte _byte)
        {
            _buffer[_currentWritePosition] = _byte;
            _currentWritePosition++;
        }

        private void WriteBytes(byte[] _bytes)
        {
            for(int i = 0; i < _bytes.Length; i++)
            {
                WriteByte(_bytes[i]);
            }
        }

        public void WriteUInt16(UInt16 _value) => WriteBytes(BitConverter.GetBytes(_value));
        public void WriteInt32(Int32 _value) => WriteBytes(BitConverter.GetBytes(_value));
        internal void WriteUInt64(UInt64 _value) => WriteBytes(BitConverter.GetBytes(_value));

        public UInt16 WritePacketLength()
        {
            UInt16 _packetLength = (UInt16)_currentWritePosition;
            byte[] _weightArray = BitConverter.GetBytes(_packetLength);
            _buffer[_initialWritePosition + 0] = _weightArray[0];
            _buffer[_initialWritePosition + 1] = _weightArray[1];
            return _packetLength;
        }

    }
}