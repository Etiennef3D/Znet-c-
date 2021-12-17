using System;
using System.Collections.Generic;

namespace Znet.Utils
{
    /// <summary>
    /// Handle the followings of packets acknowledgement.
    /// </summary>
    public class AckHandler
    {
        public UInt16 LastAck => m_LastAck;
        public UInt64 PreviousAckMask => m_PreviousAcks;
        public List<UInt16> NewAcks => GetNewAcks();
        public List<UInt16> Loss => m_Loss;

        private UInt16 m_LastAck = UInt16.MaxValue;
        private UInt64 m_PreviousAcks = UInt64.MaxValue;
        private UInt64 m_NewAcks = 0;
        private List<UInt16> m_Loss = new List<UInt16>();
        private bool m_LastAckIsNew = false;

        public void Update(UInt16 _newAck, UInt64 _previousAcks, bool _trackLoss = true)
        {
            m_LastAckIsNew = false;

            //Same as last ack, but can contains new informations in its mask
            if (_newAck == m_LastAck)
            {
                //Mark new acks and update masks
                m_NewAcks = (m_PreviousAcks & _previousAcks) ^ _previousAcks;
                m_PreviousAcks |= _previousAcks;
            }
            else if(Utils.IsSequenceNewer(_newAck, m_LastAck))
            {
                int diff = Utils.SequenceDiff(_newAck, m_LastAck);
                int gap = diff - 1;

                int bitsToShift = (int)MathF.Min(diff, 64);

                if(_trackLoss)
                {
                    for(int i = 0; i < bitsToShift; ++i)
                    {
                        int packetDiffWithLastAck = 64 - i;
                        int bitInPreviousMask = packetDiffWithLastAck - 1;

                        if (!Utils.HasBit(ref m_PreviousAcks, (byte)bitInPreviousMask))
                        {
                            UInt16 packetId = (UInt16)(m_LastAck - packetDiffWithLastAck);
                            m_Loss.Add(packetId);
                        }
                    }
                }
                if (bitsToShift >= 63)
                {
                    bitsToShift = 63;
                    Utils.UnsetBit(ref m_PreviousAcks, 0);
                }

                m_PreviousAcks <<= bitsToShift;

                if (gap >= 64)
                {
                    m_PreviousAcks = 0;
                    m_NewAcks = 0;

                    if (_trackLoss)
                    {
                        //IS IT 63 INSTEAD OF 64 ?
                        for(int i = 63; i < gap; ++i)
                        {
                            ushort packetId = (ushort)(m_LastAck + (i - 63) + 1);
                            m_Loss.Add(packetId);
                        }
                    }
                }
                else
                {
                    Utils.SetBit(ref m_PreviousAcks, (byte)gap);
                }

                m_LastAck = _newAck;
                m_LastAckIsNew = true;

                //Mark new acks and update masks
                m_NewAcks = (m_PreviousAcks & _previousAcks) ^ _previousAcks;
                m_PreviousAcks |= _previousAcks;
            }
            else
            {
                //Old ack but can still have useful informations
                int diff = Utils.SequenceDiff(m_LastAck, _newAck);

                if(diff <= 64)
                {
                    //Align received mask with current mask
                    _previousAcks <<= diff - 1;

                    //Insert ack in the mask
                    byte ackBitInMask = (byte)(diff - 1);
                    Utils.SetBit(ref _previousAcks, ackBitInMask);
                    m_NewAcks = (m_PreviousAcks & _previousAcks) ^ _previousAcks;
                    m_PreviousAcks |= _previousAcks;
                    
                }
                else
                {
                    //Older ack than the left outer mask - ignore it.
                }
            }
            //string _prev = Convert.ToString((long)m_PreviousAcks, 2);
            //Console.WriteLine(_prev);
        }

        public bool IsAcked(UInt16 _ack)
        {
            if(_ack == m_LastAck)
            {
                return true;
            }

            if(Utils.IsSequenceNewer(_ack, m_LastAck))
            {
                return false;
            }

            int diff = Utils.SequenceDiff(m_LastAck, _ack);

            if(diff > 64)
            {
                return false;
            }

            byte bitPosition = (byte)(diff-1);
            return Utils.HasBit(ref m_PreviousAcks, bitPosition);
        }

        public bool IsNewlyAcked(UInt16 _ack)
        {
            if(_ack == m_LastAck)
            {
                return m_LastAckIsNew;
            }

            if(Utils.IsSequenceNewer(_ack, m_LastAck))
            {
                return false;
            }

            int diff = Utils.SequenceDiff(m_LastAck, _ack);
            if(diff > 64)
            {
                return false;
            }
            byte bitPosition = (byte)(diff - 1);

            return Utils.HasBit(ref m_NewAcks, bitPosition);
        }

        public List<UInt16> GetNewAcks()
        {
            List<UInt16> newAcks = new List<UInt16>(64);

            for(byte i = 64; i != 0; --i)
            {
                byte bitToCheck = (byte)(i - 1);
                if(Utils.HasBit(ref m_NewAcks, bitToCheck))
                {
                    UInt16 id = (UInt16)(m_LastAck - i);
                    newAcks.Add(id);
                }
            }
            
            if (m_LastAckIsNew)
            {
                newAcks.Add(m_LastAck);
            }

            return newAcks;
        }
    }
}