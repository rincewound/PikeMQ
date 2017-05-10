using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace PikeMQ.Core.Test
{
    public class UtilTests
    {
        [Fact]
        public void ExtractMultiByte_CorrectValueForSingleByte()
        {
            var res = Util.ExtractMultiByte(new byte[] { 21 }, 0);
            Assert.Equal(21u, res.value);
        }

        [Fact]
        public void ExtractMultiByte_CorrectValueForMultiByte_2Byte()
        {
            var res = Util.ExtractMultiByte(new byte[] { 0x81, 0x2 }, 0);
            Assert.Equal(130u, res.value);
            Assert.Equal(2, res.numBytesUsed);
        }

        [Fact]
        public void ExtractMultiByte_CorrectValueForMultiByte_1Byte_WithOffset()
        {
            var res = Util.ExtractMultiByte(new byte[] { 0x81, 0x2 }, 1);
            Assert.Equal(2u, res.value);
            Assert.Equal(1, res.numBytesUsed);
        }

        [Fact]
        public void ExtractMultiByte_CorrectValueForMultiByte_2Byte_WithOffset()
        {
            var res = Util.ExtractMultiByte(new byte[] { 0xFF, 0x81, 0x2 }, 1);
            Assert.Equal(130u, res.value);
        }

        [Fact]
        public void ExtractMultiByte_FailsForMultiByteWithMissingLastByte()
        {
            var res = Util.ExtractMultiByte(new byte[] { 0x81, 0x82 }, 0);
            Assert.False(res.success);
        }
    }
}
