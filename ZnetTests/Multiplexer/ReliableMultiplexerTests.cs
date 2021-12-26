using Microsoft.VisualStudio.TestTools.UnitTesting;
using Znet.Messages.Packet;
using Znet.Multiplexer;

namespace ZnetTests.Multiplexer
{
    [TestClass]
    public class ReliableMultiplexerTests
    {
        [TestMethod]
        public void Initialize()
        {
            ReliableMultiplexer _multiplexer = new ReliableMultiplexer();
            Assert.AreEqual(0, _multiplexer.m_Queue.Count);
        }

        [TestMethod]
        public void QueueSimplePacket()
        {
            ReliableMultiplexer _multiplexer = new ReliableMultiplexer();
            byte[] _data = new byte[100];
            _multiplexer.Queue(_data);

            Assert.AreEqual(1, _multiplexer.m_Queue.Count);
        }

        [TestMethod]
        public void QueueFragmentedPacket()
        {
            ReliableMultiplexer _multiplexer = new ReliableMultiplexer();
            byte[] _data = new byte[Packet.DataMaxSize * 3];
            _multiplexer.Queue(_data);

            Assert.AreEqual(3, _multiplexer.m_Queue.Count);
        }
    }
}
