using System;
using System.Net.Sockets;

namespace PikeMQ.Client.App
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient();
            var tsk = client.ConnectAsync(new System.Net.IPAddress(new byte[] {127,0,0,1 }), 1984);
            tsk.Wait();
            PikeMQ.Core.AsyncSocket socket = new Core.AsyncSocket(client.Client);
            var theClient = new PikeMQ.Client.PikeMQClient(socket);
            theClient.Connect();

            theClient.SendSubscribeRequest("SomeChannel");
            theClient.PostMessage("AnotherChannel", new byte[] { 0x01, 0x02 }, Core.QoS.BestEffort);

            Console.WriteLine("Press key to stop");

            Console.ReadLine();
        }
    }
}