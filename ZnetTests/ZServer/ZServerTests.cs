using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using Znet.Connections;

namespace ZnetTests.ZServer
{
    [TestClass]
    public class ZServerTests
    {
        [TestMethod]
        public void Server_Start()
        {
            Znet.Server.ZServer _server = new Znet.Server.ZServer();
            _server.Start(50002);
            Assert.IsTrue(_server.IsActive);

            _server.Stop();
            Assert.IsFalse(_server.IsActive);
        }

        [TestMethod]
        public void Server_Receive_Message()
        {
            Znet.Server.ZServer _server = new Znet.Server.ZServer();
            _server.Start(50005);

            Assert.IsTrue(_server.IsActive);

            Znet.Client.ZClient client = new Znet.Client.ZClient();
            client.Start(50005, 12555);
            client.Connect();

            Thread.Sleep(1);
            Assert.IsTrue(_server.DatagramHandler.ReceivedDatagrams == 1, $"Received datagram count is not 1: {_server.DatagramHandler.ReceivedDatagrams}");
        }

        [TestMethod]
        public void Server_Accept_Client()
        {
            Znet.Server.ZServer _server = new Znet.Server.ZServer();
            _server.Start(50004);

            Assert.IsTrue(_server.IsActive);

            Znet.Client.ZClient client = new Znet.Client.ZClient();
            client.Start(50004, 12300);
            client.Connect();

            ZConnection _conn = new ZConnection();
            _server.connectionManager.AddConnection(_conn);

            Assert.IsTrue(_server.connectionManager.ConnectionCount == 1, $"Not 1: {_server.connectionManager.ConnectionCount}");
            Assert.IsTrue(_server.connectionManager.GetConnectionByID(1) != null);

            Assert.IsTrue(_server.DatagramHandler.ReceivedDatagrams == 1);
        }
    }
}