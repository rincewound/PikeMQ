using PikeMQ.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using PikeMQ.Core.StatusCodes;

namespace PikeMQ.Server
{
    public class RemotePeer : IPeer
    {
        public enum PeerState
        {
            Unknown,
            Connected,
            Disconnected
        }

        AsyncSocket socket;
        FrameExtractor fex = new FrameExtractor();
        public PeerState ConnectionState { get; private set; }
        byte[] clientID = new byte[16];

        FrameReceived.FrameReceivedDelegate frameReceive = delegate { };
        const int MaxReceiveBuffer = 65565;

        byte[] ReceiveBuffer = new byte[MaxReceiveBuffer];
        int WritePos;

        public RemotePeer(AsyncSocket socket)
        {
            this.socket = socket;
        }

        public virtual void Stop()
        {
            socket.OnDataReceived -= DataReceivedDelegate;
            socket.Stop();
        }

        public virtual void Run()
        {
            socket.OnDataReceived += DataReceivedDelegate;
            socket.StartReceiving();            
        }

        public override string ToString()
        {
            return "RemotePeer@" + socket.ToString();
        }

        async Task<PostResult> IPeer.PostMessage(string topic, byte[] data, QoS qos)
        {
            return PostResult.Ok;
        }

        public void DataReceivedDelegate(byte[] buffer, int count)
        {
            // Socket possible closed...
            if(count == 0)
            {
                Disconnect();
            }

            if (WritePos + count > MaxReceiveBuffer)
            {
                // Overflow!
            }            
            Array.Copy(buffer, 0, ReceiveBuffer, WritePos, count);
            WritePos += count;

            var res = fex.TryExtract(ReceiveBuffer);
            while (res.success)
            {
                if (!res.success)
                    break;

                // dispatch frame, clear buffer...
                Array.Copy(ReceiveBuffer, (int)res.firstUnusedByte, ReceiveBuffer, 0, (int)res.firstUnusedByte);
                Array.Clear(ReceiveBuffer, (int)res.firstUnusedByte, (int) (WritePos - res.firstUnusedByte));
                WritePos -= (int)res.firstUnusedByte;

                OnFrameReceived(res.frame);
                res = fex.TryExtract(ReceiveBuffer);
            }

            // @ToDo Possible failure mode: no stx at the beginning
            // ==> cannot recover :(

        }

        private void Disconnect()
        {
            Frame disconnect = new Frame();
            disconnect.frameType = FrameType.Disconnect;
            OnFrameReceived(disconnect);
        }

        void OnFrameReceived(Frame f)
        {
            // CHeck internal state:
            if(f.frameType == FrameType.Connect)
            {
                // Bad State for connectionattempt
                if (ConnectionState != PeerState.Unknown)
                {
                    // Break connection, kill peer. Set fire to his house.
                    Disconnect();
                    return;
                }

                if(ProcessConnectionAttempt(f))
                    ConnectionState = PeerState.Connected;
            }

            // Process frame, if we are connected.
            if(ConnectionState == PeerState.Connected ||
               f.frameType == FrameType.Disconnect)
                frameReceive(f, this);
        }

        private bool ProcessConnectionAttempt(Frame f)
        {
            // Read data from frame..
            MemoryStream strm = new MemoryStream(f.payload);

            byte protoVersion = (byte) strm.ReadByte();

            if (protoVersion != 0x01)
                return false;

            
            strm.Read(clientID, 0, 16);
            var secDataLen = Util.ExtractMultiByte(strm);

            if (!secDataLen.success)
                return false;

            // Skip security data for now.
            strm.Seek(secDataLen.value, SeekOrigin.Current);

            var lastWillChanLen = Util.ExtractMultiByte(strm);
            strm.Seek(lastWillChanLen.value, SeekOrigin.Current);
            var lastWillDataLen = Util.ExtractMultiByte(strm);
            strm.Seek(lastWillChanLen.value, SeekOrigin.Current);

            return true;
        }

        public void SetFrameReceiver(FrameReceived.FrameReceivedDelegate frd)
        {
            frameReceive = frd;
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
