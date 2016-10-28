using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;

using NGTUtil;

using ChatProtoDatabase;
using ChatProtoDataStruct;
using ChatProtoNetwork;
using System.Threading.Tasks;

namespace ChatProto
{
    public class User : ChatProtoServerSession
    {
        public static ConcurrentDictionary<int, User> SessionContainer = new ConcurrentDictionary<int, User>();
        public static ConcurrentDictionary<long, User> UserContainer = new ConcurrentDictionary<long, User>();
        private ConcurrentDictionary<long, ChatRoom> JoinedChatRooms = new ConcurrentDictionary<long, ChatRoom>();

        public UserInfo UserInfo { get; set; }

        public User(TcpClient client) : base(client) { }

        protected override void OnAccept()
        {
            if (!SessionContainer.TryAdd(Index, this))
            {
                Console.WriteLine($"Fatal Error occured on Adding Session!! [SessionIndex: {Index}]");
            }
        }

        protected override void OnClose()
        {
            foreach (var pair in JoinedChatRooms)
            {
                ChatRoom.Unsubscribe(this, pair.Key);
            }
            JoinedChatRooms.Clear();

            User user;
            if (!SessionContainer.TryRemove(Index, out user) || this != user)
            {
                Console.WriteLine($"Fatal Error occured on Removing Session!! [SessionIndex: {Index}]");
            }

            if (UserInfo != null && (!UserContainer.TryRemove(UserInfo.UserId, out user) || this != user))
            {
                Console.WriteLine($"Fatal Error occured on Removing User!! [UserId: {UserInfo.UserId}]");
            }
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

        protected async void HandlePacket(SignUpRequest packet)
        {
            var response = new SignUpResponse();
            response.Result = -1;

            if (UserInfo != null)
            {
                await Send(response);
                return;
            }
            
            try
            {
                using (var userCreate = new UserCreate { Nickname = packet.Nickname, Password = StaticUtility.Sha256Crypt(packet.Password) })
                using (var userCreateExecute = userCreate.ExecuteAsync(ChatProtoSqlServer.Instance))
                {
                    await userCreateExecute;
                    var userCreateResult = userCreate.Result.FirstOrDefault();
                    if (userCreateResult == null)
                    {
                        throw new Exception("User Inquiry Failure!");
                    }
                    response.UserInfo = userCreateResult;
                    response.Result = 0;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Close();
                return;
            }

            await Send(response);
        }

        protected async void HandlePacket(SignInRequest packet)
        {
            var response = new SignInResponse();
            response.Result = -1;

            if (UserInfo != null)
            {
                await Send(response);
                return;
            }

            try
            {
                using (var userInquiry = new UserInquiry { Nickname = packet.Nickname, Password = StaticUtility.Sha256Crypt(packet.Password) })
                using (var userInquiryExecute = userInquiry.ExecuteAsync(ChatProtoSqlServer.Instance))
                {
                    await userInquiryExecute;
                    var userInquiryResult = userInquiry.Result.First();
                    if (userInquiryResult == null)
                    {
                        throw new Exception("User Inquiry Failure!");
                    }

                    UserInfo = userInquiryResult;
                    response.UserInfo = UserInfo.Clone() as UserInfo;
                }

                var noti = new UserChatRoomInfoList();

                using (var userChatRoomListInquiry = new UserChatRoomListInquiry { UserId = UserInfo.UserId })
                using (var userChatRoomListInquiryExecute = userChatRoomListInquiry.ExecuteAsync(ChatProtoSqlServer.Instance))
                {
                    await userChatRoomListInquiryExecute;
                    if (!userChatRoomListInquiryExecute.Result)
                    {
                        throw new Exception("UserChatRoomListInquiry Failure!");
                    }
                    noti.ChatRoomInfoList = new List<ChatRoomInfo>();
                    foreach (var result in userChatRoomListInquiry.Result)
                    {
                        var subscribeChatRoom = ChatRoom.Subscribe(this, result.ChatRoomId);
                        await subscribeChatRoom;
                        JoinedChatRooms.TryAdd(subscribeChatRoom.Result.ChatRoomInfo.ChatRoomId, subscribeChatRoom.Result);
                        noti.ChatRoomInfoList.Add(subscribeChatRoom.Result.ChatRoomInfo.Clone() as ChatRoomInfo);
                    }
                    await Send(noti);
                }


                if (!UserContainer.TryAdd(UserInfo.UserId, this))
                {
                    Console.WriteLine($"Fatal Error occured on Adding User!! [UserId: {UserInfo.UserId}]");
                }

                response.Result = 0;
                await Send(response);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Close();
                return;
            }
        }

        protected async void HandlePacket(UserChatRoomInfoListRequest packet)
        {
            if (UserInfo == null)
            {
                return;
            }

            try
            {
                var noti = new UserChatRoomInfoList();
                noti.ChatRoomInfoList = JoinedChatRooms.Select(pair => pair.Value.ChatRoomInfo).ToList();
                await Send(noti);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Close();
                return;
            }
        }

        protected async void HandlePacket(ChatRoomInfoListRequest packet)
        {
            var response = new ChatRoomInfoListResponse();
            response.Result = -1;

            if (UserInfo == null)
            {
                await Send(response);
                return;
            }

            try
            {

                using (var chatRoomListInquiry = new ChatRoomListInquiry())
                using (var chatRoomListInquiryExecute = chatRoomListInquiry.ExecuteAsync(ChatProtoSqlServer.Instance))
                {
                    await chatRoomListInquiryExecute;
                    if (!chatRoomListInquiryExecute.Result)
                    {
                        throw new Exception("ChatRoomListInquiry Failure!");
                    }
                    response.ChatRoomInfoList = chatRoomListInquiry.Result.ToList();
                }
                response.Result = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Close();
                return;
            }

            await Send(response);
        }

        protected async void HandlePacket(ChatRoomCreateRequest packet)
        {
            var response = new ChatRoomCreateResponse();
            response.Result = -1;

            if (UserInfo == null)
            {
                await Send(response);
                return;
            }

            try
            {
                var chatRoomCreate = ChatRoom.Create(this, packet.Title);
                await chatRoomCreate;
                if(chatRoomCreate.Result == null)
                {
                    throw new Exception("Create ChatRoom Failure!!");
                }

                response.ChatRoom = chatRoomCreate.Result;
                response.Result = 0;

                var chatRoomJoin = ChatRoom.Join(this, chatRoomCreate.Result.ChatRoomId);
                await chatRoomJoin;
                JoinedChatRooms.TryAdd(chatRoomJoin.Result.ChatRoomInfo.ChatRoomId, chatRoomJoin.Result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                await Send(response);
            }
        }

        protected async void HandlePacket(ChatRoomJoinRequest packet)
        {
            var response = new ChatRoomJoinResponse();
            response.Result = -1;

            if (UserInfo == null)
            {
                Task send = Send(response);
                return;
            }

            try
            {
                var chatRoomJoin = ChatRoom.Join(this, packet.ChatRoomId);
                await chatRoomJoin;
                JoinedChatRooms.TryAdd(chatRoomJoin.Result.ChatRoomInfo.ChatRoomId, chatRoomJoin.Result);
                response.ChatRoom = chatRoomJoin.Result.ChatRoomInfo.Clone() as ChatRoomInfo;
                response.Result = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                await Send(response);
            }
        }

        protected async void HandlePacket(ChatRoomLeaveRequest packet)
        {
            var response = new ChatRoomLeaveResponse();
            response.Result = -1;

            if (UserInfo == null)
            {
                await Send(response);
                return;
            }

            try
            {
                var chatRoomLeave = ChatRoom.Leave(this, packet.ChatRoomId);
                await chatRoomLeave;
                var chatRoom = chatRoomLeave.Result;
                JoinedChatRooms.TryRemove(chatRoom.ChatRoomInfo.ChatRoomId, out chatRoom);
                response.ChatRoomInfo = chatRoom.ChatRoomInfo;
                response.Result = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                await Send(response);
            }
        }

        protected async void HandlePacket(ChatRoomInfoRequest packet)
        {
            var response = new ChatRoomInfoResponse();
            response.Result = -1;
            
            if (UserInfo == null)
            {
                response.ChatRoomInfo = new ChatRoomInfo();
                response.ChatRoomInfo.ChatRoomId = packet.ChatRoomId;
                Send(response);
                return;
            }

            try
            {
                var targetChatRoom = JoinedChatRooms.Where(pair => pair.Value.ChatRoomInfo.ChatRoomId == packet.ChatRoomId).FirstOrDefault().Value;
                if (targetChatRoom == null)
                {
                    throw new Exception("Cannot Find Joined Chat Room!!");
                }
                response.ChatRoomInfo = targetChatRoom.ChatRoomInfo.Clone() as ChatRoomInfo;
                response.ChatRoomUserInfoList = targetChatRoom.GetMembers();
                response.Result = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                await Send(response);
            }
        }

        protected async void HandlePacket(ChatInfoHistoryRequest packet)
        {
            var response = new ChatInfoHistoryResponse();
            response.ChatRoomId = packet.ChatRoomId;
            response.Result = -1;

            if (UserInfo == null)
            {
                Send(response);
                return;
            }

            try
            {
                var targetChatRoom = JoinedChatRooms.Where(pair => pair.Value.ChatRoomInfo.ChatRoomId == response.ChatRoomId).FirstOrDefault().Value;
                if (targetChatRoom == null)
                {
                    throw new Exception("Cannot Find Joined Chat Room!!");
                }
                response.ChatInfoHistory = targetChatRoom.GetChatInfoList();
                response.Result = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                await Send(response);
            }
        }

        protected void HandlePacket(ChatRequest packet)
        {
            if (UserInfo == null)
            {
                return;
            }

            try
            {
                var targetChatRoom = JoinedChatRooms.Where(pair => pair.Value.ChatRoomInfo.ChatRoomId == packet.ChatRoomId).FirstOrDefault().Value;
                if (targetChatRoom == null)
                {
                    throw new Exception("Cannot Find Joined Chat Room!!");
                }

                var noti = new ChatProtoNetwork.Chat();
                targetChatRoom.Chat(this, packet.ChatText);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
