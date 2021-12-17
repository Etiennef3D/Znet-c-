using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Znet.Messages;
using Znet.Serialization;
using Znet.Utils;

namespace ZnetTests.Datagrams
{
    [TestClass]
    public class DatagramHandlerTests
    {
        private static UInt64 MASK_COMPLETE = UInt64.MaxValue;
        private static UInt64 MASK_FIRST_ACKED = Utils.Bit.Right;
        private static UInt64 MASK_FIRST_MISSING = ~MASK_FIRST_ACKED;
        private static UInt64 MASK_LAST_ACKED = (MASK_FIRST_ACKED << 63);
        private static UInt64 MASK_EMPTY = 0;
        private static UInt64 MASK_FIRST_AND_SECOND_ACKED = (MASK_FIRST_ACKED << 1) | Utils.Bit.Right;

        private byte[] GetBuffer(UInt16 headerID, ref DatagramHandler _handler)
        {
            ZWriter _writer = new ZWriter();
            _writer.Init(0);
            
            return _handler.CreateDatagramHeader();
        }

        private byte[] GetHeader(UInt16 _headerValue)
        {
            return BitConverter.GetBytes(_headerValue);
        }

        [TestMethod]
        public void DatagramHandlerInit()
        {

        }
    }
}