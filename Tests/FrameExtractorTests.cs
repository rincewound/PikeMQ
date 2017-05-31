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

            Assert.True(fex.TryExtract(data, 4).success == FrameExtractor.ResultState.Ok);

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

            Assert.False(fex.TryExtract(data, 3).success == FrameExtractor.ResultState.MalformedPacket);
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

            Assert.True(fex.TryExtract(data, 4).success == FrameExtractor.ResultState.MissingData);
        }

        [Fact]
        public void TryExtract_YieldsFalse_IfSTXIsMissing()
        {
            FrameExtractor fex = new FrameExtractor();

            var data = new byte[] { 0x01 /* 1 Byte Payload */,
                                    0x01 /* Connection attempt */,
                                    // We have no additional data.
                                    0x03 /*ETX*/ };

            Assert.True(fex.TryExtract(data, 3).success == FrameExtractor.ResultState.MalformedPacket);
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

            Assert.True(fex.TryExtract(data, data.Length).success == FrameExtractor.ResultState.Ok);            
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

            Assert.Equal(fex.TryExtract(data, data.Length).frame.payload.Length, 128);
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

            Assert.True(fex.TryExtract(data,5).success == FrameExtractor.ResultState.MissingData );
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

            Assert.Equal(fex.TryExtract(data, 4).frame.frameType, FrameType.Connect);
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

            Assert.True(fex.TryExtract(data, 8).frame
                                            .payload
                                            .SequenceEqual(new byte[] {0x01, 0x02, 0x03, 0x04 }));
        }

        [Fact]
        public void CanExtract_FrameBuilderOutput()
        {
            FrameBuilder blder = new FrameBuilder();

            blder.WriteByte(0x44);     //Protocol Version 1
            // empty client id for now
            blder.WriteArray(new byte[16]);
            blder.WriteMultiByte(0);    // no secdata
            blder.WriteMultiByte(0);    // no lastwill channel
            blder.WriteMultiByte(0);    // no lastwill data

            var theFrame = blder.Build(FrameType.Connect);

            FrameExtractor fex = new FrameExtractor();
            Assert.True(fex.TryExtract(theFrame,23).success == FrameExtractor.ResultState.Ok);
        }

    }
}
