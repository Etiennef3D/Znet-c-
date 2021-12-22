using System;
using System.Collections.Generic;
using Znet.Messages.Packet;
using Znet.Serialization;

namespace Znet.Queue
{
    public class UnreliableDemultiplexer
    {
		public List<Packet> m_PendingQueue = new List<Packet>();
		public UInt16 m_LastProcessed = UInt16.MaxValue;

		private ZReader _reader = new ZReader();

		public void OnDataReceived(ref byte[] _buffer, int _dataSize)
        {
			Console.WriteLine($"{nameof(OnDataReceived)} - Data size: {_dataSize} ");

			int _processedDataSize = 0;

			while(_processedDataSize < _dataSize)
            {
				_reader.Init(_buffer, _processedDataSize);

				UInt16 _packetID = _reader.ReadUInt16();
				PacketType _packetType = (PacketType)_reader.ReadByte();
				UInt16 _payloadSize = _reader.ReadUInt16();

				//Extract packet from buffer
				Packet _packet = new Packet
				{
					header = new Packet.Header
					{
						ID = _packetID,
						Type = _packetType,
						PayloadSize = _payloadSize
					},

					data = new byte[_payloadSize]
				};

				Console.WriteLine($"Processing packet {_packet}");

				//Set the payload data in the packet
				Array.Copy(_buffer, _processedDataSize + Packet.HeaderSize, _packet.data, 0, _packet.header.PayloadSize);

				//Prevent malformed packet
                if (_processedDataSize > _dataSize || _packet.data.Length > Packet.DataMaxSize)
                {
                    return;
                }

				OnPacketReceived(ref _packet);
				_processedDataSize += _dataSize;
			}
		}

        private void OnPacketReceived(ref Packet _packet)
        {
			Console.WriteLine($"Received packet: Header ID: {_packet.header.ID} - {m_LastProcessed}");
			if (!Utils.Utils.IsSequenceNewer(_packet.header.ID, m_LastProcessed))
            {
				Console.WriteLine("Sequence isn't newer. Return");
				return;
            }

			if(m_PendingQueue.Count == 0 || Utils.Utils.IsSequenceNewer(_packet.header.ID, m_PendingQueue[m_LastProcessed].header.ID))
            {
				Console.WriteLine($"Add packet {_packet.header.ID} to the queue");
				m_PendingQueue.Add(_packet);
            }
            else
            {
				int _index = 0;

				//<! Trouver le premier itérateur avec un identifiant égal à ou plus récent que notre paquet, nous devons placer le paquet avant celui-ci
				for(int i = 0; i < m_PendingQueue.Count; i++)
                {
					if(m_PendingQueue[i].header.ID != _packet.header.ID)
                    {
						_index = i;
						m_PendingQueue.Insert(_index, _packet);
						break;
					}
                }
			}
		
			m_LastProcessed = _packet.header.ID;
		}
	}
}