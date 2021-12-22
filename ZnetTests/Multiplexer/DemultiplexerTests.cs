using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Znet.Messages.Packet;
using Znet.Queue;

namespace ZnetTests.Multiplexer
{
    [TestClass]
    public class DemultiplexerTests
    {
        [TestMethod]
        public void Initialize()
        {
            //UnreliableDemultiplexer _demultiplexer = new UnreliableDemultiplexer();
            //Assert.AreEqual(_demultiplexer.m_PendingQueue.Count, 0);
            //Assert.AreEqual(_demultiplexer.m_LastProcessed, UInt16.MaxValue);
        }

        [TestMethod]
        public void ReceiveData()
        {
            //Packet _packetZero = new Packet
            //{
            //    header = new Packet.Header
            //    {
            //        ID = 0,
            //        PayloadSize = 5,
            //        Type = PacketType.Packet
            //    },
            //    data = new byte[] { 1, 2, 3, 4, 5 }
            //};
           
            //UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            //UnreliableDemultiplexer _demultiplexer = new UnreliableDemultiplexer();

            //Assert.AreEqual(_packetZero.header.ID, 0);
            //Assert.AreEqual(_packetZero.header.Type, PacketType.Packet);

            //_multiplexer.Queue(ref _packetZero.data);
            //_multiplexer.Serialize(ref _packetZero.data, Packet.PacketMaxSize);
            //_demultiplexer.OnDataReceived(ref _packetZero.data, _packetZero.data.Length);

            //Assert.AreEqual(_demultiplexer.m_PendingQueue.Count, 1);


            //Packet _packetOne = new Packet
            //{
            //    header = new Packet.Header
            //    {
            //        ID = 1,
            //        PayloadSize = 6,
            //        Type = PacketType.Packet
            //    },
            //    data = new byte[] { 1, 2, 3, 4, 5, 6}
            //};

            //Assert.AreEqual(_packetOne.header.ID, 1);
            //Assert.AreEqual(_packetOne.header.Type, PacketType.Packet);
            //Assert.AreEqual(_packetOne.header.PayloadSize, 6);

            //_multiplexer.Queue(ref _packetOne.data);
            //_multiplexer.Serialize(ref _packetOne.data, Packet.PacketMaxSize);
            //_demultiplexer.OnDataReceived(ref _packetOne.data, _packetOne.header.PayloadSize);

            //Assert.AreEqual(_demultiplexer.m_PendingQueue.Count, 2);
        }
    }
}