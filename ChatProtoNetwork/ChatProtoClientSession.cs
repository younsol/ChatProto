using System;

using NGTNetwork;
using NGTUtil;

namespace ChatProtoNetwork
{
    public abstract class ChatProtoClientSession : TcpClientSession
    {
        public ChatProtoClientSession() : base(ChatProtoNetworkSerializer.Instance) { }
        public ChatProtoClientSession(Serializer serializer) : base(serializer) { }
    }
}
