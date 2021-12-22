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

		/// <summary>
		/// Extract packets from the buffer
		/// </summary>
		/// <param name="_buffer"></param>
		/// <param name="_dataSize"></param>
		public void OnDataReceived(ref byte[] _buffer, int _dataSize)
        {
			Console.WriteLine($"Data received. Buffer length: {_buffer.Length}, data size: {_dataSize}");
			int _processedDataSize = 0;

			//<! Extraire les paquets du tampon
			while(_processedDataSize < _dataSize)
            {
				_reader.Init(_buffer, _processedDataSize);

				Packet _packet = new Packet
				{
					header = new Packet.Header
					{
						ID = _reader.ReadUInt16(),
						PayloadSize = _reader.ReadUInt16(),
						Type = (PacketType) _reader.ReadByte()
					},
				};

				int _size = _packet.header.PayloadSize + Packet.HeaderSize;
				_packet.data = new byte[_size];

				//Form the received packet from the buffer
				Array.Copy(_buffer, _packet.data, _size);

				//Read header, if not good, discard
				if (_processedDataSize + _packet.header.PayloadSize > _dataSize || _packet.data.Length > Packet.DataMaxSize)
					return;

				OnPacketReceived(ref _packet);
				_processedDataSize += _packet.header.PayloadSize + _dataSize;
				Console.WriteLine($"Processed: {_processedDataSize} - Data size: {_dataSize}");
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
				Console.WriteLine("ADD PACKET");
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