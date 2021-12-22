using System;
using System.Text;
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

            byte[] _veryBigMessage = new byte[Packet.DataMaxSize * 3];

            Console.WriteLine("Big message incoming of length: " + _veryBigMessage.Length);

            byte[] _buffer = new byte[Packet.PacketMaxSize];

            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            _multiplexer.Queue(_veryBigMessage);
            
            //Serialize message one
            int _length = _multiplexer.Serialize(ref _buffer, _buffer.Length);

            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine("Packet to binary");
            StringBuilder _builder = new StringBuilder();
            for (int i = 0; i < _buffer.Length; i++)
            {
                _builder.Append(_buffer[i]);
            }

            Console.WriteLine($"{_builder}");
            Console.WriteLine("-------------------------------------------------------------------");

            UnreliableDemultiplexer _demultiplexer = new UnreliableDemultiplexer();
            _demultiplexer.OnDataReceived(ref _buffer, _length);

            //It works ! :)




            //Serialize message two
            //int _length2 = _multiplexer.Serialize(ref _buffer, _buffer.Length);

            //Serialize message three
            //int _length3 = _multiplexer.Serialize(ref _buffer, _buffer.Length);
        }
    }
}