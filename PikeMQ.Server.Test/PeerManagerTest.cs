using System;
using System.Collections.Generic;
using System.Text;

using FakeItEasy;
using Xunit;
using PikeMQ.Core;

namespace PikeMQ.Server.Test
{
    public class PeerManagerTest
    {
        ISubscriptionManager subManFake;
        PeerManager peerMan;
        IPeer rpFake;

        public PeerManagerTest()
        {
            rpFake = A.Fake<IPeer>();
            subManFake = A.Fake<ISubscriptionManager>();
            MicroIOC.IOC.Reset();
            MicroIOC.IOC.Register<ISubscriptionManager>(() => subManFake);
            peerMan = new PeerManager();
            peerMan.RegisterPeer(rpFake);
        }

        [Fact]
        public void DisconnectFrame_RemovesAllSubscriptions()
        {
            Frame f = new Frame();
            f.frameType = FrameType.Disconnect;            
            peerMan.FrameReceived(f, rpFake);

            A.CallTo(() => subManFake.UnsubscribeAll(rpFake)).MustHaveHappened();        
        }

        [Fact]
        public void PublishFrame_CallsSubscriptionManager()
        {
            FrameBuilder fb = new FrameBuilder();
            fb.WriteByte((byte)QoS.BestEffort);
            fb.WriteArray(new byte[] { 0xCC, 0xDD, 0xEE, 0xFF });
            fb.WriteString("Test");
            fb.WriteMultiByte(3);
            fb.WriteArray(new byte[] { 0xAA, 0xBB, 0xCC });
            var data = fb.GetData();
            var frm = new Frame();
            frm.payload = data;
            frm.frameType = FrameType.Publish;
            peerMan.FrameReceived(frm, rpFake);
            A.CallTo(() => subManFake.DispatchMessage("Test", A<byte[]>.That.IsSameSequenceAs(new byte[] {0xAA, 0xBB, 0xCC }), A<QoS>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void PublishFrame_Qos_GuaranteedDispatch_SendsReply()
        {
            FrameBuilder fb = new FrameBuilder();
            fb.WriteByte((byte)QoS.GuaranteedDispatch);
            fb.WriteArray(BitConverter.GetBytes((UInt32) 123456));
            fb.WriteString("Test");
            fb.WriteMultiByte(3);
            fb.WriteArray(new byte[] { 0xAA, 0xBB, 0xCC });
            var data = fb.GetData();
            var frm = new Frame();
            frm.payload = data;
            frm.frameType = FrameType.Publish;
            A.CallTo(() => subManFake.DispatchMessage(A<string>.Ignored, A<byte[]>.Ignored, A<QoS>.Ignored)).Returns(PostResult.Dispatched);
            peerMan.FrameReceived(frm, rpFake);            
            A.CallTo(() => rpFake.SendPublishReply(123456, Core.StatusCodes.PublishStatus.Ack)).MustHaveHappened();
        }

        [Fact]
        public void PublishFrame_Qos_GuaranteedDelivery_SendsFailureNotice_IfNooneIsListening()
        {
            FrameBuilder fb = new FrameBuilder();
            fb.WriteByte((byte)QoS.GuaranteedDelivery);
            fb.WriteArray(BitConverter.GetBytes((UInt32)234567));
            fb.WriteString("Test");
            fb.WriteMultiByte(3);
            fb.WriteArray(new byte[] { 0xAA, 0xBB, 0xCC });
            var data = fb.GetData();
            var frm = new Frame();
            frm.payload = data;
            frm.frameType = FrameType.Publish;
            peerMan.FrameReceived(frm, rpFake);
            A.CallTo(() => rpFake.SendPublishReply(234567, Core.StatusCodes.PublishStatus.NakDelivery)).MustHaveHappened();
        }

        [Fact]
        public void SubscribeFrame_CallsSubscriptionManager()
        {
            FrameBuilder fb = new FrameBuilder();
            var bytes = Encoding.UTF8.GetBytes("Test");
            fb.WriteMultiByte(bytes.Length);
            fb.WriteArray(bytes);
            var data = fb.GetData();
            var frm = new Frame();
            frm.payload = data;
            frm.frameType = FrameType.Subscribe;
            peerMan.FrameReceived(frm, rpFake);

            A.CallTo(() => subManFake.Subscribe(rpFake, "Test")).MustHaveHappened();
            A.CallTo(() => rpFake.SendSubscribeReply("Test", Core.StatusCodes.SubscribeStatus.Success)).MustHaveHappened();
        }

        [Fact]
        public void UnsubscribeFrame_CallsSubscriptionManager()
        {
            FrameBuilder fb = new FrameBuilder();
            var bytes = Encoding.UTF8.GetBytes("Test");
            fb.WriteMultiByte(bytes.Length);
            fb.WriteArray(bytes);
            var data = fb.GetData();
            var frm = new Frame();
            frm.payload = data;
            frm.frameType = FrameType.Unsub;
            peerMan.FrameReceived(frm, rpFake);
            A.CallTo(() => subManFake.Unsubscribe(rpFake, "Test")).MustHaveHappened();
            A.CallTo(() => rpFake.SendUnsubReply("Test")).MustHaveHappened();
        }

    }
}
