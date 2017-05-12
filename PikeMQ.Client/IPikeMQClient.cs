using PikeMQ.Core;
using System;
using System.Net.Sockets;
using PikeMQ.Core.StatusCodes;
using System.Threading.Tasks;

namespace PikeMQ.Client
{
    public class PikeMQClient: PeerBaseImpl
    {
        public delegate void OnEvent();

        public event OnEvent OnConnect;
        public event OnEvent OnDisconnect;
        public event OnEvent OnMessageReceived;        

        public PikeMQClient(AsyncSocket socket): base(socket)
        {
        }
    
        public void Connect()
        {
            FrameBuilder builder = new FrameBuilder();

            // Build connection request.
            FrameBuilder blder = new FrameBuilder();

            blder.WriteByte(1);
            // empty client id for now
            blder.WriteArray(new byte[16]);

            blder.WriteMultiByte(0);    // no secdata
            blder.WriteMultiByte(0);    // no lastwill channel
            blder.WriteMultiByte(0);    // no lastwill data

            var theFrame = blder.Build(FrameType.Connect);

            socket.Send(theFrame);

        }

        protected override void OnFrameReceived(Frame f)
        {
            throw new NotImplementedException();
        }

        public override Task<PostResult> PostMessage(string topic, byte[] data, QoS qos)
        {
            throw new NotImplementedException();
        }

        public override void SetFrameReceiver(FrameReceived.FrameReceivedDelegate frd)
        {
            throw new NotImplementedException();
        }

        public override void SendSubscribeReply(string channel, SubscribeStatus status)
        {
            throw new NotImplementedException();
        }

        public override void SendConnectionReply(ConnectionAttemptStatus status)
        {
            throw new NotImplementedException();
        }

        public override void SendUnsubReply(string channel)
        {
            throw new NotImplementedException();
        }
    }
}
