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
            using (var chatRoomCreate = new ChatRoomCreate { UserId = user.UserInfo.UserId, Title = title })
            using (var chatRoomCreateExecute = chatRoomCreate.ExecuteAsync(ChatProtoSqlServer.Instance))
            {
                await chatRoomCreateExecute;
                return chatRoomCreate.Result.FirstOrDefault();
            }
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
            using (var chatRoomInquiry = new ChatRoomInquiry { ChatRoomId = chatRoomId })
            using (var chatRoomInquiryExecute = chatRoomInquiry.ExecuteAsync(ChatProtoSqlServer.Instance))
            {
                await chatRoomInquiryExecute;
                var chatRoomInquiryResult = chatRoomInquiry.Result.First();
                if (chatRoomInquiryResult == null)
                {
                    return null;
                }

                chatRoom = new ChatRoom
                {
                    ChatRoomInfo = chatRoomInquiryResult,
                    Members = new ConcurrentBag<UserInfo>(),
                    Subscribers = new ConcurrentBag<User>(),
                    ChatHistory = new ConcurrentDictionary<long, ChatInfo>()
                };
            }

            using (var chatRoomUserInquiry = new ChatRoomUserInquiry { ChatRoomId = chatRoomId })
            using (var chatRoomUserInquiryExecute = chatRoomUserInquiry.ExecuteAsync(ChatProtoSqlServer.Instance))
            {
                await chatRoomUserInquiryExecute;
                foreach (var userInfo in chatRoomUserInquiry.Result)
                {
                    chatRoom.Members.Add(userInfo);
                }
            }

            using (var chatInquiry = new ChatInquiry { ChatRoomId = chatRoom.ChatRoomInfo.ChatRoomId, Count = 100 })
            using (var chatInquiryExecute = chatInquiry.ExecuteAsync(ChatProtoSqlServer.Instance))
            {
                await chatInquiryExecute;
                foreach (var result in chatInquiry.Result)
                {
                    chatRoom.ChatHistory.TryAdd(result.ChatId, result);
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
            using (var chatRoomJoin = new ChatRoomJoin { ChatRoomId = ChatRoomInfo.ChatRoomId, UserId = userInfo.UserId })
            using (var chatRoomJoinExecute = chatRoomJoin.ExecuteAsync(ChatProtoSqlServer.Instance))
            {
                await chatRoomJoinExecute;
                var chatRoomJoinResult = chatRoomJoin.Result.First();
                if (chatRoomJoinResult == null)
                {
                    return false;
                }
                Members.Add(userInfo);
                return true;
            }
        }

        private async void RemoveMember(UserInfo userInfo)
        {
            using (var chatRoomLeave = new ChatRoomLeave { ChatRoomId = ChatRoomInfo.ChatRoomId, UserId = userInfo.UserId })
            using (var chatRoomLeaveExecute = chatRoomLeave.ExecuteAsync(ChatProtoSqlServer.Instance))
            {
                await chatRoomLeaveExecute;
                Members.TryTake(out userInfo);
            }
        }

        public async void Broadcast(User user, string chatText)
        {
            using (var chatCreate = new ChatCreate { ChatRoomId = ChatRoomInfo.ChatRoomId, UserId = user.UserInfo.UserId, ChatText = chatText })
            using (var chatCreateExecute = chatCreate.ExecuteAsync(ChatProtoSqlServer.Instance))
            {
                await chatCreateExecute;
                if (!chatCreateExecute.Result)
                {
                    return;
                }

                foreach (var chatInfo in chatCreate.Result)
                {
                    ChatHistory.TryAdd(chatInfo.ChatId, chatInfo);
                    var noti = new SN_Chat { ChatInfo = chatInfo };
                    foreach (var subscriber in Subscribers)
                    {
                        subscriber.Send(noti);
                    }
                }
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