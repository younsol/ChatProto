using System;

using NGTNetwork;

namespace ChatProtoNetwork
{
    public abstract class ChatProtoClientSession : TcpClientSession
    {
        public ChatProtoClientSession() : base(ChatProtoNetworkSerializer.Instance) { }
        
        public override void OnPacket(dynamic packet)
        {
            try
            {
                HandlePacket(packet);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Cannot Handle Packet! {packet}");
                Console.WriteLine(e.StackTrace);
            }
        }

        protected abstract void HandlePacket(SA_UserSignUp packet);
        protected abstract void HandlePacket(SA_UserSignIn packet);
        protected abstract void HandlePacket(SN_UserChatRoomInfoList packet);
        protected abstract void HandlePacket(SA_ChatRoomInfoList packet);
        protected abstract void HandlePacket(SA_ChatRoomCreate packet);
        protected abstract void HandlePacket(SA_ChatRoomJoin packet);
        protected abstract void HandlePacket(SN_ChatRoomJoin packet);
        protected abstract void HandlePacket(SA_ChatRoomLeave packet);
        protected abstract void HandlePacket(SN_ChatRoomLeave packet);
        protected abstract void HandlePacket(SA_ChatRoomInfo packet);
        protected abstract void HandlePacket(SA_ChatInfoHistory packet);
        protected abstract void HandlePacket(SN_Chat packet);
    }
}
