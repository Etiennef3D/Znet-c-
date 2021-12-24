using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Znet.Utils;

namespace ZnetTests
{
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void Utils_Class_Tests()
        {
            Assert.IsTrue(Utils.IsSequenceNewer(1, 0));
            Assert.IsFalse(Utils.IsSequenceNewer(0, 1));
            Assert.IsFalse(Utils.IsSequenceNewer(0, 0));
            Assert.IsTrue(Utils.IsSequenceNewer(0, UInt16.MaxValue));

            Assert.IsTrue(Utils.SequenceDiff(0, 0) == 0);
            Assert.IsTrue(Utils.SequenceDiff(1, 0) == 1);
            Assert.IsTrue(Utils.SequenceDiff(0, UInt16.MaxValue) == 1);

            UInt64 bitfield = 0;
            Assert.IsTrue(bitfield == 0);

            Utils.SetBit(ref bitfield, 0);
            Assert.IsTrue(Utils.HasBit(ref bitfield, 0));
            Assert.IsTrue(bitfield == Utils.Bit.Right);
            Utils.UnsetBit(ref bitfield, 0);
            Assert.IsTrue(bitfield == 0);

            Utils.SetBit(ref bitfield, 5);
            Assert.IsTrue(Utils.HasBit(ref bitfield, 5));
            Assert.IsTrue(bitfield == Utils.Bit.Right << 5);

            //	Bousk::Utils::UnsetBit(bitfield, 0);
            Utils.UnsetBit(ref bitfield, 0);
            Assert.IsTrue(bitfield == Utils.Bit.Right << 5);

            Utils.UnsetBit(ref bitfield, 5);
            Assert.IsTrue(bitfield == 0);
        }

        private static UInt64 MASK_COMPLETE = UInt64.MaxValue;
        private static UInt64 MASK_FIRST_ACKED = Utils.Bit.Right;
        private static UInt64 MASK_FIRST_MISSING = ~MASK_FIRST_ACKED;
        private static UInt64 MASK_LAST_ACKED = (MASK_FIRST_ACKED << 63);
        private static UInt64 MASK_EMPTY = 0;
        private static UInt64 MASK_FIRST_AND_SECOND_ACKED = (MASK_FIRST_ACKED << 1) | Utils.Bit.Right;

