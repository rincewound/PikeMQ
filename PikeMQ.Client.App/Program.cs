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
            theClient.OnMessageReceived += (x, y) => System.Console.WriteLine("Received event in channel " + x + ", data: " + System.Text.Encoding.UTF8.GetString(y));
            theClient.Run();
            theClient.Connect();            
            theClient.SendSubscribeRequest("SomeChannel");
            theClient.PostMessage("AnotherChannel", new byte[] { 0xAA, 0xFF }, Core.QoS.BestEffort);

            Console.WriteLine("Press key to stop");

            Console.ReadLine();
            while(true)
            {
                Console.WriteLine("#");
                var task = theClient.PostMessage("AnotherChannel", new byte[] { 0xBB, 0xCC }, Core.QoS.BestEffort);
                task.RunSynchronously();
                System.Threading.Thread.Sleep(50);
            }


        }
    }
}