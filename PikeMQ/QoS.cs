using System;
using System.Collections.Generic;
using System.Text;

namespace PikeMQ.Core
{
    public enum QoS: byte
    {
        BestEffort         = 0x01,
        GuaranteedDispatch = 0x02,
        GuaranteedDelivery = 0x03
    }

    public enum PostResult: byte
    {

        // Refused to dispatch (too much load?)
        DispatchError = 0x01,
        // Failed to deliver (nobody was listening)
        DeliveryError = 0x02,

        // Dispatch Message was accepted for delivery
        Dispatched = 0x03,
        // Message was delivered to at least one peer
        Delivered = 0x04,
    }
}
