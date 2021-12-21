using System;
using System.Collections.Generic;
using Znet.Messages.Packet;

namespace Znet.Queue
{
    public class UnreliableDemultiplexer
    {

		private List<Packet> m_PendingQueue = new List<Packet>();
		private UInt16 m_LastProcessed = 0;

		/// <summary>
		/// Extract packets from the buffer
		/// </summary>
		/// <param name="_buffer"></param>
		/// <param name="_dataSize"></param>
		public void OnDataReceived(ref byte[] _buffer, int _dataSize)
        {
			int _processedDataSize = 0;

			//while(_processedDataSize < _dataSize)
   //         {
			//	//TODO: Get packet from buffer
			//	Packet _packet = (Packet)_buffer;
			//	if(_processedDataSize + _packet.header.Size > _dataSize || _packet.header.Size > Packet.DataMaxSize)
   //             {
			//		return;
   //             }
			//	OnPacketReceived(ref _packet);
			//	_processedDataSize += _packet.header.Size;

			//	//Increase pointer position ?
			//	_buffer += _packet.header.Size;
   //         }
        }

        private void OnPacketReceived(ref Packet _packet)
        {
			if (!Utils.Utils.IsSequenceNewer(_packet.header.ID, m_LastProcessed))
				return;

			if(m_PendingQueue.Count == 0 ||Utils.Utils.IsSequenceNewer(_packet.header.ID, m_PendingQueue[m_LastProcessed].header.ID))
            {
				m_PendingQueue.Add(_packet);
            }
            else
            {
				int _index = 0;

				//<! Trouver le premier itérateur avec un identifiant égal à ou plus récent que notre paquet, nous devons placer le paquet avant celui-ci
				for(int i = 0; i < m_PendingQueue.Count; i++)
                {
					if(m_PendingQueue[i].header.ID >= _packet.header.ID)
                    {
						_index = i;
						m_PendingQueue.Insert(_index, _packet);
						break;
					}
                }
			}
		}
	}
}