using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PikeMQ.Core.StatusCodes;
using System.Linq;

namespace PikeMQ.Core
{
    public abstract class PeerBaseImpl
    {
        const int MaxReceiveBuffer = 65565;        
        byte[] ReceiveBuffer = new byte[MaxReceiveBuffer];
        int WritePos;

        FrameExtractor fex = new FrameExtractor();

        protected AsyncSocket socket;

        public PeerBaseImpl(AsyncSocket sock)
        {
            socket = sock;
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

        public void DataReceivedDelegate(byte[] buffer, int count)
        {
            // Socket possible closed...
            if (count == 0)
            {
                Disconnect();
                return;
            }

            if (WritePos + count > MaxReceiveBuffer)
            {
                // Overflow!
            }
            Array.Copy(buffer, 0, ReceiveBuffer, WritePos, count);
            WritePos += count;

            // Not an stx at the beginning--> search it an kill off any 
            // preceding data
            if (ReceiveBuffer[0] != 0x02)
                ReceiveBuffer = ReceiveBuffer.SkipWhile(x => x != 0x02).ToArray();

            var res = fex.TryExtract(ReceiveBuffer);
            while (res.success)
            {
                if (!res.success)
                {
                    break;
                }

                // dispatch frame, clear buffer...
                Array.Copy(ReceiveBuffer, (int)res.firstUnusedByte, ReceiveBuffer, 0, (int)res.firstUnusedByte);
                Array.Clear(ReceiveBuffer, (int)res.firstUnusedByte, (int)(WritePos - res.firstUnusedByte));
                WritePos -= (int)res.firstUnusedByte;

                OnFrameReceived(res.frame);
                res = fex.TryExtract(ReceiveBuffer);
            }

            // @ToDo Possible failure mode: no stx at the beginning
            // ==> cannot recover :(

        }
        protected void Disconnect()
        {
            Frame disconnect = new Frame();
            disconnect.frameType = FrameType.Disconnect;
            OnFrameReceived(disconnect);
        }

        protected abstract void OnFrameReceived(Frame f);
    }
}
