using System;
using System.Collections.Generic;
using System.Text;
using Znet.Messages.Packet;
using Znet.Serialization;

namespace Znet.Multiplexer
{
    public class UnreliableDemultiplexer
    {
		public List<Packet> m_PendingQueue = new List<Packet>();
		public UInt16 m_LastProcessed = UInt16.MaxValue;

		private ZReader _reader = new ZReader();

		/// <summary>
		/// Called when data is received.
		/// Extract packets from the buffer and adds them to a list.
		/// Handles malformed packets.
		/// </summary>
		/// <param name="_buffer"></param>
		/// <param name="_dataSize"></param>
		public void OnDataReceived(ref byte[] _buffer, int _dataSize)
        {
			Console.WriteLine($"{nameof(OnDataReceived)} - Data size: {_dataSize} ");
			
			int _processedDataSize = 0;

			while(_processedDataSize < _dataSize)
            {
				_reader.Init(_buffer, _processedDataSize);

				StringBuilder _builder = new StringBuilder();
				for(int i = 0; i < Packet.HeaderSize; i++)
                {
					_builder.Append(_buffer[i]);
                }

				Console.WriteLine($"{_builder}");

				UInt16 _packetID = _reader.ReadUInt16();
				PacketType _packetType = (PacketType)_reader.ReadByte();
				UInt16 _payloadSize = _reader.ReadUInt16();
				Console.WriteLine($"Read packet type: {_packetType}");
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
                if (_packet.data.Length > Packet.DataMaxSize)
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

			if(m_PendingQueue.Count == 0 
				|| (m_PendingQueue.Count > 0 && Utils.Utils.IsSequenceNewer(_packet.header.ID, m_LastProcessed)))
            {
				Console.WriteLine($"Add packet {_packet.header.ID} to the queue");
				m_PendingQueue.Add(_packet);
            }
			m_LastProcessed = _packet.header.ID;
		}

		/// <summary>
		/// Get complete messages from the last call.
		/// Process returns the received message
		/// </summary>
		public byte[] Process()
        {
			byte[] _messagesReady = new byte[Packet.MaxMessageSize];
			bool _hasProcessed = false;
			UInt16 _lastHeaderID = 0;
			ZWriter _writer = new ZWriter();
			List<Packet> _packetToRemove = new List<Packet>();
			int _bufferTotalSize = 0;
			for(int i = 0; i < m_PendingQueue.Count; i++)
            {
				Packet _packet = m_PendingQueue[i];

				if(_packet.header.Type == PacketType.Packet)
                {
					Console.WriteLine("Packet detected type: Packet");
					//Copy data to the messageReady
					_hasProcessed = true;
					_writer.WriteBytesInBuffer(_packet.data, ref _messagesReady);
					_lastHeaderID = _packet.header.ID;
					_packetToRemove.Add(_packet);
					_bufferTotalSize = _packet.header.PayloadSize + Packet.HeaderSize;
				}
				else if(_packet.header.Type == PacketType.FirstFragment)
                {
					Console.WriteLine($"Packet detected type: {PacketType.FirstFragment}");
					//Fragmented message case
					//Find the other messages in the list, otherwise skip this message

					List<Packet> _assembledMessage = new List<Packet>();
					bool _isMessageComplete = false;
					//Find corresponding message suite
					for (int j = i; j < m_PendingQueue.Count; j++)
                    {
						if(m_PendingQueue[j].header.ID == _packet.header.ID + j)
                        {
							_assembledMessage.Add(m_PendingQueue[j]);
							if (m_PendingQueue[j].header.Type == PacketType.LastFragment)
                            {
								_isMessageComplete = true;
								break;
							}
                        }
                    }

                    if (_isMessageComplete)
                    {
						Console.WriteLine($"Message complete found in the queue. Assembling message.");

						//Assemble message
						_hasProcessed = true;

						for(int k = 0; k < _assembledMessage.Count; k++)
						{
							_writer.WriteBytesInBuffer(_assembledMessage[k].data, ref _messagesReady);
							_lastHeaderID = _assembledMessage[k].header.ID;
							_packetToRemove.Add(_assembledMessage[k]);
							_bufferTotalSize += _assembledMessage[k].header.PayloadSize + Packet.HeaderSize;
						}
					}
                }
                else
                {
					//Fragment we can't handle now, skip
                }
            }

			//Update internal state
			if(_hasProcessed)
            {
				Console.WriteLine($"Process ended. Packets to remove from the queue: {_packetToRemove.Count}");
				//Update last handled packet to refuse older next time
				m_LastProcessed = _lastHeaderID;

				//Remove the ready packets from the pending queue
				for(int i = 0; i < _packetToRemove.Count; i++)
                {
					m_PendingQueue.Remove(_packetToRemove[i]);
                }
            }
			
			byte[] _returnedMessage = new byte[_bufferTotalSize];
			Array.Copy(_messagesReady, _returnedMessage, _bufferTotalSize);
			Console.WriteLine($"Message total size: {_bufferTotalSize}");
			return _returnedMessage;
        }
	}
}