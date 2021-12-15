using System;
using System.Diagnostics;

namespace Znet.Utils
{
    public static class Utils
    {
        public static void SetBit(ref UInt64 bitfield, byte n)
        {
            Debug.Assert(n < 64);
            bitfield |= (Bit.Right << n);
        }

        public static void UnsetBit(ref UInt64 bitfield, byte n)
        {
            Debug.Assert(n < 64);
            bitfield &= (~Bit.Right << n);
        }

        public static bool HasBit(ref UInt64 bitfield, byte n)
        {
            Debug.Assert(n < 64);
            return (bitfield & (Bit.Right << n)) != 0;
        }

        public struct Bit
        {
            public const UInt64 Right =
                0b0000000000000000000000000000000000000000000000000000000000000001;
        }

        public static bool IsSequenceNewer(UInt16 sNew, UInt16 sLast)
        {
            if (sNew == sLast)
            {
                return false;
            }
            return (sNew > sLast && sNew - sLast <= UInt16.MaxValue / 2)
                || (sNew < sLast && sLast - sNew > UInt16.MaxValue / 2);
        }

        public static int SequenceDiff(UInt16 sNew, UInt16 sLast)
        {
            if (sNew == sLast)
            {
                return 0;
            }

            Debug.Assert(IsSequenceNewer(sNew, sLast));
            if (sNew > sLast && sNew - sLast <= UInt16.MaxValue / 2)
            {
                return (UInt16)(sNew - sLast);
            }

            return UInt16.MaxValue - sLast + sNew + 1;
        }
    }
}