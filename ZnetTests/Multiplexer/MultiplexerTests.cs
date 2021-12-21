using Microsoft.VisualStudio.TestTools.UnitTesting;
using Znet.Messages.Packet;
using Znet.Queue;

namespace ZnetTests.Multiplexer
{
    [TestClass]
    public class MultiplexerTests
    {
        [TestMethod]
        public void QueueMessage()
        {
            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            Assert.IsTrue(_multiplexer.m_Queue.Count == 0);
            Assert.IsTrue(_multiplexer.m_NextID == 1);

            //Queue a buffer of size 5
            byte[] _buffer = new byte[5];
            _multiplexer.Queue(ref _buffer);

            Assert.IsTrue(_multiplexer.m_Queue.Count == 1, $"Multiplexer queue count is : {_multiplexer.m_Queue.Count}");
            Assert.IsTrue(_multiplexer.m_NextID == 2, $"Multiplexer next id: {_multiplexer.m_NextID}");

            //Serialize a buffer of size 1388
            byte[] _bigBuffer = new byte[Packet.PacketMaxSize];
            int _size = _multiplexer.Serialize(ref _bigBuffer, Packet.PacketMaxSize);
            Assert.IsTrue(_size == _buffer.Length, $"Size is {_size}. It should be {_buffer.Length}");
        }

        [TestMethod]
        public void FragmentMessage()
        {
            UnreliableMultiplexer _multiplexer = new UnreliableMultiplexer();
            //Sending a fragmented message (3 fragments)
            byte[] _veryBigMessage = new byte[Packet.DataMaxSize * 3];

            _multiplexer.Queue(ref _veryBigMessage);

            Assert.AreEqual(_multiplexer.m_Queue.Count, 3);
            Assert.AreEqual(_multiplexer.m_NextID, 4);
        }
    }
}