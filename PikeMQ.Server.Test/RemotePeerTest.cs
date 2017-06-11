using PikeMQ.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FakeItEasy;
using PikeMQ.Core.StatusCodes;

namespace PikeMQ.Server.Test
{
    public class RemotePeerTest
    {
        RemotePeer p = new RemotePeer(null);
        Frame lastFrame;
        AsyncSocket socket;

        public RemotePeerTest()
        {
            socket = A.Fake<AsyncSocket>();
            p = new RemotePeer(socket);
        }

        public void Connect()
        {
            var data = new byte[] {  0x02, // STX
                                     0x15,  // 21 byte payload
                                     0x01,   // Connection Attempt
                                     0x01,   // Protoversion
                                    // Client ID
                                    (byte) 'A', (byte) 'B', (byte) 'C', (byte) 'D', 0x00, 0x00, 0x00, 0x00,
                                     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                     0x00,  // No SecData
                                     0x00,  // No LastWillChan
                                     0x00,  // No LastWillData
                                     0x03
                                  };

            p.DataReceivedDelegate(data, data.Length);

        }

        private void Send(byte[] data)
        {
            var frame = new List<byte>();
            frame.Add(0x02);
            frame.Add((byte)data.Length);
            frame.AddRange(data);
            frame.Add(0x03);
            p.DataReceivedDelegate(frame.ToArray(), frame.Count);

        }

        [Fact]
        public void RemotePeer_StartsWithUnknownConState()
        {
            Assert.Equal(RemotePeer.PeerState.Unknown, p.ConnectionState);
        }

        [Fact]
        public void SuccessfulConnectionAttempt_PutsPeerIntoConnectedState()
        {
            Connect();
            Assert.Equal(RemotePeer.PeerState.Connected, p.ConnectionState);
        }

        [Fact]
        public void BadProtocolVersion_DoesNotConnect()
        {
            var data = new byte[] {  
                                     0x01,   // Connection Attempt
                                     0x04,   // Bad Protoversion
                                    // Client ID
                                    (byte) 'A', (byte) 'B', (byte) 'C', (byte) 'D', 0x00, 0x00, 0x00, 0x00,
                                     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                     0x00,  // No SecData
                                     0x00,  // No LastWillChan
                                     0x00,  // No LastWillData
                                  };

            Send(data);
            Assert.NotEqual(RemotePeer.PeerState.Connected, p.ConnectionState);
        }

        [Fact]
        public void FrameNotRelayed_IfNotConnected()
        {
            var data = new byte[] { 0x02 /*STX*/,
                                    0x01 /* 1 Byte Payload */,
                                    0x03 /* Publish attempt */,
                                    // We have no additional data.
                                    0x03 /*ETX*/ };
            p.SetFrameReceivedCb((x, y) => lastFrame = x);
            p.DataReceivedDelegate(data, 4);

            Assert.Null(lastFrame);           
        }        

        [Fact]
        public void CanExtractSimpleFrame()
        {
            Connect();
            var data = new byte[] { 0x02 /*STX*/,
                                    0x01 /* 1 Byte Payload */,
                                    0x03 /* Connection attempt */,
                                    // We have no additional data.
                                    0x03 /*ETX*/ };
            p.SetFrameReceivedCb((x,y)  => lastFrame = x);
            p.DataReceivedDelegate(data, 4);

            Assert.NotNull(lastFrame);
            Assert.Equal(lastFrame.frameType, FrameType.Publish);
        }

        [Fact]
        public void CanExtractTwoFrames()
        {
            Connect();
            var data = new byte[] { 0x02 /*STX*/,
                                    0x01 /* 1 Byte Payload */,
                                    0x03 /* Publish attempt */,
                                    // We have no additional data.
                                    0x03, /*ETX*/

                                    0x02 /*STX*/,
                                    0x01 /* 1 Byte Payload */,
                                    0x0E /* Disconnect attempt */,
                                    // We have no additional data.
                                    0x03 /*ETX*/
                                  };
            p.SetFrameReceivedCb((x, y) => lastFrame = x);
            p.DataReceivedDelegate(data, 8);

            Assert.NotNull(lastFrame);
            Assert.Equal(lastFrame.frameType, FrameType.Disconnect);
        }

        [Fact]
        public void BufferOverflow_IsRecoverable()
        {
            Assert.False(true, "Implement");
        }

        [Fact]
        public void GarbageInBuffer_IsRecoverable()
        {

            Connect();
            var data = new byte[] { 0xFF,
                                    0x24,
                                    0x02 /*STX*/,
                                    0x01 /* 1 Byte Payload */,
                                    0x03 /* Connection attempt */,
                                    // We have no additional data.
                                    0x03 /*ETX*/ };
            p.SetFrameReceivedCb((x, y) => lastFrame = x);
            p.DataReceivedDelegate(data, 6);

            Assert.NotNull(lastFrame);
            Assert.Equal(lastFrame.frameType, FrameType.Publish);
        }

        [Fact]
        public void SendConnectReply_ConSuccessful_SendsConnectionReply()
        {
            FrameBuilder bld = new FrameBuilder();
            bld.WriteByte((byte)ConnectionAttemptStatus.Accepted);
            var expectedFrame = bld.Build(FrameType.ConReply);

            p.SendConnectionReply(ConnectionAttemptStatus.Accepted);

            A.CallTo(() => socket.Send(A<byte[]>.That.IsSameSequenceAs(expectedFrame))).MustHaveHappened();

        }

        [Fact]
        public void SendConnectReply_ConRefused_SendsConnectionReply()
        {
            FrameBuilder bld = new FrameBuilder();
            bld.WriteByte((byte)ConnectionAttemptStatus.Refused);
            var expectedFrame = bld.Build(FrameType.ConReply);

            p.SendConnectionReply(ConnectionAttemptStatus.Refused);

            A.CallTo(() => socket.Send(A<byte[]>.That.IsSameSequenceAs(expectedFrame))).MustHaveHappened();

        }

        [Fact]
        public void SendSubscribeReply_SendsSubscribeReply()
        {
            FrameBuilder bld = new FrameBuilder();
            bld.WriteString("Test");
            bld.WriteByte((byte)SubscribeStatus.Success);
            var expectedFrame = bld.Build(FrameType.SubReply);

            p.SendSubscribeReply("Test", SubscribeStatus.Success);

            A.CallTo(() => socket.Send(A<byte[]>.That.IsSameSequenceAs(expectedFrame))).MustHaveHappened();
            
        }

        [Fact]
        public void PostMessage_SendsMessageToPeer()
        {
            FrameBuilder bld = new FrameBuilder();
            bld.WriteByte((byte)QoS.BestEffort);
            bld.WriteArray(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            bld.WriteString("Fnord");
            bld.WriteString("I am a payload");

            var expected = bld.Build(FrameType.ChannelEvent);

            var result = p.PostMessage("Fnord", Encoding.UTF8.GetBytes("I am a payload"), QoS.BestEffort);
            result.Wait();

            A.CallTo(() => socket.Send(A<byte[]>.That.IsSameSequenceAs(expected))).MustHaveHappened();
        }

        [Fact]
        public void SendPublishReply_SendsMessageToPeer()
        {
            FrameBuilder bld = new FrameBuilder();
            bld.WriteArray(BitConverter.GetBytes((UInt32) 112233));       
            var expected = bld.Build(FrameType.PubReply);

            p.SendPublishReply(112233);
            A.CallTo(() => socket.Send(A<byte[]>.That.IsSameSequenceAs(expected))).MustHaveHappened();
        }
    }
}
