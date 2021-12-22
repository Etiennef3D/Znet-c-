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
        public UInt16 m_NextID = 0;

        private ZWriter _writer = new ZWriter();

        public void Queue(byte[] _message)
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

                    if ((_queuedSize + _fragmentSize) >= _message.Length)
                    {
                        _packet.header.Type = PacketType.LastFragment;
                    }

                    m_Queue.Add(_packet);
                    _queuedSize += _fragmentSize;
                }
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
            Console.WriteLine($"Multiplexer serialization. Messages to process in the queue: {m_Queue.Count}");
            int _currentSerializedSize = 0;

            List<Packet> _packetList = new List<Packet>();

            //Fill the copied list
            for(int i = 0; i < m_Queue.Count; i++)
            {
                _packetList.Add(m_Queue[i]);
            }

            _writer.Init(0);

            //Iterate through a copied list instead of m_Queue
            foreach (Packet _packet in _packetList)
            {
                if(_packet.header.PayloadSize + Packet.HeaderSize > _bufferSize)
                {
                    _currentSerializedSize = - 1;
                    break;
                }
                if(_currentSerializedSize >= _bufferSize || _packet.header.PayloadSize >= _bufferSize)
                {
                    Console.WriteLine("Buffer not large enough to serialize or serialization ended.");
                    break;
                }

                Console.WriteLine($"Serializing packet: {_packet}");

                //Write packet header in the buffer
                _writer.WritePacket(_packet, ref _buffer);

                //Write packet data in the buffer
                _currentSerializedSize += (_packet.header.PayloadSize + Packet.HeaderSize);

                m_Queue.Remove(_packet);
                Console.WriteLine($"Removing packet {_packet}. Remaining: {m_Queue.Count}");
            }
            return _currentSerializedSize;
        }
    }
}