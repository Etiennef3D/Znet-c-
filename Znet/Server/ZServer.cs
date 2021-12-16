using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Znet.Connections;
using Znet.Messages;
using Znet.Queue;
using Znet.Serialization;
using Znet.Utils;
using static Znet.Messages.SystemMessages;

namespace Znet.Server
{
    /// <summary>
    /// Main server class. 
    /// Creates a socket, bin it to a port and listen for any message coming.
    /// </summary>
    public class ZServer
    {
        public volatile bool isActive = false;

        public ZConnectionManager connectionManager;

        private byte[] _buffer;
        private int m_Port;
        private const int MAX_BUFFER_SIZE = 4096;
        private Socket m_Socket;
        private DatagramHandler m_DatagramHandler;
        private IMessagePacker m_MessagePacker;
        private IQueue<Datagram> m_ReceivingQueue;

        public ZServer()
        {
            m_MessagePacker = new MessagePacker();
            m_DatagramHandler = new DatagramHandler();
            connectionManager = new ZConnectionManager();
            m_ReceivingQueue = new DatagramReceivingQueue();

            _buffer = new byte[MAX_BUFFER_SIZE];
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            Thread _receivingQueueThread = new Thread(() =>
            {
                m_ReceivingQueue.Start();
            });
            _receivingQueueThread.Start();
        }


        public void Start(int _port)
        {
            m_Port = _port;

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, m_Port);

            m_Socket.Bind(ipep);
            Listen();
        }

        public void Listen()
        {
            Console.WriteLine("Server listening...");
            isActive = true;
            
            Thread udpThread = new Thread(new ThreadStart( () => {
                while (isActive)
                {
                    int res = m_Socket.Receive(_buffer);
                    Console.WriteLine($"SERVER Received packet of size {res}");

                    if (res > 0)
                    {
                        //StringBuilder _builder = new StringBuilder();
                        //for(int i = 0; i < res; i++)
                        //{
                        //    _builder.Append(_buffer[i]);
                        //    _builder.Append("|");
                        //}
                        //Console.WriteLine(_builder);

                        ZReader _reader = new ZReader();
                        _reader.Init(_buffer);

                        if(m_DatagramHandler.OnDatagramReceived(ref _buffer, ref _reader, out Datagram _datagram))
                        {
                            m_ReceivingQueue.AddToTheQueue(_datagram);
                        }
                        
                        WelcomeMessage _welcome = new WelcomeMessage
                        {
                            welcomeMessageValue = 4
                        };

                        Send(_welcome);
                    }
                    else
                    {
                        Stop();
                        break;
                    }
                }
            }));

            udpThread.Start();
        }

        public void Send<T>(T _message) where T : ZNetworkMessage
        {
            Console.WriteLine("SERVER send");

            ZWriter _writer = new ZWriter();
            UInt16 _packetLength = 0;

            m_MessagePacker.PackMessage(ref _writer, _message, out _packetLength);
            m_DatagramHandler.CreateDatagramHeader(ref _writer);
            byte[] _dat = _writer.Buffer;
            //_datagram.header.ID = m_NextDatagramIdToSend;
            //++m_NextDatagramIdToSend;

            //_datagram.header.ack = m_ReceivedAcks.LastAck;
            //_datagram.header.previousAck = m_ReceivedAcks.PreviousAckMask;
            IPAddress _ip = IPAddress.Parse("127.0.0.1");
            m_Socket.SendTo(_dat, 0, _packetLength, SocketFlags.None, new IPEndPoint(_ip, m_Port));

            //m_Socket.SendTo(_datagram.payloadData, 0, _data.Length + Datagram.HeaderSize, SocketFlags.None, new IPEndPoint(_ip, 12345));

        }

        public void Stop()
        {
            Console.WriteLine("Server stopped.");
            isActive = false;
        }
    }
}