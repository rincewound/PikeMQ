using System;
using System.Collections.Generic;
using System.Text;

namespace PikeMQ.Core.StatusCodes
{
    public enum ConnectionAttemptStatus: byte
    {
        Accepted,
        Refused
    }

    public enum PublishStatus: byte
    {
        Ack,
        NakDispatch,
        AckDelivery,
        NakDelivery
    }

    public enum SubscribeStatus: byte
    {
        Success,
        FailedNotAuthorized,
        GeneralError = 0xFF
    }
}
