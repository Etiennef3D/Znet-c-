using System;
using System.Collections.Generic;
using System.Diagnostics;
using Znet.Messages.Packet;
using Znet.Serialization;

namespace Znet.Queue
{
    public class UnreliableMultiplexer
    {
        public List<Packet> m_Queue = new List<Packet>();
        public UInt16 m_NextID = 1;

        private ZWriter _writer = new ZWriter();

        public void Queue(ref byte[] _msgData)
        {
            Console.WriteLine($"Message length: {_msgData.Length}");
            //Mark this message as first fragment or fragment of a message
            Debug.Assert(_msgData.Length <= Packet.MaxMessageSize);
            if(_msgData.Length > Packet.DataMaxSize)
            {
                int _queuedSize = 0;
                while (_queuedSize < _msgData.Length)
                {
                    int _fragmentSize = (int)MathF.Min(Packet.DataMaxSize, _msgData.Length - _queuedSize);

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

                    Array.Copy(_msgData, _queuedSize, _packet.data, 0, _fragmentSize);
                    m_Queue.Add(_packet);
                    _queuedSize += _fragmentSize;
                }

                int _index = m_Queue.Count - 1;
                Packet _pack = m_Queue[_index];
                _pack.header.Type = PacketType.LastFragment;
                Debug.Assert(_queuedSize == _msgData.Length);
            }
            else
            {
                //Add this message to the sending list waiting for this message to be sent
                Packet _packet = new Packet
                {
                    header = new Packet.Header
                    {
                        ID = m_NextID++,
                        Type = PacketType.Packet,
                        PayloadSize = (UInt16)_msgData.Length
                    },
                    data = new byte[_msgData.Length]
                };
                
                Array.Copy(_packet.data, _msgData, _msgData.Length);
                m_Queue.Add(_packet);
            }
            Console.WriteLine("End queue");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_buffer"></param>
        /// <param name="_bufferSize"></param>
        /// <returns></returns>
        public int Serialize(ref byte[] _buffer, int _bufferSize)
        {
            int _currentSerializedSize = 0;

            for(int i = 0; i < m_Queue.Count; i++)
            {
                Packet _packet = m_Queue[i];

                if (_packet.header.PayloadSize + _currentSerializedSize > _bufferSize)
                    break;

                _writer.WritePacket(_packet, ref _packet.data);
                Array.Copy(_writer.Buffer, 0, _buffer, _currentSerializedSize, _writer.Buffer.Length);
                _currentSerializedSize += _packet.header.PayloadSize;
                m_Queue.Remove(_packet);
            }

            return _currentSerializedSize;
        }
    }
}