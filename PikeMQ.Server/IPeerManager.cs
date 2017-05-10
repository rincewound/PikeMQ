using PikeMQ.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace PikeMQ.Server
{
    public interface IPeerManager
    {
        void RegisterPeer(IPeer thePeer);
        void UnregisterPeer(IPeer thePeer);
    }
}
