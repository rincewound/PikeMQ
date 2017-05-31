using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PikeMQ.Core
{
    public class FrameExtractor
    {
        const byte STX = 0x02;
        const byte ETX = 0x03;

        public enum ResultState
        {
            Ok,
            MalformedPacket,
            MissingData
        }

        /// <summary>
        /// Tries to extract a frame from a given datastream. Returns a Tuple, indicating
        /// success, frame and the amount of consumed data (by means of the first byte in the
        /// inputbuffer that has not been processed).
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public (ResultState success, Frame frame, uint firstUnusedByte) TryExtract(byte[] data, int numBytesAvailable)
        {
            // Check if the first byte is an STX, if not, fail!
            if (data[0] != STX)
                return (ResultState.MalformedPacket, null, 0);
            
            var len = Util.ExtractMultiByte(data, 1);

            if (!len.success)
                return (ResultState.MissingData, null, 0);

            var dataStart = len.numBytesUsed + 1;

            if (numBytesAvailable - 2 <= len.value)
                return (ResultState.MissingData, null, 0);

            // Add one offset for length byte and one for the STX byte.
            if (data[dataStart + len.value] != ETX)
                return (ResultState.MalformedPacket, null, 0);            

            var theFrame = new Frame();
            theFrame.frameType = (FrameType)data[dataStart];

            if (len.value > 1)
            {
                theFrame.payload = new byte[(int)len.value - 1];
                Array.Copy(data, dataStart + 1, theFrame.payload, 0, (int)(len.value - 1));
            }
            return (ResultState.Ok, theFrame, (uint) dataStart + len.value + 1);
        }
    }
}
