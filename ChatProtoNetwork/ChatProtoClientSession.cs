using System;

using NGTNetwork;

namespace ChatProtoNetwork
{
    public abstract class ChatProtoClientSession : TcpClientSession
    {
        public ChatProtoClientSession() : base(ChatProtoNetworkSerializer.Instance) { }
    }
}
