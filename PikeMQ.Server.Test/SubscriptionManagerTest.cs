using System;
using Xunit;

namespace PikeMQ.Server.Test
{
    public class SubscriptionManagerTest
    {
        Core.InMemoryPeer peer;
        SubscriptionManager manager;
        bool messageReceived = false;

        public SubscriptionManagerTest()
        {
            peer = new Core.InMemoryPeer();
            manager = new SubscriptionManager();
            manager.Subscribe(peer, "foo");
            peer.MessageReceived += ((x, y) => { if (x == "foo") messageReceived = true; });
        }

        [Fact]
        public void CanSubscribe()
        {            
            manager.DispatchMessage("foo", new byte[] { 0x01 }, Core.QoS.BestEffort);
            Assert.True(messageReceived);            
        }

        [Fact]
        public void CanSubscribe_WildCard()
        {
            peer = new Core.InMemoryPeer();
            manager = new SubscriptionManager();
            bool messageReceived = false;
            manager.Subscribe(peer, "foo/*");
            peer.MessageReceived += ((x, y) => { if (x == "foo/fno") messageReceived = true; });

            manager.DispatchMessage("foo/fno", new byte[] { 0x01 }, Core.QoS.BestEffort);

            Assert.True(messageReceived);
        }

        [Fact]
        public void WildCard_NeedsFullTextAheadOfWildCard()
        {
            peer = new Core.InMemoryPeer();
            manager = new SubscriptionManager();
            bool messageReceived = false;
            manager.Subscribe(peer, "foo/*");
            peer.MessageReceived += ((x, y) => { if (x == "fo/fno") messageReceived = true; });

            manager.DispatchMessage("fo/fno", new byte[] { 0x01 }, Core.QoS.BestEffort);

            Assert.False(messageReceived);
        }

        [Fact]
        public void CanUnsubscribe()
        {
            manager.Unsubscribe(peer, "foo");
            manager.DispatchMessage("foo", new byte[] { 0x01 }, Core.QoS.BestEffort);

            Assert.False(messageReceived);
        }

        [Fact]
        public void CanHaveTwoSubscriptions()
        {
            var fnordReceived = false;
            manager.Subscribe(peer, "fnord");
            peer.MessageReceived += ((x, y) => { if (x == "fnord") fnordReceived = true; });
            manager.DispatchMessage("foo", new byte[] { 0x01 }, Core.QoS.BestEffort);
            manager.DispatchMessage("fnord", new byte[] { 0x01 }, Core.QoS.BestEffort);


            Assert.True(fnordReceived);
            Assert.True(messageReceived);
        }
    }
}
