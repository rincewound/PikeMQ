using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;

namespace PikeMQ.Core.Test
{
    public class FrameBuilderTest
    {

        FrameBuilder builder = new FrameBuilder();

        [Fact]
        public void WriteArray_WritesArray()
        {
            builder.WriteArray(new byte[] { 0x01, 0x02, 0x03 });

            var data = builder.GetData();
            Assert.True(data.SequenceEqual(new byte[] { 0x01, 0x02, 0x03 }));
        }

        [Fact]
        public void WriteMultiByte_WritesMultiByte()
        {
            builder.WriteMultiByte(331);
            var data = builder.GetData();
            var decoded = Util.ExtractMultiByte(data, 0).value;
            Assert.Equal(331u, decoded);
        }

        [Fact]
        public void Build_CreatesFrame()
        {
            builder.WriteArray(new byte[] { 0xAA, 0xBB });

            var frame = builder.Build(FrameType.Disconnect);

            var expectedFrame = new byte[]
                                    {
                                        0x02,
                                        0x02,
                                        0x0E,
                                        0xAA,
                                        0xBB,
                                        0x03
                                    };

            Assert.True(expectedFrame.SequenceEqual(frame));
        }

    }
}
