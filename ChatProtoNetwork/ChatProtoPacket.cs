using System;

using NGTNetwork;
using ChatProtoDataStruct;
using System.Collections.Generic;

namespace ChatProtoNetwork
{
    [Serializable]
    public abstract class AnswerPacket
    {
        public int Result { get; set; }
    }

    [Serializable]
    public class CQ_UserSignUp
    {
        public string Nickname { get; set; }
        public string Password { get; set; }
    }
    [Serializable]
    public class SA_UserSignUp : AnswerPacket
    {
        public UserInfo UserInfo { get; set; }
    }

    [Serializable]
    public class CQ_UserSignIn
    {
        public string Nickname { get; set; }
        public string Password { get; set; }
    }
    [Serializable]
    public class SA_UserSignIn : AnswerPacket
    {
        public UserInfo UserInfo { get; set; }
    }

    [Serializable]
    public class CN_UserChatRoomInfoList { }
    [Serializable]
    public class SN_UserChatRoomInfoList 
    {
        public List<ChatRoomInfo> ChatRoomInfoList { get; set; }
    }

    [Serializable]
    public class CQ_ChatRoomInfoList { }
    [Serializable]
    public class SA_ChatRoomInfoList : AnswerPacket
    {
        public List<ChatRoomInfo> ChatRoomInfoList { get; set; }
    }

    [Serializable]
    public class CQ_ChatRoomCreate 
    {
        public string Title { get; set; }
    }

    [Serializable]
    public class SA_ChatRoomCreate : AnswerPacket
    {
        public ChatRoomInfo ChatRoom { get; set; }
    }

    [Serializable]
    public class CQ_ChatRoomJoin 
    {
        public long ChatRoomId { get; set; }
    }

    [Serializable]
    public class SA_ChatRoomJoin : AnswerPacket
    {
        public ChatRoomInfo ChatRoom { get; set; }
    }

    [Serializable]
    public class SN_ChatRoomJoin 
    {
        public long ChatRoomId { get; set; }
        public UserInfo User { get; set; }
    }

    [Serializable]
    public class CQ_ChatRoomLeave 
    {
        public long ChatRoomId { get; set; }
    }

    [Serializable]
    public class SA_ChatRoomLeave : AnswerPacket
    {
        public ChatRoomInfo ChatRoomInfo { get; set; }
    }

    [Serializable]
    public class SN_ChatRoomLeave 
    {
        public long ChatRoomId { get; set; }
        public UserInfo UserInfo { get; set; }
    }

    [Serializable]
    public class CQ_ChatRoomInfo 
    {
        public long ChatRoomId { get; set; }
    }

    [Serializable]
    public class SA_ChatRoomInfo : AnswerPacket
    {
        public ChatRoomInfo ChatRoomInfo { get; set; }
        public List<UserInfo> ChatRoomUserInfoList { get; set; }
    }

    [Serializable]
    public class CQ_ChatInfoHistory 
    {
        public long ChatRoomId { get; set; }
    }

    [Serializable]
    public class SA_ChatInfoHistory : AnswerPacket
    {
        public long ChatRoomId { get; set; }
        public List<ChatInfo> ChatInfoHistory { get; set; }
    }

    [Serializable]
    public class CN_Chat 
    {
        public long ChatRoomId { get; set; }
        public string ChatText { get; set; }
    }

    [Serializable]
    public class SN_Chat 
    {
        public ChatInfo ChatInfo { get; set; }
    }

}
