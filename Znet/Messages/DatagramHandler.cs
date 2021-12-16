using System;
using System.Text;
using Znet.Serialization;
using Znet.Utils;

namespace Znet.Messages
{
    public class DatagramHandler
    {
        public UInt16 NextDatagramIdToSend => m_NextDatagramIdToSend;
        public AckHandler ReceivedAcks => m_ReceivedAcks;
        public AckHandler SentAcks => m_SentAcks;


        private AckHandler m_ReceivedAcks;
        private AckHandler m_SentAcks;
        private UInt16 m_NextDatagramIdToSend = 0;

        public DatagramHandler()
        {
            m_ReceivedAcks = new AckHandler();
            m_SentAcks = new AckHandler();
        }

        public bool OnDatagramReceived(ref byte[] _buffer, ref ZReader _reader, out Datagram _datagram)
        {
            _datagram = new Datagram();

            _datagram.header.ID = _reader.ReadUInt16();
            _datagram.header.ack = _reader.ReadUInt16();
            _datagram.header.previousAck = _reader.ReadUInt64();

            Array.Copy(_buffer, _datagram.headerData, Datagram.HeaderSize);

            m_ReceivedAcks.Update(_datagram.header.ID, _datagram.header.previousAck);

            if (!m_ReceivedAcks.IsNewlyAcked(_datagram.header.ID))
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

        private void OnMessageReady(/*ref NetworkMessage _message*/)
        {
            //memcpy(&(msg->from), &mAddress, sizeof(mAddress));
            //mClient.onMessageReceived(std::move(msg));
            //m_Messages.Add(_message);
        }

        public void CreateDatagramHeader(ref ZWriter _writer)
        {
            _writer.WriteHeader(m_NextDatagramIdToSend, m_ReceivedAcks.LastAck, m_ReceivedAcks.PreviousAckMask);
            ++m_NextDatagramIdToSend;
        }
    }
}