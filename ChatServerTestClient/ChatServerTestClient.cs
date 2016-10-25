using System;
using System.Threading;

using NGTUtil;

using ChatProtoDataStruct;
using ChatProtoNetwork;

namespace ChatServerTestClient
{
    public class Connection : ChatProtoClientSession
    {
        protected override void OnClose()
        {
            Console.WriteLine("Connection Defused!! ByeBye~");
            Thread.Sleep(1500);
            Environment.Exit(0);
        }

        protected override void OnConnect()
        {
            Console.WriteLine("Server Connected!!");
        }
        public override void OnPacket(dynamic packet)
        {
            try
            {
                Console.WriteLine($"Recv Packet >> {packet}");
                Console.WriteLine(StaticUtility.GetObjectContent(packet));
                HandlePacket(packet);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Cannot Handle Packet! {packet}");
                Console.WriteLine(e.StackTrace);
            }
        }


        protected void HandlePacket(SN_UserChatRoomInfoList packet)
        {
        }

        protected void HandlePacket(SA_ChatRoomCreate packet)
        {
        }

        protected void HandlePacket(SN_ChatRoomJoin packet)
        {
        }

        protected void HandlePacket(SN_ChatRoomLeave packet)
        {
        }

        protected void HandlePacket(SA_ChatInfoHistory packet)
        {
        }

        protected void HandlePacket(SN_Chat packet)
        {
        }

        protected void HandlePacket(SA_ChatRoomInfo packet)
        {
        }

        protected void HandlePacket(SA_ChatRoomLeave packet)
        {
        }

        protected void HandlePacket(SA_ChatRoomJoin packet)
        {
        }

        protected void HandlePacket(SA_ChatRoomInfoList packet)
        {
        }

        protected void HandlePacket(SA_UserSignIn packet)
        {
            if (packet.Result == 0)
                ChatServerTestClient.UserInfo = packet.UserInfo;
        }

        protected void HandlePacket(SA_UserSignUp packet)
        {
        }

    }
    class ChatServerTestClient
    {
        public static UserInfo UserInfo { get; set; }
        private Connection connection = new Connection();
        
        public void CQ_UserSignUp(string nickname, string password)
        {
            connection.Send(new CQ_UserSignUp { Nickname = nickname, Password = password });
        }
        public void CQ_UserSignIn(string nickname, string password)
        {
            connection.Send(new CQ_UserSignIn { Nickname = nickname, Password = password });
        }
        public void CN_UserChatRoomInfoList()
        {
            connection.Send(new CN_UserChatRoomInfoList());
        }
        public void CQ_ChatRoomInfoList()
        {
            connection.Send(new CQ_ChatRoomInfoList());
        }
        public void CQ_ChatRoomCreate(string title)
        {
            connection.Send(new CQ_ChatRoomCreate { Title = title });
        }
        public void CQ_ChatRoomJoin(long chatRoomId)
        {
            connection.Send(new CQ_ChatRoomJoin { ChatRoomId = chatRoomId });
        }
        public void CQ_ChatRoomLeave(long chatRoomId)
        {
            connection.Send(new CQ_ChatRoomLeave { ChatRoomId = chatRoomId });
        }
        public void CQ_ChatRoomInfo(long chatRoomId)
        {
            connection.Send(new CQ_ChatRoomInfo { ChatRoomId = chatRoomId });
        }
        public void CQ_ChatInfoHistory(long chatRoomId)
        {
            connection.Send(new CQ_ChatInfoHistory { ChatRoomId = chatRoomId });
        }
        public void CN_Chat(long chatRoomId, string chatText)
        {
            connection.Send(new CN_Chat { ChatRoomId = chatRoomId, ChatText = chatText });
        }

        static void Main(string[] args)
        {
            ChatServerTestClient client = new ChatServerTestClient();
            client.connection.Connect("localhost", 11000);
            while (true)
            {
                try
                {
                    Console.WriteLine();
                    Console.Write($"{UserInfo?.Nickname}..>");
                    var tokens = Console.ReadLine().Split(' ');
                    if (tokens.Length == 0)
                        continue;

                    switch (tokens[0])
                    {
                        case "exit":
                            Environment.Exit(0);
                            break;
                        case "signup":
                            if (tokens.Length < 3)
                            {
                                Console.WriteLine("-- Check Parameters.. --");
                                Console.WriteLine("signup [Nickname] [Password]");
                                continue;
                            }
                            client.CQ_UserSignUp(tokens[1], tokens[2]);
                            break;
                        case "signin":
                            if (tokens.Length < 3)
                            {
                                Console.WriteLine("-- Check Parameters.. --");
                                Console.WriteLine("signin [Nickname] [Password]");
                                continue;
                            }
                            client.CQ_UserSignIn(tokens[1], tokens[2]);
                            break;
                        case "myrooms":
                            client.CN_UserChatRoomInfoList();
                            break;
                        case "allrooms":
                            client.CQ_ChatRoomInfoList();
                            break;
                        case "createroom":
                            if (tokens.Length < 2)
                            {
                                Console.WriteLine("-- Check Parameters.. --");
                                Console.WriteLine("createroom [Title]");
                                continue;
                            }
                            client.CQ_ChatRoomCreate(tokens[1]);
                            break;
                        case "joinroom":
                            if (tokens.Length < 2)
                            {
                                Console.WriteLine("-- Check Parameters.. --");
                                Console.WriteLine("joinroom [ChatRoomId]");
                                continue;
                            }
                            client.CQ_ChatRoomJoin(int.Parse(tokens[1]));
                            break;
                        case "leaveroom":
                            if (tokens.Length < 2)
                            {
                                Console.WriteLine("-- Check Parameters.. --");
                                Console.WriteLine("leaveroom [ChatRoomId]");
                                continue;
                            }
                            client.CQ_ChatRoomLeave(int.Parse(tokens[1]));
                            break;
                        case "roominfo":
                            if (tokens.Length < 2)
                            {
                                Console.WriteLine("-- Check Parameters.. --");
                                Console.WriteLine("roominfo [ChatRoomId]");
                                continue;
                            }
                            client.CQ_ChatRoomInfo(int.Parse(tokens[1]));
                            break;
                        case "chathistory":
                            if (tokens.Length < 2)
                            {
                                Console.WriteLine("-- Check Parameters.. --");
                                Console.WriteLine("chathistory [ChatRoomId]");
                                continue;
                            }
                            client.CQ_ChatInfoHistory(int.Parse(tokens[1]));
                            break;
                        case "chat":
                            if (tokens.Length < 3)
                            {
                                Console.WriteLine("-- Check Parameters.. --");
                                Console.WriteLine("chat [ChatRoomId] [ChatText]");
                                continue;
                            }
                            client.CN_Chat(int.Parse(tokens[1]), tokens[2]);
                            break;
                        case "help":
                            Console.WriteLine("-- CommandList --");
                            Console.WriteLine("exit");
                            Console.WriteLine("signup [Nickname] [Password]");
                            Console.WriteLine("signin [Nickname] [Password]");
                            Console.WriteLine("myrooms");
                            Console.WriteLine("allrooms");
                            Console.WriteLine("createroom [Title]");
                            Console.WriteLine("joinroom [ChatRoomId]");
                            Console.WriteLine("leaveroom [ChatRoomId]");
                            Console.WriteLine("roominfo [ChatRoomId]");
                            Console.WriteLine("chathistory [ChatRoomId]");
                            Console.WriteLine("chat [ChatRoomId] [ChatText]");
                            break;
                        default:
                            Console.WriteLine("type 'help' for command list..");
                            break;
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }
    }
}
