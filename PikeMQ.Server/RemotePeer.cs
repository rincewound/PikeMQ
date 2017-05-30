using PikeMQ.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using PikeMQ.Core.StatusCodes;

namespace PikeMQ.Server
{
    public class RemotePeer : PeerBaseImpl, IPeer
    {
        public enum PeerState
        {
            Unknown,
            Connected,
            Disconnected
        }
              
        public PeerState ConnectionState { get; private set; }
        byte[] clientID = new byte[16];

        FrameReceived.FrameReceivedDelegate frameReceive = delegate { };

        public RemotePeer(AsyncSocket sock) : base(sock)
        {
        }

        public override string ToString()
        {
            return "RemotePeer@" + socket.ToString();
        }

        public async Task<PostResult> PostMessage(string topic, byte[] data, QoS qos)
        {
            FrameBuilder bld = new FrameBuilder();
            bld.WriteByte((byte)QoS.BestEffort);
            // ToDo: Generate Frame Number to receive ACK.
            bld.WriteArray(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            bld.WriteString(topic);
            bld.WriteMultiByte(data.Length);
            bld.WriteArray(data);

            socket.Send(bld.Build(FrameType.ChannelEvent));

            return PostResult.Ok;
        }
              
        private bool ProcessConnectionAttempt(Frame f)
        {
            // Read data from frame..
            MemoryStream strm = new MemoryStream(f.payload);

            byte protoVersion = (byte) strm.ReadByte();

            if (protoVersion != 0x01)
                return false;

            
            //strm.Read(clientID, 0, 16);
            //var secDataLen = Util.ExtractMultiByte(strm);

            //if (!secDataLen.success)
            //    return false;

            //// Skip security data for now.
            //strm.Seek(secDataLen.value, SeekOrigin.Current);

            //var lastWillChanLen = Util.ExtractMultiByte(strm);
            //strm.Seek(lastWillChanLen.value, SeekOrigin.Current);
            //var lastWillDataLen = Util.ExtractMultiByte(strm);
            //strm.Seek(lastWillChanLen.value, SeekOrigin.Current);

            return true;
        }

        public void SetFrameReceivedCb(FrameReceived.FrameReceivedDelegate frd)
        {
            frameReceive = frd;
        }

        protected override void OnFrameReceived(Frame f)
        {
            // CHeck internal state:
            if (f.frameType == FrameType.Connect)
            {
                // Bad State for connectionattempt
                if (ConnectionState != PeerState.Unknown)
                {
                    // Break connection, kill peer. Set fire to his house.
                    Disconnect();
                    return;
                }

                if (ProcessConnectionAttempt(f))
                    ConnectionState = PeerState.Connected;
            }

            // Process frame, if we are connected.
            if (ConnectionState == PeerState.Connected ||
               f.frameType == FrameType.Disconnect)
                frameReceive(f, this);
        }

        public virtual void SendSubscribeReply(string channel, SubscribeStatus status)
        {
            FrameBuilder theBuilder = new FrameBuilder();
            theBuilder.WriteString(channel);
            theBuilder.WriteByte((byte)status);

            socket.Send(theBuilder.Build(FrameType.SubReply));
        }

        public virtual void SendConnectionReply(ConnectionAttemptStatus status)
        {
            FrameBuilder theBuilder = new FrameBuilder();
            theBuilder.WriteByte((byte)status);

            socket.Send(theBuilder.Build(FrameType.ConReply));
        }

        public virtual void SendUnsubReply(string channel)
        {
            FrameBuilder theBuilder = new FrameBuilder();
            theBuilder.WriteString(channel);

            socket.Send(theBuilder.Build(FrameType.UnsubReply));
        }
    }
}
