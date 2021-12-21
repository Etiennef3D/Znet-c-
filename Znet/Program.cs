using System;
using Znet.Messages.Packet;
using Znet.Queue;

namespace Znet
{
    public class Program
    {
        static void Main(string[] args)
        {
            //Znet.Server.ZServer _server = new Znet.Server.ZServer();
            //_server.Start(50004);

            //Znet.Client.ZClient client = new Znet.Client.ZClient();
            //client.Start(50004, 12300);
            //client.Connect();
            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            //Sending a fragmented message (3 fragments)
            byte[] _veryBigMessage = new byte[Packet.DataMaxSize * 3];

            _multiplexer.Queue(ref _veryBigMessage);
            Console.WriteLine(_multiplexer.m_Queue.Count); //Should be 3
            Console.WriteLine(_multiplexer.m_NextID); //Should be 4
        }
    }
}