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

        protected override async void HandlePacket(CQ_UserSignUp packet)
        {
            var response = new SA_UserSignUp();
            response.Result = -1;

            if (UserInfo != null)
            {
                Send(response);
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

            Send(response);
        }

        protected override async void HandlePacket(CQ_UserSignIn packet)
        {
            var response = new SA_UserSignIn();
            response.Result = -1;

            if (UserInfo != null)
            {
                Send(response);
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

                var noti = new SN_UserChatRoomInfoList();

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
                    Send(noti);
                }


                if (!UserContainer.TryAdd(UserInfo.UserId, this))
                {
                    Console.WriteLine($"Fatal Error occured on Adding User!! [UserId: {UserInfo.UserId}]");
                }

                response.Result = 0;
                Send(response);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Close();
                return;
            }
        }

        protected override void HandlePacket(CN_UserChatRoomInfoList packet)
        {
            if (UserInfo == null)
            {
                return;
            }

            try
            {
                var noti = new SN_UserChatRoomInfoList();
                noti.ChatRoomInfoList = JoinedChatRooms.Select(pair => pair.Value.ChatRoomInfo).ToList();
                Send(noti);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Close();
                return;
            }
        }

        protected override async void HandlePacket(CQ_ChatRoomInfoList packet)
        {
            var response = new SA_ChatRoomInfoList();
            response.Result = -1;

            if (UserInfo == null)
            {
                Send(response);
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
                    response.ChatRoomInfoList = chatRoomListInquiry.Result;
                }
                response.Result = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Close();
                return;
            }

            Send(response);
        }

        protected override async void HandlePacket(CQ_ChatRoomCreate packet)
        {
            var response = new SA_ChatRoomCreate();
            response.Result = -1;

            if (UserInfo == null)
            {
                Send(response);
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
                Send(response);
            }
        }

        protected override async void HandlePacket(CQ_ChatRoomJoin packet)
        {
            var response = new SA_ChatRoomJoin();
            response.Result = -1;

            if (UserInfo == null)
            {
                Send(response);
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
                Send(response);
            }
        }

        protected override void HandlePacket(CQ_ChatRoomLeave packet)
        {
            var response = new SA_ChatRoomLeave();
            response.Result = -1;

            if (UserInfo == null)
            {
                Send(response);
                return;
            }

            try
            {
                var chatRoom = ChatRoom.Leave(this, packet.ChatRoomId);
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
                Send(response);
            }
        }

        protected override void HandlePacket(CQ_ChatRoomInfo packet)
        {
            var response = new SA_ChatRoomInfo();
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
                Send(response);
            }
        }

        protected override void HandlePacket(CQ_ChatInfoHistory packet)
        {
            var response = new SA_ChatInfoHistory();
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
                Send(response);
            }
        }

        protected override void HandlePacket(CN_Chat packet)
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

                var noti = new SN_Chat();
                targetChatRoom.Broadcast(this, packet.ChatText);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
