using System.Linq;
using Xunit;

namespace PikeMQ.Core.Test
{
    public class FrameExtractorTests
    {
        [Fact]
        public void TryExtract_YieldsTrue_ForWellformedFrame()
        {
            FrameExtractor fex = new FrameExtractor();

            var data = new byte[] { 0x02 /*STX*/,
                                    0x01 /* 1 Byte Payload */,
                                    0x01 /* Connection attempt */,
                                    // We have no additional data.
                                    0x03 /*ETX*/ };

            Assert.True(fex.TryExtract(data).success);

        }

        [Fact]
        public void TryExtract_YieldsFalse_IfETXIsMissing()
        {
            FrameExtractor fex = new FrameExtractor();

            var data = new byte[] { 0x02 /*STX*/,
                                    0x01 /* 1 Byte Payload */,
                                    0x01 /* Connection attempt */,
                                    // We have no additional data.
                                    };

            Assert.False(fex.TryExtract(data).success);
        }

        [Fact]
        public void TryExtract_YieldsFalse_IfLengthIsWrong()
        {
            FrameExtractor fex = new FrameExtractor();

            var data = new byte[] { 0x02 /*STX*/,
                                    0x11 /* 1 Byte Payload */,
                                    0x01 /* Connection attempt */,
                                    // We have no additional data.
                                    0x03 /*ETX*/ };

            Assert.False(fex.TryExtract(data).success);
        }

        [Fact]
        public void TryExtract_YieldsFalse_IfSTXIsMissing()
        {
            FrameExtractor fex = new FrameExtractor();

            var data = new byte[] { 0x01 /* 1 Byte Payload */,
                                    0x01 /* Connection attempt */,
                                    // We have no additional data.
                                    0x03 /*ETX*/ };

            Assert.False(fex.TryExtract(data).success);
        }

        [Fact]
        public void TryExtract_YieldsTrue_ForWellFormedMultiBytePacket()
        {
            FrameExtractor fex = new FrameExtractor();

            var data = new byte[] { 0x02 /*STX*/,
                                    0x81 /* Payload > 127 Byte, but upper bit is 1 */,
                                    0x01 /* Payload > 128+1 Byte, but upper bit is 0*/,
                                    0x01 /* Connection attempt */,
                                    0x01, 0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,
                                    0x01, 0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01, // 32
                                    0x01, 0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,
                                    0x01, 0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01, // 64
                                    0x01, 0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,
                                    0x01, 0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01, // 96
                                    0x01, 0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,
                                    0x01, 0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01, // 128
                                    0x03 /*ETX*/ };

            Assert.True(fex.TryExtract(data).success);            
        }

        [Fact]
        public void TryExtract_ExtractsCorrectAmountOfData_ForMultiBytePacket()
        {
            FrameExtractor fex = new FrameExtractor();

            var data = new byte[] { 0x02 /*STX*/,
                                    0x81 /* Payload > 127 Byte, but upper bit is 1 */,
                                    0x01 /* Payload > 128+1 Byte, but upper bit is 0*/,
                                    0x01 /* Connection attempt */,
                                    0x01, 0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,
                                    0x01, 0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01, // 32
                                    0x01, 0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,
                                    0x01, 0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01, // 64
                                    0x01, 0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,
                                    0x01, 0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01, // 96
                                    0x01, 0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,
                                    0x01, 0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01, // 128
                                    0x03 /*ETX*/ };

            Assert.Equal(fex.TryExtract(data).frame.payload.Length, 128);
        }

        [Fact]
        public void TryExtract_YieldsFalse_IfDataIsMissingInMultiBytePacket()
        {
            FrameExtractor fex = new FrameExtractor();

            var data = new byte[] { 0x02 /*STX*/,
                                    0x81 /* Payload > 127 Byte, but upper bit is 1 */,
                                    0x02 /* Payload > 127+1 Byte, but upper bit is 1 */,
                                    0x01 /* Connection attempt */,
                                    // We have no additional data.
                                    0x03 /*ETX*/ };

            Assert.False(fex.TryExtract(data).success);
        }

        [Fact]
        public void TryExtract_ExtractsPacketType()
        {
            FrameExtractor fex = new FrameExtractor();


            var data = new byte[] { 0x02 /*STX*/,
                                    0x01 /* 1 Byte Payload */,
                                    0x01 /* Connection attempt */,
                                    // We have no additional data.
                                    0x03 /*ETX*/ };

            Assert.Equal(fex.TryExtract(data).frame.frameType, FrameType.Connect);
        }

        [Fact]
        public void TryExtract_ExtractsData()
        {
            FrameExtractor fex = new FrameExtractor();


            var data = new byte[] { 0x02 /*STX*/,
                                    0x05 /* 1 Byte Payload */,
                                    0x01 /* Connection attempt */,
                                    0x01,0x02, 0x03, 0x04,
                                    0x03 /*ETX*/ };

            Assert.True(fex.TryExtract(data).frame
                                            .payload
                                            .SequenceEqual(new byte[] {0x01, 0x02, 0x03, 0x04 }));
        }
    }
}
