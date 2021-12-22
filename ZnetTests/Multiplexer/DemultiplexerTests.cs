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
            UnreliableDemultiplexer _demultiplexer = new UnreliableDemultiplexer();
            Assert.AreEqual(_demultiplexer.m_PendingQueue.Count, 0);
            Assert.AreEqual(_demultiplexer.m_LastProcessed, UInt16.MaxValue);
        }

        [TestMethod]
        public void ReceiveUnfragmentedMessage()
        {
            byte[] _data = new byte[] { 1, 2, 3, 4, 5 };
            byte[] _buffer = new byte[Packet.PacketMaxSize];
            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            _multiplexer.Queue(_data);
            _multiplexer.Serialize(ref _buffer, _buffer.Length);

            UnreliableDemultiplexer _demultiplexer = new UnreliableDemultiplexer();
            _demultiplexer.OnDataReceived(ref _buffer, _data.Length);

            Assert.AreEqual(_demultiplexer.m_PendingQueue.Count, 1);
            Assert.AreEqual(_demultiplexer.m_LastProcessed, 0);
        }

        [TestMethod]
        public void ReceiveFragmentedMessage()
        {
            byte[] _veryBigMessage = new byte[Packet.DataMaxSize * 3];
            byte[] _buffer = new byte[Packet.PacketMaxSize];

            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            _multiplexer.Queue(_veryBigMessage);
            int _length = _multiplexer.Serialize(ref _buffer, Packet.PacketMaxSize);

            Assert.AreEqual(_length, Packet.PacketMaxSize);
            Assert.AreEqual(3, _multiplexer.m_Queue[_multiplexer.m_Queue.Count - 1].header.ID);

            UnreliableDemultiplexer _demultiplexer = new UnreliableDemultiplexer();
            _demultiplexer.OnDataReceived(ref _buffer, _length);

            Assert.AreEqual(1, _demultiplexer.m_LastProcessed);
            Assert.AreEqual(1, _demultiplexer.m_PendingQueue.Count);
        }

        [TestMethod]
        public void ReceivedFragmentedMessageInWrongOrder()
        {
            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            UnreliableDemultiplexer _demultiplexer = new UnreliableDemultiplexer();

            byte[] _data = new byte[Packet.DataMaxSize * 3];
            _multiplexer.Queue(_data);
            
            int _firstMessageLength = _multiplexer.Serialize(ref _data, Packet.PacketMaxSize);
            
            Assert.AreEqual(Packet.PacketMaxSize, _firstMessageLength);

            //Receive message 1
            _demultiplexer.OnDataReceived(ref _data, _data.Length);
            Assert.AreEqual(1, _demultiplexer.m_PendingQueue.Count);

            int _secondMessageLength = _multiplexer.Serialize(ref _data, Packet.PacketMaxSize);
            int _thirdMessageLength = _multiplexer.Serialize(ref _data, Packet.PacketMaxSize);

            //Receive message 3
            _demultiplexer.OnDataReceived(ref _data, _data.Length);

            Assert.AreEqual(2, _demultiplexer.m_PendingQueue.Count);
            Assert.AreEqual(3, _demultiplexer.m_LastProcessed);


            //Receive message 2 -- Should be ignored
            UInt16 _fakeHeaderId = 2;
            byte[] _fakeHeaderBytes = BitConverter.GetBytes(_fakeHeaderId);
            Array.Copy(_fakeHeaderBytes, _data, 2);

            _demultiplexer.OnDataReceived(ref _data, _data.Length);

            //Still the same as before, because UnreliableDemultiplex doesn't handle late packets
            Assert.AreEqual(2, _demultiplexer.m_PendingQueue.Count);
            Assert.AreEqual(3, _demultiplexer.m_LastProcessed);

            //Check order of the list
        }
    }
}