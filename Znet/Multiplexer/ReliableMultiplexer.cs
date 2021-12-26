using System;
using System.Collections.Generic;
using System.Diagnostics;
using Znet.Messages.Packet;

namespace Znet.Multiplexer
{
    public class ReliableMultiplexer : IMultiplexer
    {
        public List<ReliablePacket> m_Queue = new List<ReliablePacket>();
        private UInt16 m_NextID = 0;

        public void Queue(byte[] _message)
        {
            Console.WriteLine("Queuing message");

            if (_message.Length > Packet.MaxMessageSize)
            {
                //Not handled, message is too big
                return;
            }
            Debug.Assert(_message.Length <= Packet.MaxMessageSize);
            int _queuedSize = 0;
            if (_message.Length > Packet.DataMaxSize)
            {
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

                    m_Queue.Add(new ReliablePacket(_packet));
                    _queuedSize += _fragmentSize;
                }
                Debug.Assert(_queuedSize == _message.Length);
            }
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
                m_Queue.Add(new ReliablePacket(_packet));
            }
        }

        public int Serialize(ref byte[] _buffer, int _bufferSize)
        {
            return 0;
        }

        public void OnDatagramAcked(UInt16 _datagramID)
        {

        }

        public void OnDatagramLost(UInt16 _datagramID)
        {

        }
    }
}