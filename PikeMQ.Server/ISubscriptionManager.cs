using PikeMQ.Core;

namespace PikeMQ.Server
{
    public interface ISubscriptionManager
    {
        PostResult DispatchMessage(string topic, byte[] data, QoS qos);
        void Subscribe(IPeer peer, string topic);
        void Unsubscribe(IPeer peer, string topic);
        void UnsubscribeAll(IPeer peer);
    }
}