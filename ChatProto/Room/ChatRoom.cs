using ChatProtoDatabase;
using ChatProtoDataStruct;
using ChatProtoNetwork;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace ChatProto
{
    public class ChatRoom
    {
        public static ConcurrentDictionary<long, ChatRoom> Container = new ConcurrentDictionary<long, ChatRoom>();

        public static async Task<ChatRoomInfo> Create(User user, string title)
        {
            var chatRoomCreate = ChatProtoDb.ChatRoomCreate(user.UserInfo.UserId, title);
            await chatRoomCreate;
            var chatRoomCreateResult = chatRoomCreate.Result.First();
            if (chatRoomCreateResult == null)
                return null;
            return new ChatRoomInfo { ChatRoomId = chatRoomCreateResult.ChatroomId, Title = chatRoomCreateResult.Title };
        }

        public static async Task<ChatRoom> Join(User user, long chatRoomId)
        {
            ChatRoom chatRoom;
            if (!Container.TryGetValue(chatRoomId, out chatRoom))
            {
                var loadChatRoom = Load(chatRoomId);
                await loadChatRoom;
                chatRoom = loadChatRoom.Result;
            }
            await chatRoom.AddMember(user.UserInfo);
            chatRoom.AddSubscriber(user);
            return chatRoom;
        }

        public static ChatRoom Leave(User user, long chatRoomId)
        {
            ChatRoom chatRoom;
            if (!Container.TryGetValue(chatRoomId, out chatRoom))
            {
                return null;
            }

            chatRoom.RemoveSubscriber(user);
            chatRoom.RemoveMember(user.UserInfo);
            return chatRoom;
        }

        public static async Task<ChatRoom> Subscribe(User user, long chatRoomId)
        {
            ChatRoom chatRoom;
            if (!Container.TryGetValue(chatRoomId, out chatRoom))
            {
                var loadChatRoom = Load(chatRoomId);
                await loadChatRoom;
                chatRoom = loadChatRoom.Result;
            }

            if (chatRoom == null)
            {
                return null;
            }

            chatRoom.AddSubscriber(user);
            return chatRoom;
        }

        public static ChatRoom Unsubscribe(User user, long chatRoomId)
        {
            ChatRoom chatRoom;
            if (!Container.TryGetValue(chatRoomId, out chatRoom))
            {
                return null;
            }

            chatRoom.RemoveSubscriber(user);
            return chatRoom;
        }

        public static async Task<ChatRoom> Load(long chatRoomId)
        {
            ChatRoom chatRoom;
            using (var chatRoomInquiry = ChatProtoDb.ChatRoomInquiry(chatRoomId))
            {
                await chatRoomInquiry;
                var chatRoomInquiryResult = chatRoomInquiry.Result.First();
                if (chatRoomInquiryResult == null)
                {
                    return null;
                }
                chatRoom = new ChatRoom {
                    ChatRoomInfo = new ChatRoomInfo { ChatRoomId = chatRoomInquiryResult.ChatRoomId, Title = chatRoomInquiryResult.Title },
                    Members = new ConcurrentBag<UserInfo>(),
                    Subscribers = new ConcurrentBag<User>(),
                    ChatHistory = new ConcurrentDictionary<long, ChatInfo>()
                };
            }

            using (var chatRoomUserInquiry = ChatProtoDb.ChatRoomUserInquiry(chatRoomId))
            {
                await chatRoomUserInquiry;
                foreach (var result in chatRoomUserInquiry.Result)
                {
                    chatRoom.Members.Add(new UserInfo { UserId = result.UserId, Nickname = result.Nickname });
                }
            }

            using (var chatInfoHitoryInquiry = ChatProtoDb.ChatInquiry(chatRoom.ChatRoomInfo.ChatRoomId))
            {
                await chatInfoHitoryInquiry;
                foreach (var result in chatInfoHitoryInquiry.Result)
                {
                    chatRoom.ChatHistory.TryAdd(result.ChatId, new ChatInfo
                    {
                        ChatId = result.ChatId,
                        ChatRoomId = result.ChatRoomId,
                        UserId = result.UserId,
                        ChatText = result.ChatText,
                        ChatTime = result.ChatTime
                    });
                }
            }
            return Container.TryAdd(chatRoom.ChatRoomInfo.ChatRoomId, chatRoom) ? chatRoom : null;
        }

        public ChatRoomInfo ChatRoomInfo { get; set; }
        private ConcurrentBag<UserInfo> Members { get; set; }
        private ConcurrentBag<User> Subscribers { get; set; }
        private ConcurrentDictionary<long, ChatInfo> ChatHistory { get; set; }

        private void AddSubscriber(User user)
        {
            Subscribers.Add(user);
        }

        private void RemoveSubscriber(User user)
        {
            Subscribers.TryTake(out user);
            if (Subscribers.Count == 0)
            {
                Dispose();
            }
        }

        public List<UserInfo> GetMembers()
        {
            return Members.ToList();
        }

        private async Task<bool> AddMember(UserInfo userInfo)
        {
            var chatRoomJoin = ChatProtoDb.ChatRoomJoin(ChatRoomInfo.ChatRoomId, userInfo.UserId);
            await chatRoomJoin;
            var chatRoomJoinResult = chatRoomJoin.Result.First();
            if (chatRoomJoinResult == null)
            {
                return false;
            }

            Members.Add(userInfo);
            return true;
        }

        private async void RemoveMember(UserInfo userInfo)
        {
            var chatRoomLeave = ChatProtoDb.ChatRoomLeave(ChatRoomInfo.ChatRoomId, userInfo.UserId);
            await chatRoomLeave;
            Members.TryTake(out userInfo);
        }

        public async void Broadcast(User user, string chatText)
        {
            var chatCreate = ChatProtoDb.ChatCreate(ChatRoomInfo.ChatRoomId, user.UserInfo.UserId, chatText);
            await chatCreate;
            var chatCreateResult = chatCreate.Result.First();
            if (chatCreateResult == null)
            {
                return;
            }

            var newChat = new ChatInfo
            {
                ChatId = chatCreateResult.ChatId,
                ChatRoomId = chatCreateResult.ChatRoomId,
                UserId = chatCreateResult.UserId,
                ChatText = chatCreateResult.ChatText,
                ChatTime = chatCreateResult.ChatTime
            };
            ChatHistory.TryAdd(newChat.ChatId, newChat);
            var noti = new SN_Chat { ChatInfo = newChat };
            foreach (var subscriber in Subscribers)
            {
                subscriber.Send(noti);
            }
        }

        private void Dispose()
        {
            ChatRoom outValue;
            Container.TryRemove(ChatRoomInfo.ChatRoomId, out outValue);
        }

        public List<ChatInfo> GetChatInfoList()
        {
            return ChatHistory.Select(chatInfo => chatInfo.Value).ToList();
        }
    }
}