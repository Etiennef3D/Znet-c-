using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Znet.Messages.Packet;
using Znet.Multiplexer;

namespace ZnetTests.Multiplexer
{
    [TestClass]
    public class UnreliableDemultiplexerTests
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

            _multiplexer.Serialize(ref _data, Packet.PacketMaxSize);
            _multiplexer.Serialize(ref _data, Packet.PacketMaxSize);

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
        }

        [TestMethod]
        public void ReceivedMaximumSizeMessage()
        {
            byte[] _buffer = new byte[Packet.PacketMaxSize];            // 1388 o
            byte[] _maxSizeMessage = new byte[Packet.MaxMessageSize];   // ~42  ko

            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            UnreliableDemultiplexer _demultiplexer = new UnreliableDemultiplexer();

            _multiplexer.Queue(_maxSizeMessage);
            Assert.AreEqual(32, _multiplexer.m_Queue.Count);

            //We should have the exact same packet 32 times
            for(int i = 0; i < 32; i++)
            {
                int _dataLength = _multiplexer.Serialize(ref _buffer, Packet.PacketMaxSize);
                Assert.AreEqual(Packet.PacketMaxSize, _dataLength);

                _demultiplexer.OnDataReceived(ref _buffer, _dataLength);
                Assert.AreEqual(i+1, _demultiplexer.m_PendingQueue.Count);
            }
        }

        [TestMethod]
        public void ReceiveMinimumSizeMessage()
        {
            byte[] _buffer = new byte[Packet.PacketMaxSize];
            byte[] _minSizeMessage = new byte[0];
            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            UnreliableDemultiplexer _demultiplexer = new UnreliableDemultiplexer();

            _multiplexer.Queue(_minSizeMessage);
            _multiplexer.Serialize(ref _buffer, Packet.PacketMaxSize);
            _demultiplexer.OnDataReceived(ref _buffer, _buffer.Length);

            Assert.AreEqual(1, _demultiplexer.m_PendingQueue.Count);
        }

        [TestMethod]
        public void ProcessSimpleMessage()
        {
            byte[] _buffer = new byte[Packet.PacketMaxSize];
            byte[] _minSizeMessage = new byte[0];
            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            UnreliableDemultiplexer _demultiplexer = new UnreliableDemultiplexer();

            _multiplexer.Queue(_minSizeMessage);
            _multiplexer.Serialize(ref _buffer, Packet.PacketMaxSize);
            _demultiplexer.OnDataReceived(ref _buffer, _buffer.Length);
            _demultiplexer.Process();

            Assert.AreEqual(0, _demultiplexer.m_PendingQueue.Count);
        }

        [TestMethod]
        public void ProcessExistingFragmentedMessage()
        {
            byte[] _buffer = new byte[Packet.PacketMaxSize];
            byte[] _minSizeMessage = new byte[Packet.DataMaxSize * 3];
            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            UnreliableDemultiplexer _demultiplexer = new UnreliableDemultiplexer();

            _multiplexer.Queue(_minSizeMessage);

            //Receive 1/3 message
            int _length = _multiplexer.Serialize(ref _buffer, Packet.PacketMaxSize);
            _demultiplexer.OnDataReceived(ref _buffer, _length);

            //Receive 2/3 message
            _length = _multiplexer.Serialize(ref _buffer, Packet.PacketMaxSize);
            _demultiplexer.OnDataReceived(ref _buffer, _length);

            //Receive 3/3 message
            _length = _multiplexer.Serialize(ref _buffer, Packet.PacketMaxSize);
            _demultiplexer.OnDataReceived(ref _buffer, _length);

            Assert.AreEqual(3, _demultiplexer.m_PendingQueue.Count);

            //Assemble messages and clear the queue
            int _byteLength = _demultiplexer.Process().Length;

            Assert.AreEqual(_byteLength, Packet.PacketMaxSize * 3);

            //Now the list should be empty
            Assert.AreEqual(0, _demultiplexer.m_PendingQueue.Count);
        }

        [TestMethod]
        public void ProcessNonExistingFragmentedMessage()
        {
            byte[] _buffer = new byte[Packet.PacketMaxSize];
            byte[] _minSizeMessage = new byte[Packet.DataMaxSize * 3];
            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            UnreliableDemultiplexer _demultiplexer = new UnreliableDemultiplexer();

            _multiplexer.Queue(_minSizeMessage);

            //Receive 1/3 message
            int _length = _multiplexer.Serialize(ref _buffer, Packet.PacketMaxSize);
            _demultiplexer.OnDataReceived(ref _buffer, _length);

            //Receive 2/3 message
            _length = _multiplexer.Serialize(ref _buffer, Packet.PacketMaxSize);
            _demultiplexer.OnDataReceived(ref _buffer, _length);

            //Process should keep messages in the queue
            Assert.AreEqual(2, _demultiplexer.m_PendingQueue.Count);

            _demultiplexer.Process();
            Assert.AreEqual(2, _demultiplexer.m_PendingQueue.Count);
        }

        [TestMethod]
        public void ProcessWithMissingFragmentedMessage()
        {
            byte[] _buffer = new byte[Packet.PacketMaxSize];
            byte[] _minSizeMessage = new byte[Packet.DataMaxSize * 3];
            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            UnreliableDemultiplexer _demultiplexer = new UnreliableDemultiplexer();

            _multiplexer.Queue(_minSizeMessage);

            //Receive 1/3 message
            int _length = _multiplexer.Serialize(ref _buffer, Packet.PacketMaxSize);
            _demultiplexer.OnDataReceived(ref _buffer, _length);

            //Receive 3/3 message
            _multiplexer.Serialize(ref _buffer, Packet.PacketMaxSize);
            _length = _multiplexer.Serialize(ref _buffer, Packet.PacketMaxSize);
            _demultiplexer.OnDataReceived(ref _buffer, _length);

            //Process should keep messages in the queue
            Assert.AreEqual(2, _demultiplexer.m_PendingQueue.Count);

            _demultiplexer.Process();

            Assert.AreEqual(2, _demultiplexer.m_PendingQueue.Count);
        }
    }
}