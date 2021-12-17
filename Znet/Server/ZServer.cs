using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Znet.Connections;
using Znet.Messages;
using Znet.Queue;

namespace Znet.Server
{
    /// <summary>
    /// Main server class. 
    /// Creates a socket, bin it to a port and listen for any message coming.
    /// </summary>
    public class ZServer
    {
        public bool IsActive { get => isActive; set => isActive = value; }
        public DatagramHandler DatagramHandler => m_DatagramHandler;

        public ZConnectionManager connectionManager;

        private volatile bool isActive;
        private byte[] _buffer;
        private int m_Port;
        private const int MAX_BUFFER_SIZE = 4096;

        private readonly Socket m_Socket;
        private readonly DatagramHandler m_DatagramHandler;
        private readonly IMessagePacker m_MessagePacker;
        private readonly IQueue<Datagram> m_ReceivingQueue;

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
            IsActive = true;
            
            Thread udpThread = new Thread(new ThreadStart( () => {
                while (IsActive)
                {
                    int res = m_Socket.Receive(_buffer);
                    Console.WriteLine($"SERVER Received packet of size {res}");

                    if (res > 0)
                    {
                        if (m_DatagramHandler.OnDatagramReceived(ref _buffer))
                        {
                            Console.WriteLine("Send datagram to receiving queue");
                        }
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
            byte[] _header = m_DatagramHandler.CreateDatagramHeader();

            IPAddress _ip = IPAddress.Parse("127.0.0.1");
            m_Socket.SendTo(_header, 0, Datagram.HeaderSize, SocketFlags.None, new IPEndPoint(_ip, m_Port));
        }

        public void Stop()
        {
            Console.WriteLine("Server stopped.");
            IsActive = false;
        }
    }
}