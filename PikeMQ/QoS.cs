using System;
using System.Collections.Generic;
using System.Text;

namespace PikeMQ.Core
{
    public enum QoS: byte
    {
        BestEffort = 0x01,
        GuaranteedDispatch = 0x02,
        GuaranteedDelivery = 0x03
    }

    public enum PostResult: byte
    {
        Ok = 0x01,
        DispatchError = 0x02,
        DeliveryError = 0x03
    }
}
