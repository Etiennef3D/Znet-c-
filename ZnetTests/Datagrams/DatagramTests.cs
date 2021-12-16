using Microsoft.VisualStudio.TestTools.UnitTesting;
using Znet.Messages;

namespace ZnetTests.Datagrams
{
    [TestClass]
    public class DatagramTests
    {
        [TestMethod]
        public void Datagram_Initialization()
        {
            Datagram datagram = new Datagram();
            Assert.IsTrue(Datagram.HeaderSize == 12);
        }
    }
}
