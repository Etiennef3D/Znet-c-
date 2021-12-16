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
            public UInt16 ack;

            //Packet previous acknowledgement
            public UInt64 previousAck;
        }

        public Datagram()
        {
            header = new Header();
            payloadData = new byte[DataMaxSize];
            headerData = new byte[HeaderSize];
        }

        public static int HeaderSize = sizeof(UInt16) * 2 + sizeof(UInt64);
        public static int BUFFER_MAX_SIZE = 1400;
        public static int DataMaxSize = BUFFER_MAX_SIZE - HeaderSize;
        public int dataSize = 0;

        public Header header;
        public byte[] payloadData;
        public byte[] headerData;
    }
}