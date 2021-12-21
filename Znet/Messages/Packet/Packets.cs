using System;

namespace Znet.Messages.Packet
{
    struct Packet
    {
        struct Header
        {
            UInt16 ID;
            UInt16 Size;
            PacketType Type;
        }

        static int PacketMaxSize = Datagram.DataMaxSize; 
	    static int HeaderSize = 5;
	    static int DataMaxSize = PacketMaxSize - HeaderSize; 
	    static int MaxPacketsPerMessage = 32;
	    static int MaxMessageSize = MaxPacketsPerMessage* DataMaxSize;

        Header header;
        byte[] data;
    }

    public enum PacketType
    {
        Packet,
        FirstFragment,
        Fragment,
        LastFragment
    }
}