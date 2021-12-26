using System;
using System.Collections.Generic;

namespace Znet.Messages.Packet
{
    public class ReliablePacket
    {
        public Packet Packet => m_Packet;

        private Packet m_Packet;
        private List<UInt16> m_DatagramsIncluding = new List<UInt16>();

        public ReliablePacket(Packet _packet)
        {
            m_Packet = _packet;
        }
        public void OnSent(UInt16 _datagramID)
        {
            m_DatagramsIncluding.Add(_datagramID);
        }

        public bool IsIncludedIn(UInt16 _datagramID)
        {
            return m_DatagramsIncluding.Contains(_datagramID);
        }
    }
}
