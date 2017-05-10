using System;

namespace PikeMQ
{
    public enum FrameType: byte
    {
        // Lower Nibble Frametypes, same as MQTT
        Connect =  0x01,
        ConReply = 0x02,
        Publish = 0x03,
        PubReply = 0x04,
        Subscribe = 0x08,
        SubReply = 0x09,
        Unsub  = 0x0A,
        UnsubReply = 0x0B,
        Ping = 0x0C,
        PingAck = 0x0D,
        Disconnect = 0x0E  
        
        // Mesh Control Frames use the Upper Nibble.
    }
}
