using System;
using PikeMQ.Core;
using System.Text;

namespace PikeMQ.Server.App
{
    class VerboseSubscriptionManager: SubscriptionManager
    {
        public override void Subscribe(IPeer peer, string topic)
        {
            Console.WriteLine("Peer " + peer.ToString() + " subscribed to " + topic);
            base.Subscribe(peer, topic);
        }

        public override PostResult DispatchMessage(string topic, byte[] data, QoS qos)
        {
            Console.WriteLine("A message was posted in: " + topic);
            Console.WriteLine(Encoding.UTF8.GetString(data));
            return base.DispatchMessage(topic, data, qos);
        }
    }

    class VerbosePeerManager: PeerManager
    {
        public override void RegisterPeer(IPeer thePeer)
        {
            Console.WriteLine("Peer registered: " + thePeer.ToString());
            base.RegisterPeer(thePeer);
        }

        public override void UnregisterPeer(IPeer thePeer)
        {
            Console.WriteLine("Peer left: " + thePeer.ToString());
            base.UnregisterPeer(thePeer);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            MicroIOC.IOC.Register<ISubscriptionManager>(() => new VerboseSubscriptionManager());
            PikeMQ.Server.PeerManager pm = new VerbosePeerManager();
            PikeMQ.Server.ServerSocket sock = new ServerSocket(pm);

            sock.Start();

            Console.WriteLine("Press key to stop");

            Console.ReadLine();

        }
    }
}