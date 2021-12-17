using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Znet.Messages;
using Znet.Serialization;
using static Znet.Messages.SystemMessages;

namespace Znet.Client
{
    public class ZClient
    {
        //Handle the socket.
        private Socket m_Socket;
        private EndPoint m_Address;

        private ushort m_ListeningPort;
        private ushort m_SendingPort;

        private MessageHandler m_MessageHandler;
        private DatagramHandler m_DatagramHandler;
        private IMessagePacker m_MessagePacker;

        /// <summary>
        /// Sends a datagram to the server to be registered on the server.
        /// </summary>
        public void Connect()
        {
            WelcomeMessage _welcome = new WelcomeMessage
            {
                welcomeMessageValue = 4
            };

            Send(_welcome);
        }

        //private NetworkMessage m_Messages;

        public ZClient()
        {
            m_MessagePacker = new MessagePacker();
            m_MessageHandler = new MessageHandler();
            m_DatagramHandler = new DatagramHandler();
        }

        public bool Start(ushort _sendingPort, ushort _listeningPort)
        {
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            m_ListeningPort = _listeningPort;
            m_SendingPort = _sendingPort;

            //Check if the socket has been created before going here
            m_Address = new IPEndPoint(IPAddress.Any, m_ListeningPort);

            try
            {
                m_Socket.Bind(m_Address);
            }
            catch(Exception _ex)
            {
                Console.WriteLine($"Couldn't bind this socket to the port : {_sendingPort} : {_ex}");
            }

            Thread _thread = new Thread(new ThreadStart(() =>
            {
                Receive();
            }));
            _thread.Start();

            return true;
        }

        public void Send<T>(T _message) where T : ZNetworkMessage
        {
            Console.WriteLine("CLIENT send");
            
            byte[] _header = m_DatagramHandler.CreateDatagramHeader();

            IPAddress _ip = IPAddress.Parse("127.0.0.1");
            m_Socket.SendTo(_header, 0, Datagram.HeaderSize, SocketFlags.None, new IPEndPoint(_ip, m_SendingPort));
        }

        public void Receive()
        {
            //Console.WriteLine("Client receive mode.");
            while (true)
            {
                byte[] _buffer = new byte[Datagram.BUFFER_MAX_SIZE];
                int receive = m_Socket.Receive(_buffer);
                
                Console.WriteLine($"CLIENT received a message of size {receive}");
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