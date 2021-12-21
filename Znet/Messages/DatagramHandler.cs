using System;
using Znet.Serialization;
using Znet.Utils;

namespace Znet.Messages
{
    public class DatagramHandler
    {
        //public UInt16 NextDatagramIdToSend => m_NextDatagramIdToSend;
        //public AckHandler ReceivedAcks => m_ReceivedAcks;
        public int ReceivedDatagrams => m_ReceivedDatagrams;

        private readonly AckHandler m_ackHandler;
        private UInt16 m_NextDatagramIdToSend = 0;
        private int m_ReceivedDatagrams;

        public DatagramHandler()
        {
            m_ackHandler = new AckHandler();
        }

        public bool OnDatagramReceived(ref byte[] _buffer)
        {
            m_ReceivedDatagrams++;

            byte[] _idBuffer = new byte[2];
            byte[] _maskBuffer = new byte[2];

            Array.Copy(_buffer, _idBuffer, 2);
            Array.Copy(_buffer, 2, _maskBuffer, 0, 2);

            UInt16 _id = BitConverter.ToUInt16(_idBuffer);
            UInt16 previousAck = BitConverter.ToUInt16(_maskBuffer);

            m_ackHandler.Update(_id, previousAck);

            if (!m_ackHandler.IsNewlyAcked(_id))
            {
                Console.WriteLine("ERROR: Datagram is not newly acked");
                return false;
            }

            return true;








            //OnDataReceived(_datagram.payloadData);

            //m_SentAcks.Update(_datagram.header.ack, _datagram.header.previousAck);

            //////////////////////////////////////////////////////////////

            //Handle datagram loss
            /*List<UInt16> lostDatagrams = m_SentAcks.Loss;
            
            for(int i = 0; i < lostDatagrams.Count; i++)
            {
                //OnDatagramReceivedLost(lostDatagrams[i]);
            }

            List<UInt16> datagramSentLost = m_SentAcks.Loss;

            for (int i = 0; i < datagramSentLost.Count; i++)
            {
                //OnDatagramSentLost(datagramSentLost[i]);
            }

            List<UInt16> datagramSentAcked = m_SentAcks.GetNewAcks();

            for(int i = 0; i < datagramSentAcked.Count; i++)
            {
                //OndatagramSentAcked(datagramSentAcked[i]);
            }*/
        }

        public byte[] CreateDatagramHeader()
        {
            byte[] _datagramHeader = new byte[Datagram.HeaderSize];

            byte[] _nextDatagramToSend = BitConverter.GetBytes(m_NextDatagramIdToSend);
            byte[] LastAck = BitConverter.GetBytes(m_ackHandler.LastAck);
            byte[] PreviousAckMask = BitConverter.GetBytes(m_ackHandler.PreviousAckMask);

            Array.Copy(_nextDatagramToSend, _datagramHeader, 2);
            Array.Copy(LastAck, 0, _datagramHeader, 2, 2);
            Array.Copy(PreviousAckMask, 0, _datagramHeader, 4, 8);

            //Place the header at the head of the message
            ++m_NextDatagramIdToSend;

            Console.WriteLine($"DATAGRAM: Create datagram header of size {_datagramHeader.Length}");

            return _datagramHeader;
        }
    }
}