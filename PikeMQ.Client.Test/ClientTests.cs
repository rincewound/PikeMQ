using FakeItEasy;
using PikeMQ.Core;
using System;
using System.Text;
using Xunit;

namespace PikeMQ.Client.Test
{
    public class ClientTests
    {
        AsyncSocket socket = A.Fake<AsyncSocket>();
        PikeMQClient client;

        public ClientTests()
        {
            client = new PikeMQClient(socket);
        }

        [Fact]
        public void Connect_SendsConnectionRequest()
        {
            FrameBuilder blder = new FrameBuilder();

            blder.WriteByte(1);     //Protocol Version 1
            // empty client id for now
            blder.WriteArray(new byte[16]);
            blder.WriteMultiByte(0);    // no secdata
            blder.WriteMultiByte(0);    // no lastwill channel
            blder.WriteMultiByte(0);    // no lastwill data

            var theFrame = blder.Build(FrameType.Connect);

            client.Connect();

            A.CallTo(() => socket.Send(A<byte[]>.That.IsSameSequenceAs(theFrame))).MustHaveHappened();

        }

        [Fact]
        public void Subscribe_SendsSubscriptionRequest()
        {
            FrameBuilder blder = new FrameBuilder();
            blder.WriteString("Fnord");
            var theFrame = blder.Build(FrameType.Subscribe);
            client.SendSubscribeRequest("Fnord");
            A.CallTo(() => socket.Send(A<byte[]>.That.IsSameSequenceAs(theFrame))).MustHaveHappened();
        }

        [Fact]
        public void Publish_SendsPublishframe()
        {
            FrameBuilder blder = new FrameBuilder();
            blder.WriteByte((byte)QoS.BestEffort);
            blder.WriteArray(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            blder.WriteString("What a great channel!");
            blder.WriteString("This is my data!");
            var theFrame = blder.Build(FrameType.Publish);
            client.PostMessage("What a great channel!", Encoding.UTF8.GetBytes("This is my data!"), QoS.BestEffort);
            A.CallTo(() => socket.Send(A<byte[]>.That.IsSameSequenceAs(theFrame))).MustHaveHappened();
        }
    }
}
