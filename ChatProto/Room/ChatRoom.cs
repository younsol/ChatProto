using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ChatProtoDatabase;
using ChatProtoDataStruct;
using ChatProtoNetwork;

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

        public static async Task<ChatRoom> Leave(User user, long chatRoomId)
        {
            ChatRoom chatRoom;
            if (!Container.TryGetValue(chatRoomId, out chatRoom))
            {
                return null;
            }

            chatRoom.RemoveSubscriber(user);
            await chatRoom.RemoveMember(user.UserInfo);
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
                    Members = new ConcurrentDictionary<long, UserInfo>(),
                    Subscribers = new ConcurrentDictionary<long, User>(),
                    ChatHistory = new ConcurrentDictionary<long, ChatInfo>()
                };
            }

            using (var chatRoomUserInquiry = new ChatRoomUserInquiry { ChatRoomId = chatRoomId })
            using (var chatRoomUserInquiryExecute = chatRoomUserInquiry.ExecuteAsync(ChatProtoSqlServer.Instance))
            {
                await chatRoomUserInquiryExecute;
                foreach (var userInfo in chatRoomUserInquiry.Result)
                {
                    chatRoom.Members.TryAdd(userInfo.UserId, userInfo);
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
        private ConcurrentDictionary<long, UserInfo> Members { get; set; }
        private ConcurrentDictionary<long, User> Subscribers { get; set; }
        private ConcurrentDictionary<long, ChatInfo> ChatHistory { get; set; }

        private bool AddSubscriber(User user)
        {
            return Subscribers.TryAdd(user.UserInfo.UserId, user);
        }

        private bool RemoveSubscriber(User user)
        {
            bool success = Subscribers.TryRemove(user.UserInfo.UserId, out user);
            if (Subscribers.Count == 0)
            {
                Dispose();
            }
            return success;
        }

        public List<UserInfo> GetMembers()
        {
            return Members.Select(pair => pair.Value).ToList();
        }

        private async Task<bool> AddMember(UserInfo userInfo)
        {
            using (var chatRoomJoin = new ChatRoomJoin { ChatRoomId = ChatRoomInfo.ChatRoomId, UserId = userInfo.UserId })
            using (var chatRoomJoinExecute = chatRoomJoin.ExecuteAsync(ChatProtoSqlServer.Instance))
            {
                await chatRoomJoinExecute;
                var chatRoomJoinResult = chatRoomJoin.Result.First();
                return (chatRoomJoinResult == null || chatRoomJoinResult.Result < 0) && Members.TryAdd(userInfo.UserId, userInfo);
            }
        }

        private async Task<bool> RemoveMember(UserInfo userInfo)
        {
            using (var chatRoomLeave = new ChatRoomLeave { ChatRoomId = ChatRoomInfo.ChatRoomId, UserId = userInfo.UserId })
            using (var chatRoomLeaveExecute = chatRoomLeave.ExecuteAsync(ChatProtoSqlServer.Instance))
            {
                await chatRoomLeaveExecute;
                var chatRoomLeaveResult = chatRoomLeave.Result.FirstOrDefault();
                return (chatRoomLeaveResult == null || chatRoomLeaveResult.Result < 0) && Members.TryRemove(userInfo.UserId, out userInfo);
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
                    var noti = new ChatProtoNetwork.Chat { ChatInfo = chatInfo };
                    foreach (var pair in Subscribers)
                    {
                        await pair.Value.Send(noti);
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