using System;
using System.Collections.Generic;
using System.Text;

namespace PikeMQ.Core
{
    public class FrameReceived
    {
       public delegate void FrameReceivedDelegate(Frame frame, IPeer source);
    }
}
