using NGTNetwork;
using System;

namespace ChatProtoNetwork
{
    public abstract class ChatServerSession : TcpServerSession<PacketSerializer>
    {
        public ChatServerSession() : base()
        {
            Serializer = PacketSerializer.Instance;
        }
        
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

        protected abstract void HandlePacket(CQ_UserSignUp packet);
        protected abstract void HandlePacket(CQ_UserSignIn packet);
        protected abstract void HandlePacket(CN_UserChatRoomInfoList packet);
        protected abstract void HandlePacket(CQ_ChatRoomInfoList packet);
        protected abstract void HandlePacket(CQ_ChatRoomCreate packet);
        protected abstract void HandlePacket(CQ_ChatRoomJoin packet);
        protected abstract void HandlePacket(CQ_ChatRoomLeave packet);
        protected abstract void HandlePacket(CQ_ChatRoomInfo packet);
        protected abstract void HandlePacket(CQ_ChatInfoHistory packet);
        protected abstract void HandlePacket(CN_Chat packet);
    }
}
