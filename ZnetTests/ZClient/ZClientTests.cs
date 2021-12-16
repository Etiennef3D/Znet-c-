using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;
using Znet.Client;
using Znet.Messages;
using Znet.Utils;
using static Znet.Messages.SystemMessages;

namespace ZnetTests.ZClient
{
    [TestClass]
    public class ZClientTests
    {
        private static UInt64 MASK_COMPLETE = UInt64.MaxValue;
        private static UInt64 MASK_FIRST_ACKED = Utils.Bit.Right;
        private static UInt64 MASK_FIRST_MISSING = ~MASK_FIRST_ACKED;
        private static UInt64 MASK_LAST_ACKED = (MASK_FIRST_ACKED << 63);
        private static UInt64 MASK_EMPTY = 0;
        private static UInt64 MASK_FIRST_AND_SECOND_ACKED = (MASK_FIRST_ACKED << 1) | Utils.Bit.Right;

        [TestMethod]
        public void ClientInit()
        {
            Znet.Client.ZClient _client = new Znet.Client.ZClient();
            _client.Start(12300, 12345);

            Assert.IsTrue(_client.NextDatagramIdToSend == 0);
            Assert.IsTrue(_client.ReceivedAcks.LastAck == UInt16.MaxValue);

            string testData = "Coucou";
            byte[] _data = Encoding.UTF8.GetBytes(testData);

            WelcomeMessage _message = new WelcomeMessage();
            _message.welcomeMessageValue = 14;
            _client.Send(_message);
            Assert.IsTrue(_client.NextDatagramIdToSend == 1);

            //Check if the datagram has been received
            Datagram receivedDatagram = new Datagram();
            receivedDatagram.header.ID = 0;

            Array.Copy(_data, receivedDatagram.payloadData, _data.Length);

            _client.OnDatagramReceived(ref receivedDatagram);

            Assert.IsTrue(_client.ReceivedAcks.LastAck == 0);
            Assert.IsTrue(_client.ReceivedAcks.PreviousAckMask == MASK_COMPLETE);

            /*
                auto polledMessages = client.poll();
		        CHECK(polledMessages.size() == 1);
		        const auto& msg = polledMessages[0];
		        CHECK(msg->is<Bousk::Network::Messages::UserData>());
		        const auto dataMsg = msg->as<Bousk::Network::Messages::UserData>();
		        CHECK(dataMsg->data.size() == TestStringLength);
		        CHECK(memcmp(TestString, dataMsg->data.data(), TestStringLength) == 0);
            */

            // fake sending another datagram: It should be ignored
            receivedDatagram.header.ID = 0;
            {
                _client.OnDatagramReceived(ref receivedDatagram);
                Assert.IsTrue(_client.ReceivedAcks.LastAck == 0);
                Assert.IsTrue(_client.ReceivedAcks.PreviousAckMask == MASK_COMPLETE);
                /*
                 * auto polledMessages = client.poll();
		            CHECK(polledMessages.size() == 0);
                 */
            }

            //Jump the ID 1 datagram
            receivedDatagram.header.ID = 2;
            {
                _client.OnDatagramReceived(ref receivedDatagram);
                Assert.IsTrue(_client.ReceivedAcks.LastAck == 2);
                Assert.IsTrue(_client.ReceivedAcks.PreviousAckMask == MASK_FIRST_MISSING);

                /*
                    auto polledMessages = client.poll();
		            CHECK(polledMessages.size() == 1);
		            const auto& msg = polledMessages[0];
		            CHECK(msg->is<Bousk::Network::Messages::UserData>());
		            const auto dataMsg = msg->as<Bousk::Network::Messages::UserData>();
		            CHECK(dataMsg->data.size() == TestStringLength);
		            CHECK(memcmp(TestString, dataMsg->data.data(), TestStringLength) == 0);
                 */
            }

            // The 1 finally comes to the client
            receivedDatagram.header.ID = 1;
            {
                _client.OnDatagramReceived(ref receivedDatagram);
                Assert.IsTrue(_client.ReceivedAcks.LastAck == 2);
                Assert.IsTrue(_client.ReceivedAcks.IsNewlyAcked(1));
                Assert.IsFalse(_client.ReceivedAcks.IsNewlyAcked(2));
                Assert.IsTrue(_client.ReceivedAcks.PreviousAckMask == MASK_COMPLETE);

                /*
                    auto polledMessages = client.poll();
		            CHECK(polledMessages.size() == 1);
		            const auto& msg = polledMessages[0];
		            CHECK(msg->is<Bousk::Network::Messages::UserData>());
		            const auto dataMsg = msg->as<Bousk::Network::Messages::UserData>();
		            CHECK(dataMsg->data.size() == TestStringLength);
		            CHECK(memcmp(TestString, dataMsg->data.data(), TestStringLength) == 0);
                */
            }

            //Jump of 64 datagrams
            receivedDatagram.header.ID = 66;
            {
                _client.OnDatagramReceived(ref receivedDatagram);
                Assert.IsTrue(_client.ReceivedAcks.LastAck == 66);
                Assert.IsTrue(_client.ReceivedAcks.PreviousAckMask == MASK_LAST_ACKED);
                Assert.IsTrue(_client.ReceivedAcks.Loss.Count == 0);

                /*
		            auto polledMessages = client.poll();
		            CHECK(polledMessages.size() == 1);
		            const auto& msg = polledMessages[0];
		            CHECK(msg->is<Bousk::Network::Messages::UserData>());
		            const auto dataMsg = msg->as<Bousk::Network::Messages::UserData>();
		            CHECK(dataMsg->data.size() == TestStringLength);
		            CHECK(memcmp(TestString, dataMsg->data.data(), TestStringLength) == 0);
                 */
            }

            //Receive one datagram after
            receivedDatagram.header.ID = 67;
            {
                _client.OnDatagramReceived(ref receivedDatagram);
                Assert.IsTrue(_client.ReceivedAcks.LastAck == 67);
                Assert.IsTrue(_client.ReceivedAcks.IsNewlyAcked(67));
                Assert.IsTrue(_client.ReceivedAcks.PreviousAckMask == MASK_FIRST_ACKED);
                Assert.IsTrue(_client.ReceivedAcks.Loss.Count == 0);
                /*
		            auto polledMessages = client.poll();
		            CHECK(polledMessages.size() == 1);
		            const auto& msg = polledMessages[0];
		            CHECK(msg->is<Bousk::Network::Messages::UserData>());
		            const auto dataMsg = msg->as<Bousk::Network::Messages::UserData>();
		            CHECK(dataMsg->data.size() == TestStringLength);
		            CHECK(memcmp(TestString, dataMsg->data.data(), TestStringLength) == 0);
                */
            }

            receivedDatagram.header.ID = 68;
            {
                _client.OnDatagramReceived(ref receivedDatagram);
                Assert.IsTrue(_client.ReceivedAcks.LastAck == 68);
                Assert.IsTrue(_client.ReceivedAcks.IsNewlyAcked(68));
                Assert.IsTrue(_client.ReceivedAcks.PreviousAckMask == MASK_FIRST_AND_SECOND_ACKED);

                /*
		            auto polledMessages = client.poll();
		            CHECK(polledMessages.size() == 1);
		            const auto& msg = polledMessages[0];
		            CHECK(msg->is<Bousk::Network::Messages::UserData>());
		            const auto dataMsg = msg->as<Bousk::Network::Messages::UserData>();
		            CHECK(dataMsg->data.size() == TestStringLength);
		            CHECK(memcmp(TestString, dataMsg->data.data(), TestStringLength) == 0);
                 */
            }

            //Too old msg, ignore
            receivedDatagram.header.ID = 3;
            {
                _client.OnDatagramReceived(ref receivedDatagram);
                Assert.IsTrue(_client.ReceivedAcks.LastAck == 68);
                Assert.IsFalse(_client.ReceivedAcks.IsNewlyAcked(68));
                Assert.IsTrue(_client.ReceivedAcks.PreviousAckMask == MASK_FIRST_AND_SECOND_ACKED);

                /*datagram.header.id = htons(3);
                {
                    auto polledMessages = client.poll();
                    CHECK(polledMessages.size() == 0);
                }*/
            }

        }
    }
}
