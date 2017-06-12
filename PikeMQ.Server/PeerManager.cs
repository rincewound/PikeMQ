using System;
using System.Collections.Generic;
using System.Text;
using PikeMQ.Core;

namespace PikeMQ.Server
{
    public class PeerManager : IPeerManager
    {
        List<IPeer> peers = new List<IPeer>();
        ISubscriptionManager subMan = MicroIOC.IOC.Resolve<ISubscriptionManager>();

        public virtual void RegisterPeer(IPeer thePeer)
        {
            peers.Add(thePeer);
            thePeer.SetFrameReceivedCb(FrameReceived);
        }

        public virtual void UnregisterPeer(IPeer thePeer)
        {
            peers.Remove(thePeer);
            subMan.UnsubscribeAll(thePeer);
        }

        public virtual void FrameReceived(Frame frame, IPeer source)
        {
            switch (frame.frameType)
            {
                case FrameType.Connect:
                    break;
                case FrameType.Disconnect:
                    var rp = source as RemotePeer;
                    if (rp != null)
                        rp.Stop();
                    UnregisterPeer(source);
                    break;
                case FrameType.Ping:
                    break;
                case FrameType.Publish:
                    HandlePublish(frame, source);
                    break;
                case FrameType.Subscribe:
                    HandleSubscription(frame, source);
                    break;
                case FrameType.Unsub:
                    HandleUnsub(frame, source);
                    break;
                default:
                    break;
            }
        }

        private void HandlePublish(Frame frame, IPeer source)
        {
            // the 5 offset is added because we dont process QoS and Framenumber yet.
            var qos = (QoS)frame.payload[0];
            var msgId = BitConverter.ToUInt32(frame.payload, 1);

            var len = Util.ExtractMultiByte(frame.payload, 5);

            var buff = new byte[len.value];
            Array.Copy(frame.payload, 5 + (int)len.numBytesUsed, buff, 0, (int)len.value);
            var channelName = Encoding.UTF8.GetString(buff);

            var payloadLenOffset = 5 + len.numBytesUsed + len.value;
            var payloadLen = Util.ExtractMultiByte(frame.payload, (int) payloadLenOffset);

            var payloadBuff = new byte[payloadLen.value];
            Array.Copy(frame.payload, (int)(payloadLenOffset + payloadLen.numBytesUsed), payloadBuff, 0, (int) payloadLen.value);
            var res = subMan.DispatchMessage(channelName, payloadBuff, qos);
            
            switch(qos)
            {
                case QoS.BestEffort:
                    // No answer in BestEffort case, we favor throughput here.
                    break;
                case QoS.GuaranteedDelivery:
                    // If GuaranteedDelivery was requested, we send a failure notice if we did not
                    // get a positive answer
                    source.SendPublishReply(msgId, res == PostResult.Delivered ? Core.StatusCodes.PublishStatus.AckDelivery
                                                                               : Core.StatusCodes.PublishStatus.NakDelivery);

                    break;
                case QoS.GuaranteedDispatch:
                    source.SendPublishReply(msgId, res >= PostResult.DeliveryError ? Core.StatusCodes.PublishStatus.Ack
                                                                                   : Core.StatusCodes.PublishStatus.NakDispatch);
                    break;
            }
        }

        private void HandleSubscription(Frame frame, IPeer source)
        {
            var len = Util.ExtractMultiByte(frame.payload, 0);

            if(!len.success)
            {
                // something bad happened.
            }
            var buff = new byte[len.value];
            Array.Copy(frame.payload, (int) len.numBytesUsed, buff, 0, (int)len.value);
            var channelName = Encoding.UTF8.GetString(buff);

            subMan.Subscribe(source, channelName);

            source.SendSubscribeReply(channelName, 0x00);
        }

        private void HandleUnsub(Frame frame, IPeer source)
        {
            var len = Util.ExtractMultiByte(frame.payload, 0);

            if (!len.success)
            {
                // something bad happened.
            }
            var buff = new byte[len.value];
            Array.Copy(frame.payload, (int)len.numBytesUsed, buff, 0, (int)len.value);
            var channelName = Encoding.UTF8.GetString(buff);

            subMan.Unsubscribe(source, channelName);

            source.SendUnsubReply(channelName);
        }
    }
}
