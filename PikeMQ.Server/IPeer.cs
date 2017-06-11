using PikeMQ.Core.StatusCodes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static PikeMQ.Core.FrameReceived;

namespace PikeMQ.Core
{
    public interface IPeer
    {         
        Task<PostResult> PostMessage(string topic, byte[] data, QoS qos);
        void SetFrameReceivedCb(FrameReceivedDelegate frd);

        //void Ping();
        //void Disconnect();
        void SendSubscribeReply(string channel, SubscribeStatus status);
        void SendConnectionReply(ConnectionAttemptStatus status);
        void SendUnsubReply(string channel);
        void SendPublishReply(UInt32 messageId);        
    }
}
