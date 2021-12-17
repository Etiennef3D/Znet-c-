using System;

namespace Znet.Messages
{
    /// <summary>
    /// Represents a message with a header and a payload
    /// </summary>
    public class Datagram
    {
        /// <summary>
        /// Represents the header of a packet.
        /// This is the way we set reliability to datagrams.
        /// Header size is 12 bytes (2+2+8)
        /// </summary>
        public struct Header
        {
            //Packet unique ID
            public UInt16 ID;

            //Packet current acknowledgement
            public UInt16 newAck;

            //Packet previous acknowledgement
            public UInt64 previousAck;
        }

        public Datagram()
        {
            header = new Header
            {
                ID = UInt16.MaxValue,
                newAck = UInt16.MaxValue,
                previousAck = UInt64.MaxValue
            };

            payloadData = new byte[DataMaxSize];
            headerData = new byte[HeaderSize];
        }

        public static readonly int HeaderSize = 12;
        public static readonly int BUFFER_MAX_SIZE = 1400;
        public static readonly int DataMaxSize = BUFFER_MAX_SIZE - HeaderSize;
        public int dataSize;

        public Header header;
        public byte[] payloadData;
        public byte[] headerData;
    }
}