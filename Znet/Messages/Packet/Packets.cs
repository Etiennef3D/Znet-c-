using System;

namespace Znet.Messages.Packet
{
    public struct Packet
    {
        public struct Header
        {
            public UInt16 ID;
            public UInt16 PayloadSize;
            public PacketType Type;
        }

        public static int PacketMaxSize = Datagram.DataMaxSize;                     // 1388
	    public static int HeaderSize = 5;                                           // (UInt*2 + byte = 5)
	    public static int DataMaxSize = PacketMaxSize - HeaderSize;                 // 1388-5 = 1383
	    public static int MaxFragmentsPerMessage = 32;                              // 32 fragments max per message
	    public static int MaxMessageSize = MaxFragmentsPerMessage * DataMaxSize;    // ~42Ko

        public Header header;
        public byte[] data;

        public override string ToString()
        {
            return $"Packet ID: {header.ID}, Type: {header.Type}, PayloadSize {header.PayloadSize}";
        }
    }

    public enum PacketType
    {
        Packet = 0,
        FirstFragment = 1,
        Fragment = 2,
        LastFragment = 3
    }
}