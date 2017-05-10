using System;

namespace PikeMQ.Client
{
    public class IPikeMQClient
    {
        public delegate void OnEvent();

        public event OnEvent OnConnect;
        public event OnEvent OnDisconnect;
        public event OnEvent OnMessageReceived;

    }
}
