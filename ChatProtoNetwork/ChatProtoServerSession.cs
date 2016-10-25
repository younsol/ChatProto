using System;
using System.Net.Sockets;

using NGTNetwork;

namespace ChatProtoNetwork
{
    public abstract class ChatProtoServerSession : TcpServerSession
    {
        public ChatProtoServerSession(TcpClient client) : base(client, ChatProtoNetworkSerializer.Instance) { }
    }
}
