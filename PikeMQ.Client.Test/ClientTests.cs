using FakeItEasy;
using PikeMQ.Core;
using System;
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

            blder.WriteByte(1);
            // empty client id for now
            blder.WriteArray(new byte[16]);
            blder.WriteMultiByte(0);    // no secdata
            blder.WriteMultiByte(0);    // no lastwill channel
            blder.WriteMultiByte(0);    // no lastwill data

            var theFrame = blder.Build(FrameType.Connect);

            client.Connect();

            A.CallTo(() => socket.Send(A<byte[]>.That.IsSameSequenceAs(theFrame))).MustHaveHappened();

        }
    }
}
