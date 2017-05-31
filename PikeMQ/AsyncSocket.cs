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
            evt.SetBuffer(new byte[32], 0, 32);
            if (!sock.ReceiveAsync(evt))
            {
                System.Console.WriteLine("Data received.");
                OnDataReceived(evt.Buffer, evt.BytesTransferred);

                if (evt.BytesTransferred == 0)
                {
                    System.Console.WriteLine("Peer closed connection.");
                    terminate = true;
                }
                
                if (!terminate)
                {
                    StartReceiving();
                }
                return;
            }
            else
            {
                OnDataReceived(evt.Buffer, evt.BytesTransferred);
            }            
        }

        public void Stop()
        {
            terminate = true;
            sock.Shutdown(SocketShutdown.Both);
        }

        public virtual void Send(byte[] data)
        {
            System.Console.WriteLine("Sent data to " + sock.ToString());
            System.Console.WriteLine(BitConverter.ToString(data));
            // ToDo: Async!
            sock.Send(data);
        }

        private void ReceiveComplete(object sender, SocketAsyncEventArgs e)
        {
            System.Console.WriteLine("Data received.");
            OnDataReceived(e.Buffer, e.BytesTransferred);

            if (e.BytesTransferred == 0)
            {
                System.Console.WriteLine("Peer closed connection.");
                terminate = true;
            }

            if(!terminate)
            {
                StartReceiving();
            }
        }
    }
}
