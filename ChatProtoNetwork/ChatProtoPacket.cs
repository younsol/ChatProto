using System;
using System.Collections.Generic;

using ChatProtoDataStruct;

namespace ChatProtoNetwork
{
    [Serializable]
    public abstract class ResponsePacket
    {
        public int Result { get; set; }
    }

    [Serializable]
    public class SignUpRequest
    {
        public string Nickname { get; set; }
        public string Password { get; set; }
    }
    [Serializable]
    public class SignUpResponse : ResponsePacket
    {
        public UserInfo UserInfo { get; set; }
    }

    [Serializable]
    public class SignInRequest
    {
        public string Nickname { get; set; }
        public string Password { get; set; }
    }
    [Serializable]
    public class SignInResponse : ResponsePacket
    {
        public UserInfo UserInfo { get; set; }
    }

    [Serializable]
    public class UserChatRoomInfoListRequest { }
    [Serializable]
    public class UserChatRoomInfoList
    {
        public List<ChatRoomInfo> ChatRoomInfoList { get; set; }
    }

    [Serializable]
    public class ChatRoomInfoListRequest { }
    [Serializable]
    public class ChatRoomInfoListResponse : ResponsePacket
    {
        public List<ChatRoomInfo> ChatRoomInfoList { get; set; }
    }

    [Serializable]
    public class ChatRoomCreateRequest
    {
        public string Title { get; set; }
    }

    [Serializable]
    public class ChatRoomCreateResponse : ResponsePacket
    {
        public ChatRoomInfo ChatRoom { get; set; }
    }

    [Serializable]
    public class ChatRoomJoinRequest
    {
        public long ChatRoomId { get; set; }
    }

    [Serializable]
    public class ChatRoomJoinResponse : ResponsePacket
    {
        public ChatRoomInfo ChatRoom { get; set; }
    }

    [Serializable]
    public class ChatRoomJoinNotify
    {
        public long ChatRoomId { get; set; }
        public UserInfo User { get; set; }
    }

    [Serializable]
    public class ChatRoomLeaveRequest
    {
        public long ChatRoomId { get; set; }
    }

    [Serializable]
    public class ChatRoomLeaveResponse : ResponsePacket
    {
        public ChatRoomInfo ChatRoomInfo { get; set; }
    }

    [Serializable]
    public class ChatRoomLeaveNotify
    {
        public long ChatRoomId { get; set; }
        public UserInfo UserInfo { get; set; }
    }

    [Serializable]
    public class ChatRoomInfoRequest
    {
        public long ChatRoomId { get; set; }
    }

    [Serializable]
    public class ChatRoomInfoResponse : ResponsePacket
    {
        public ChatRoomInfo ChatRoomInfo { get; set; }
        public List<UserInfo> ChatRoomUserInfoList { get; set; }
    }

    [Serializable]
    public class ChatInfoHistoryRequest
    {
        public long ChatRoomId { get; set; }
    }

    [Serializable]
    public class ChatInfoHistoryResponse : ResponsePacket
    {
        public long ChatRoomId { get; set; }
        public List<ChatInfo> ChatInfoHistory { get; set; }
    }

    [Serializable]
    public class ChatRequest
    {
        public long ChatRoomId { get; set; }
        public string ChatText { get; set; }
    }

    [Serializable]
    public class Chat
    {
        public ChatInfo ChatInfo { get; set; }
    }

}
