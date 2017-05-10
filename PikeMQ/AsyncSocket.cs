using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace PikeMQ.Core
{
    public class AsyncSocket
    {
        Socket sock;
        bool terminate = false;
        byte[] receiveBuffer = new byte[255];

        public delegate void DataReceivedDelegate(byte[] buffer, int count);

        public event DataReceivedDelegate OnDataReceived = delegate { };

        public AsyncSocket(Socket s)
        {
            sock = s;
        }

        public override string ToString()
        {
            return sock.RemoteEndPoint.ToString();
        }

        public void StartReceiving()
        {
            var evt = new SocketAsyncEventArgs();
            evt.Completed += this.ReceiveComplete;
            evt.SetBuffer(receiveBuffer, 0, 255);
            if (sock.ReceiveAsync(evt))
                return;
            
            ReceiveComplete(this, evt);            
        }

        public void Stop()
        {
            terminate = true;
            sock.Shutdown(SocketShutdown.Both);
        }

        public virtual void Send(byte[] data)
        {
            // ToDo: Async!
            sock.Send(data);
        }

        private void ReceiveComplete(object sender, SocketAsyncEventArgs e)
        {
            OnDataReceived(e.Buffer, e.BytesTransferred);
            if(!terminate)
            {
                StartReceiving();
            }
        }
    }
}
