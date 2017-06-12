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
        public delegate void MessageReceived(string channel, byte[] data);

        public event OnEvent OnConnect;
        public event OnEvent OnDisconnect;
        public event MessageReceived OnMessageReceived = delegate { };        

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
            if (f.frameType == FrameType.ChannelEvent)
                DispatchChannelEvent(f);
        }

        private void DispatchChannelEvent(Frame f)
        {
            var flagByte = f.payload[0];
            var messageId = new byte[4];
            Array.Copy(f.payload, 1, messageId, 0, 4);
            // var clientId = ...
            var channel = Util.ExtractByteArray(f.payload, 5);
            var payload = Util.ExtractByteArray(f.payload, 5 + channel.numBytesUsed);

            OnMessageReceived(System.Text.Encoding.UTF8.GetString(channel.data), payload.data);

            if((flagByte & 0x01) != 0x00)
            {
                FrameBuilder builder = new FrameBuilder();
                builder.WriteArray(messageId);
                builder.WriteByte((byte)ChannelEventResult.Ok);
                socket.Send(builder.Build(FrameType.EventAck));
            }
        }

        public Task<PostResult> PostMessage(string topic, byte[] data, QoS qos)
        {
            FrameBuilder blder = new FrameBuilder();
            blder.WriteByte((byte)qos);
            // Empty packet id for now!
            blder.WriteArray(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            blder.WriteString(topic);
            blder.WriteMultiByte(data.Length);
            blder.WriteArray(data);
            var theFrame = blder.Build(FrameType.Publish);
            socket.Send(theFrame);
            return new Task<PostResult>(() => PostResult.Dispatched);
        }

        // No need for these to be public!
        public void SendSubscribeRequest(string channel)
        {
            FrameBuilder blder = new FrameBuilder();
            blder.WriteString(channel);
            socket.Send(blder.Build(FrameType.Subscribe));
        }

        public void SendUnsubscribeRequest(string channel)
        {
        }

        public void SendUnsubReply(string channel)
        {
            throw new NotImplementedException();
        }
    }
}
