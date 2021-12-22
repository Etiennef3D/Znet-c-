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
            byte[] _buffer = new byte[21];


            Packet _packetZero = new Packet
            {
                header = new Packet.Header
                {
                    ID = 0,
                    PayloadSize = 5,
                    Type = PacketType.Packet
                },
                data = new byte[] { 1, 2, 3, 4, 5 }
            };

            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            UnreliableDemultiplexer _demultiplexer = new UnreliableDemultiplexer();

            _multiplexer.Queue(ref _packetZero.data);
            _multiplexer.Serialize(ref _buffer, _buffer.Length);
            _demultiplexer.OnDataReceived(ref _packetZero.data, _packetZero.data.Length);

            Packet _packetOne = new Packet
            {
                header = new Packet.Header
                {
                    ID = 1,
                    PayloadSize = 6,
                    Type = PacketType.Packet
                },
                data = new byte[] { 1, 2, 3, 4, 5, 6 }
            };

            _multiplexer.Queue(ref _packetOne.data);
            _multiplexer.Serialize(ref _buffer, _buffer.Length);
            _demultiplexer.OnDataReceived(ref _buffer, _packetOne.header.PayloadSize);

            Console.WriteLine(_demultiplexer.m_PendingQueue.Count);
        }
    }
}