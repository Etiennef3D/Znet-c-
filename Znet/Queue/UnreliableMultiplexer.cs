using System;
using System.Diagnostics;
using Znet.Serialization;
using Znet.Messages.Packet;
using System.Collections.Generic;

namespace Znet.Queue
{
    public class UnreliableMultiplexer
    {
        public List<Packet> m_Queue = new List<Packet>();
        public UInt16 m_NextID = 1;

        private ZWriter _writer = new ZWriter();

        public void QueueMessage(byte[] _message)
        {
            Console.WriteLine("Queuing message");

            //A message is limited to 32 fragments (32*1383  = 44,256Ko)
            Debug.Assert(_message.Length <= Packet.MaxMessageSize);

            //If we need to fragment this message
            if(_message.Length > Packet.DataMaxSize)
            {
                int _queuedSize = 0;
                while (_queuedSize < _message.Length)
                {
                    int _fragmentSize = (int)MathF.Min(Packet.DataMaxSize, _message.Length - _queuedSize);

                    Console.WriteLine($"Fragment size: {_fragmentSize}");

                    m_NextID++;
                    Packet _packet = new Packet
                    {
                        header = new Packet.Header
                        {
                            ID = m_NextID,
                            PayloadSize = (UInt16)_fragmentSize,
                            Type = _queuedSize == 0 ? PacketType.FirstFragment : PacketType.Fragment
                        },
                    };

                    _packet.data = new byte[_fragmentSize];

                    Array.Copy(_message, _queuedSize, _packet.data, 0, _fragmentSize);
                    m_Queue.Add(_packet);
                    _queuedSize += _fragmentSize;
                }

                //At the end of the loop, we know that the last created packet is the last fragment
                int _index = m_Queue.Count - 1;
                Packet _pack = m_Queue[_index];
                _pack.header.Type = PacketType.LastFragment;

                Debug.Assert(_queuedSize == _message.Length);
            }
            //This message don't need to be fragmented, add it as an entiere packet
            else
            {
                Packet _packet = new Packet
                {
                    header = new Packet.Header
                    {
                        ID = m_NextID++,
                        PayloadSize = (UInt16)_message.Length,
                        Type = PacketType.Packet
                    },
                    data = _message
                };
                m_Queue.Add(_packet);
            }
        }

        /// <summary>
        /// Serialize packet in a given buffer
        /// </summary>
        /// <param name="_buffer"></param>
        /// <param name="_bufferSize"></param>
        /// <returns></returns>
        public int Serialize(ref byte[] _buffer, int _bufferSize)
        {
            int _currentSerializedSize = 0;

            _writer.Init(0);
            for (int i = 0; i < m_Queue.Count; i++)
            {
                Packet _packet = m_Queue[i];

                if (_packet.header.PayloadSize + _currentSerializedSize > _bufferSize)
                    break;

                _writer.WritePacket(_packet, ref _packet.data);

                //Packet header is 5
                Array.Copy(_writer.Buffer, 0, _buffer, _currentSerializedSize, _writer.Buffer.Length);
                int _packetSize = _packet.header.PayloadSize + Packet.HeaderSize;
                _currentSerializedSize += _packetSize;
                m_Queue.Remove(_packet);
            }

            return _currentSerializedSize;
        }
    }
}