using PikeMQ.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PikeMQ.Server
{
    public class SubscriptionManager : ISubscriptionManager
    {
        class SubscriptionList
        {
            public Core.IPeer thePeer;
            public List<string> activeSubscriptions = new List<string>();
        }

        List<SubscriptionList> allSubscriptions = new List<SubscriptionList>();

        public virtual void Subscribe(Core.IPeer peer, string topic)
        {
            var existingSub = allSubscriptions.FirstOrDefault(x => x.thePeer == peer);
            if(existingSub == null)
            {
                existingSub = new SubscriptionList();
                existingSub.thePeer = peer;
                allSubscriptions.Add(existingSub);
            }

            existingSub.activeSubscriptions.Add(topic);
        }

        public void Unsubscribe(Core.IPeer peer, string topic)
        {
            var existingSub = allSubscriptions.FirstOrDefault(x => x.thePeer == peer);
            if (existingSub == null)
            {
                return;
            }

            existingSub.activeSubscriptions.Remove(topic);
        }

        public virtual PostResult DispatchMessage(string topic, byte[] data, QoS qos)
        {
            var subscribers = allSubscriptions.Where(x => x.activeSubscriptions.Any(y => topic.MatchSubscription(y)));
            var tsk = Task.WhenAll(subscribers.Select(x => x.thePeer.PostMessage(topic, data, qos)));

            if (qos != QoS.GuaranteedDelivery)
                return PostResult.Dispatched;

            // We have guaranteed delivery, so we have to wait and see if at least
            // one task finishes with a good result.
            tsk.Wait();

            var good = tsk.Result.Any(x => x == PostResult.Delivered);

            return good ? PostResult.Delivered
                        : PostResult.DeliveryError;            
        }

        public void UnsubscribeAll(IPeer peer)
        {
            allSubscriptions = allSubscriptions.Where(x => x.thePeer != peer).ToList();
        }
    }
}
