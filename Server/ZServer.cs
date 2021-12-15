using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Znet.Server
{
    /// <summary>
    /// Main server class. 
    /// Creates a socket, bin it to a port and listen for any message coming.
    /// </summary>
    public class ZServer
    {
        public volatile bool isActive = false;

        private int m_Port;
        private const int MAX_BUFFER_SIZE = 4096;
        private Socket m_Socket;

        public void Start(int _port)
        {
            m_Port = _port;
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, m_Port);
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            m_Socket.Bind(ipep);

            Listen();
        }

        public void Listen()
        {
            Console.WriteLine("Server listening...");

            byte[] _buffer = new byte[MAX_BUFFER_SIZE];
            isActive = true;
            
            Thread udpThread = new Thread(new ThreadStart( () => {
                while (isActive)
                {
                    int res = m_Socket.Receive(_buffer);
                    Console.WriteLine("Received something from sender : " + res);

                    if (res <= 0)
                    {
                        Stop();
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Received more than 0 bytes of data");
                    }
                }
            }));

            udpThread.Start();
        }

        public void Stop()
        {
            Console.WriteLine("Server stopped.");
            isActive = false;
        }
    }
}