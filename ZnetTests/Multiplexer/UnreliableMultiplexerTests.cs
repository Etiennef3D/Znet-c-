using Microsoft.VisualStudio.TestTools.UnitTesting;
using Znet.Messages.Packet;
using Znet.Multiplexer;

namespace ZnetTests.Multiplexer
{
    [TestClass]
    public class UnreliableMultiplexerTests
    {
        [TestMethod]
        public void QueuingNormalMessages()
        {
            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            byte[] _buffer = new byte[5];
            _multiplexer.Queue(_buffer);

            Assert.AreEqual(_multiplexer.m_Queue.Count, 1);

            byte[] _secondBuffer = new byte[20];
            _multiplexer.Queue(_secondBuffer);

            Assert.AreEqual(_multiplexer.m_Queue.Count, 2);
        }

        [TestMethod]
        public void QueuingBigMessages()
        {
            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            byte[] _veryBigMessage = new byte[Packet.DataMaxSize * 3];

            _multiplexer.Queue(_veryBigMessage);

            Assert.AreEqual(_multiplexer.m_Queue.Count, 3);
            Assert.AreEqual(_multiplexer.m_NextID, 3);
        }

        [TestMethod]
        public void SerializeNormalMessage()
        {
            byte[] _sendBuffer = new byte[1400];
            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            int _dataLength = 5 + Packet.HeaderSize;
            byte[] _payloadBuffer = new byte[5] { 1, 2, 3, 4, 5 };

            _multiplexer.Queue(_payloadBuffer);

            int _serializedDataSize = _multiplexer.Serialize(ref _sendBuffer, _dataLength);
            Assert.AreEqual(_serializedDataSize, _dataLength);
        }

        [TestMethod]
        public void SerializeBigMessages()
        {
            byte[] _sendBuffer = new byte[Packet.PacketMaxSize];
            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            byte[] _veryBigMessage = new byte[Packet.DataMaxSize * 3];
            _multiplexer.Queue(_veryBigMessage);

            Assert.AreEqual(_multiplexer.m_Queue.Count, 3);
            Assert.AreEqual(_multiplexer.m_NextID, 3);

            //Make sure when we have 3 fragments, that the serialize method only return 1 message with a fragment
            int _serializedDataSize = _multiplexer.Serialize(ref _sendBuffer, _sendBuffer.Length);
            Assert.AreEqual(_serializedDataSize, Packet.PacketMaxSize);

            //If we serialize again, we should have the exact same packet size
            int _serializedDataSize2 = _multiplexer.Serialize(ref _sendBuffer, _sendBuffer.Length);
            Assert.AreEqual(_serializedDataSize2, Packet.PacketMaxSize);

            //And again the same size
            int _serializedDataSize3 = _multiplexer.Serialize(ref _sendBuffer, _sendBuffer.Length);
            Assert.AreEqual(_serializedDataSize3, Packet.PacketMaxSize);

            //Now we should have an empty list
            Assert.AreEqual(_multiplexer.m_Queue.Count, 0);
            Assert.AreEqual(_multiplexer.m_NextID, 3);
        }

        [TestMethod]
        public void SendingTooBigMessage()
        {
            byte[] _tooBigMessage = new byte[Packet.DataMaxSize * (Packet.MaxFragmentsPerMessage + 1)];

            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            _multiplexer.Queue(_tooBigMessage);

            Assert.AreEqual(0, _multiplexer.m_Queue.Count);
        }

        [TestMethod]
        public void SendingMaximumSizeMessage()
        {
            byte[] _maxSizeMessage = new byte[Packet.MaxMessageSize];
            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            _multiplexer.Queue(_maxSizeMessage);
            Assert.AreEqual(32, _multiplexer.m_Queue.Count);
        }

        [TestMethod]
        public void SendingMinimumSizeMessage()
        {
            byte[] _minSizeMessage = new byte[0];
            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            _multiplexer.Queue(_minSizeMessage);
            Assert.AreEqual(1, _multiplexer.m_Queue.Count);
        }
    }
}