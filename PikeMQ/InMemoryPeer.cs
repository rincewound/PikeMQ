using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PikeMQ.Core.StatusCodes;

namespace PikeMQ.Core
{
    public class InMemoryPeer : IPeer
    {
        public delegate void OnMessage(string topic, byte[] data);

        public event OnMessage MessageReceived;

        public InMemoryPeer()
        {
            MessageReceived += delegate { };
        }

#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        public async Task<PostResult> PostMessage(string topic, byte[] data, QoS qos)
#pragma warning restore CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        {
            MessageReceived(topic, data);
            return PostResult.Ok;
        }

        public void SetFrameReceivedCb(FrameReceived.FrameReceivedDelegate frd)
        {
            //throw new NotImplementedException();
        }

        public void SendSubscribeReply(string channel, byte status)
        {
            //throw new NotImplementedException();
        }

        public void SendConnectionReply(byte status)
        {
            //throw new NotImplementedException();
        }

        public void SendSubscribeReply(string channel, SubscribeStatus status)
        {
            //throw new NotImplementedException();
        }

        public void SendConnectionReply(ConnectionAttemptStatus status)
        {
            //throw new NotImplementedException();
        }

        public void SendUnsubReply(string channel)
        {
            //throw new NotImplementedException();
        }
    }
}
