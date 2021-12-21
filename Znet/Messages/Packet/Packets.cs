using System;

namespace Znet.Messages.Packet
{
    public struct Packet
    {
        public struct Header
        {
            public UInt16 ID;
            public UInt16 Size;
            public PacketType Type;
        }

        public static int PacketMaxSize = Datagram.DataMaxSize; 
	    public static int HeaderSize = 5;
	    public static int DataMaxSize = PacketMaxSize - HeaderSize;             //1388-5 = 1383
	    public static int MaxPacketsPerMessage = 32;
	    public static int MaxMessageSize = MaxPacketsPerMessage* DataMaxSize;

        public Header header;
        public byte[] data;
    }

    public enum PacketType
    {
        Packet,
        FirstFragment,
        Fragment,
        LastFragment
    }
}