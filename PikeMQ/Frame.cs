using System;
using System.Collections.Generic;
using System.Text;

namespace PikeMQ.Core
{
    public class Frame
    {
        public FrameType frameType;
        public byte[] payload;
    }
}