        /*
         * ACK HANDLING
         */
        [TestMethod]
        public void Ack_Handler_Tests()
        {
            AckHandler ackhandler = new AckHandler();
            Assert.IsTrue(ackhandler.LastAck == UInt16.MaxValue);
            Assert.IsTrue(ackhandler.PreviousAckMask == MASK_COMPLETE);
            Assert.IsFalse(ackhandler.IsAcked(0));
            Assert.IsFalse(ackhandler.IsNewlyAcked(0));
            Assert.IsTrue(ackhandler.Loss.Count == 0);

            //Reception of ID 0
            ackhandler.Update(0, MASK_COMPLETE);
            Assert.IsTrue(ackhandler.LastAck == 0);
            Assert.IsTrue(ackhandler.PreviousAckMask == MASK_COMPLETE);
            Assert.IsTrue(ackhandler.IsAcked(0));
            Assert.IsTrue(ackhandler.IsNewlyAcked(0));
            Assert.IsTrue(ackhandler.NewAcks.Count == 1);
            Assert.IsTrue(ackhandler.NewAcks[0] == 0);
            Assert.IsTrue(ackhandler.Loss.Count == 0);

            //Reception of ID 2 with ID 1 missing (63th bit at 0)
            ackhandler.Update(2, MASK_FIRST_MISSING);
            Assert.IsTrue(ackhandler.LastAck == 2);
            Assert.IsTrue(ackhandler.PreviousAckMask == MASK_FIRST_MISSING);
            Assert.IsTrue(ackhandler.IsAcked(2));
            Assert.IsTrue(ackhandler.IsAcked(0));
            Assert.IsTrue(ackhandler.IsNewlyAcked(2));
            Assert.IsFalse(ackhandler.IsNewlyAcked(0));
            Assert.IsTrue(ackhandler.Loss.Count == 0);

            //Reception of ID 1
            ackhandler.Update(1, MASK_COMPLETE);
            Assert.IsTrue(ackhandler.LastAck == 2);
            Assert.IsTrue(ackhandler.PreviousAckMask == MASK_COMPLETE);
            Assert.IsTrue(ackhandler.IsAcked(1));
            Assert.IsTrue(ackhandler.IsAcked(2));
            Assert.IsTrue(ackhandler.IsAcked(0));
            Assert.IsTrue(ackhandler.IsNewlyAcked(1));
            Assert.IsFalse(ackhandler.IsNewlyAcked(2));
            Assert.IsFalse(ackhandler.IsNewlyAcked(0));
            Assert.IsTrue(ackhandler.Loss.Count == 0);

            //Reception of ID 66 with empty mask
            ackhandler.Update(66, MASK_EMPTY);
            Assert.IsTrue(ackhandler.LastAck == 66);
            Assert.IsTrue(ackhandler.IsNewlyAcked(66));
            Assert.IsTrue(ackhandler.PreviousAckMask == MASK_LAST_ACKED);
            Assert.IsTrue(ackhandler.Loss.Count == 0);

            //Reception of ID 67 with empty mask
            ackhandler.Update(67, MASK_EMPTY);
            Assert.IsTrue(ackhandler.LastAck == 67);
            Assert.IsTrue(ackhandler.IsNewlyAcked(67));
            Assert.IsFalse(ackhandler.IsNewlyAcked(66));
            Assert.IsTrue(ackhandler.PreviousAckMask == MASK_FIRST_ACKED);
            Assert.IsTrue(ackhandler.Loss.Count == 0);

            // Reception of ID68 with complete mask
            ackhandler.Update(68, MASK_COMPLETE);
            Assert.IsTrue(ackhandler.LastAck == 68);
            Assert.IsTrue(ackhandler.IsNewlyAcked(68));
            Assert.IsTrue(ackhandler.PreviousAckMask == MASK_COMPLETE);

            Assert.IsTrue(ackhandler.Loss.Count == 1);
            Assert.IsTrue(ackhandler.Loss[0] == 3);
            
            for (UInt16 i = 4; i < 66; ++i)
            {
                Assert.IsTrue(ackhandler.IsNewlyAcked(i));
            }

            ackhandler.Update(0, MASK_EMPTY);
            Assert.IsTrue(ackhandler.LastAck == 68);
            Assert.IsFalse(ackhandler.IsNewlyAcked(68));
            Assert.IsTrue(ackhandler.PreviousAckMask == MASK_COMPLETE);

            ackhandler.Update(133, MASK_EMPTY);
            Assert.IsTrue(ackhandler.LastAck == 133);
            Assert.IsTrue(ackhandler.PreviousAckMask == MASK_EMPTY);
            Assert.IsTrue(ackhandler.Loss.Count == 2);
            Assert.IsTrue(ackhandler.Loss[1] == 69);                        //Should be 0

            ackhandler.Update(132, MASK_COMPLETE);
            Assert.IsTrue(ackhandler.LastAck == 133);
            Assert.IsTrue(ackhandler.PreviousAckMask == MASK_COMPLETE);
            Assert.IsTrue(ackhandler.Loss.Count == 2);                      // Should not return true

            ackhandler.Loss.Clear();                                        //Should be cleard automatically ???????????

            //Jump of 100 identifiers (Program should be waiting 134 at this point)
            ackhandler.Update(234, MASK_EMPTY);
            Assert.IsTrue(ackhandler.LastAck == 234);
            Assert.IsTrue(ackhandler.PreviousAckMask == 0);

            int firstLost = 134;
            int lastLost = 169;

            //Should be 35 losts
            int totalLost = lastLost - firstLost + 1;
            
            Assert.IsTrue(ackhandler.Loss.Count-1 == totalLost);
            for(int i = 0; i < totalLost; ++i)
            {
                Assert.IsTrue(ackhandler.Loss[i] == firstLost + i);
            }
            ackhandler.Update(234, MASK_COMPLETE);
            ackhandler.Update(236, MASK_COMPLETE);

            ackhandler.Loss.Clear();                                        //Should be cleard automatically ???????????

            //Jump of 65 ID with empty mask
            ackhandler.Update(301, MASK_EMPTY);
            Assert.IsTrue(ackhandler.LastAck == 301);
            Assert.IsTrue(ackhandler.PreviousAckMask == 0);
            Assert.IsTrue(ackhandler.Loss.Count == 1);

            //Ack of ID 237
            Assert.IsFalse(ackhandler.IsAcked(237));
            ackhandler.Update(237, MASK_COMPLETE);
            Assert.IsTrue(ackhandler.LastAck == 301);
            Assert.IsTrue(ackhandler.Loss.Count == 1);
            Assert.IsTrue(ackhandler.PreviousAckMask == MASK_LAST_ACKED);
            Assert.IsTrue(ackhandler.IsAcked(237));
            Assert.IsTrue(ackhandler.IsNewlyAcked(237));

            ackhandler.Update(301, MASK_COMPLETE);
            Assert.IsTrue(ackhandler.LastAck == 301);
            Assert.IsTrue(ackhandler.PreviousAckMask == MASK_COMPLETE);
            Assert.IsTrue(ackhandler.Loss.Count == 1);

            ackhandler.Update(303, MASK_COMPLETE);
            List<UInt16> newAcks = ackhandler.GetNewAcks();

            Assert.IsTrue(newAcks.Count == 2);
            Assert.IsTrue(newAcks[0] == 302);
            Assert.IsTrue(newAcks[1] == 303);
        }


        [TestMethod]
        public void AckHandlerLongRun()
        {
            AckHandler aHandler = new AckHandler();
            for(UInt16 i = 0; i < 500; i++)
            {
                aHandler.Update(i, MASK_COMPLETE);
                Assert.IsTrue(aHandler.IsAcked(i));
                Assert.IsTrue(aHandler.IsNewlyAcked(i));
            }
            Assert.IsTrue(aHandler.Loss.Count == 0);
        }
    }
}