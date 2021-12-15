using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Znet.Messages;
using Znet.Utils;

namespace Znet.Client
{
    public class Client
    {
        public UInt16 NextDatagramIdToSend => m_NextDatagramIdToSend;
        public AckHandler ReceivedAcks => m_ReceivedAcks;
        public AckHandler SentAcks => m_SentAcks;

        private Socket m_Socket;
        private AckHandler m_ReceivedAcks;
        private AckHandler m_SentAcks;
        private EndPoint m_Address;
        
        // Id of the next datagram to send
        private UInt16 m_NextDatagramIdToSend = 0;

        //private NetworkMessage m_Messages;

        public Client()
        {
            m_ReceivedAcks = new AckHandler();
            m_SentAcks = new AckHandler();
        }

        private void Release()
        {
            Console.WriteLine("Releasing client.");
        }
    
        public void OnDatagramReceived(ref Datagram _datagram)
        {
            UInt16 datagramId = _datagram.header.ID;
            m_ReceivedAcks.Update(datagramId, _datagram.header.previousAck);

            if (!m_ReceivedAcks.IsNewlyAcked(datagramId))
            {
                Console.WriteLine("Datagram is not newly acked");
                return;
            }

            OnDataReceived(_datagram.data);

            m_SentAcks.Update(_datagram.header.ack, _datagram.header.previousAck);

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

        private void OnDataReceived(byte[] _data)
        {
            //onMessageReady(std::make_unique<Messages::UserData>(std::move(data)));
        }

        private void OnMessageReady(/*ref NetworkMessage _message*/)
        {
            //memcpy(&(msg->from), &mAddress, sizeof(mAddress));
            //mClient.onMessageReceived(std::move(msg));
            //m_Messages.Add(_message);
        }

        public bool Start(ushort _port)
        {
            Release();
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //Check if the socket has been created before going here
            m_Address = new IPEndPoint(IPAddress.Any, _port);

            try
            {
                m_Socket.Bind(m_Address);
            }
            catch(Exception _ex)
            {
                Console.WriteLine($"Couldn't bind this socket to the port : {_port}");
            }

            Console.WriteLine("Client is started");


            Thread _thread = new Thread(new ThreadStart(() =>
            {
                Receive();
            }));
            _thread.Start();

            return true;
        }
        public void Send(byte[] data)
        {
            Datagram _datagram = new Datagram();
            _datagram.header.ID = m_NextDatagramIdToSend;
            ++m_NextDatagramIdToSend;

            _datagram.header.ack = m_ReceivedAcks.LastAck;
            _datagram.header.previousAck = m_ReceivedAcks.PreviousAckMask;

            Console.WriteLine($"Client sends datagram with ID: {_datagram.header.ID}");

            Array.Copy(data, _datagram.data, data.Length);

            //Is this socket should take the local end point or the remote end point ?
            IPAddress _ip = IPAddress.Parse("127.0.0.1");

            m_Socket.SendTo(_datagram.data, 0, data.Length + Datagram.HeaderSize, SocketFlags.None, new IPEndPoint(_ip, 12345));
        }

        public void Receive()
        {
            Console.WriteLine("Client receive mode.");
            while (true)
            {
                byte[] _buffer = new byte[Datagram.BUFFER_MAX_SIZE];
                int receive = m_Socket.Receive(_buffer, Datagram.BUFFER_MAX_SIZE, SocketFlags.None);
                
                Console.WriteLine("Client received");
                if(receive > 0)
                {
                    if(receive > Datagram.HeaderSize)
                    {
                        Console.WriteLine($"Receiving data: {receive} octets");
                        //Receive datagram
                        //OnDatagramReceived()
                    }
                    else
                    {
                        //Unexpected datagram
                        Console.WriteLine("Unexpected datagram has been received");
                    }
                }
                else
                {
                    if(receive < 0)
                    {
                        //Error handling
                        Console.WriteLine("Error has occured");
                    }
                    return;
                }
            }
        }
    }
}