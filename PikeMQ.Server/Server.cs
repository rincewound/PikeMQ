using System.Net;
using System.Net.Sockets;

namespace PikeMQ.Server
{
    public class ServerSocket
    {
        Socket sock;
        TcpListener listener;
        bool term = false;

        IPeerManager peerManager;

        public ServerSocket(IPeerManager peerMgr)
        {
            sock = new Socket(AddressFamily.InterNetworkV6,
                              SocketType.Stream,
                              ProtocolType.Tcp);
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 1984);
            peerManager = peerMgr;
        }        

        public async void Start()
        { 
            listener.Start(16);

            while(!term)
            {
                              
                var con = listener.AcceptSocketAsync();
                await con;

                var client = con.Result;

                // we have a client, dispatch it into a remote peer and
                // register the peer with the master dispatcher.
                var asyncSock = new Core.AsyncSocket(client);
                RemotePeer peer = new RemotePeer(asyncSock);
                peerManager.RegisterPeer(peer);
                peer.Run();
            }
        }
    }
}
