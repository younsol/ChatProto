using System;
using System.Threading;
using System.Threading.Tasks;

using ChatProtoNetwork;
using NGTUtil;
using ChatProtoDataStruct;

namespace ChatServerTestClient
{
    public class Connection : ChatProtoClientSession
    {
        public Connection() : base(CommandSerializer.Instance) { }

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
                Console.WriteLine($"Recv>> {StaticUtility.GetObjectContent(packet)}");
                HandlePacket(packet);
                ChatServerTestClient.Prompt();
            }
            catch (Exception)
            {
                Console.WriteLine($"HandlePacket Not Implemented (Packet: {packet})");
                //Console.WriteLine(e.StackTrace);
            }
        }

        private void HandlePacket(SignInResponse packet)
        {
            ChatServerTestClient.UserInfo = packet.UserInfo;
        }        
    }

    class ChatServerTestClient
    {
        private string Host { get; set; }
        private int Port { get; set; }
        public Connection Connection { get; set; }
        public static UserInfo UserInfo { get; set; }

        private ChatServerTestClient(string host, int port)
        {
            Connection = new Connection();
            Host = host;
            Port = port;

            try
            {
                Connection.Connect(Host, Port);
            }
            catch (Exception)
            {
                Console.WriteLine("Connection Failure!");
                Thread.Sleep(1500);
                Environment.Exit(0);
            }
        }

        ~ChatServerTestClient()
        {
            Connection?.Close();
        }

        private static void ShowCommandList()
        {
            Console.WriteLine("-- CommandList --");
            Console.WriteLine("exit");
            Console.WriteLine("SignUp [Nickname] [Password]");
            Console.WriteLine("SignIn [Nickname] [Password]");
            Console.WriteLine("UserChatRoomInfoList");
            Console.WriteLine("ChatRoomInfoList");
            Console.WriteLine("ChatRoomCreate [Title]");
            Console.WriteLine("ChatRoomJoin [ChatRoomId]");
            Console.WriteLine("ChatRoomLeave [ChatRoomId]");
            Console.WriteLine("ChatRoomInfo [ChatRoomId]");
            Console.WriteLine("ChatInfoHistory [ChatRoomId]");
            Console.WriteLine("Chat [ChatRoomId] [ChatText]");
        }

        private static bool DoCustomCommand(string command)
        {
            switch (command)
            {
                case "exit":
                    Environment.Exit(0);
                    return true;
                case "":
                case null:
                    return true;
                default:
                    return false;
            }
        }

        public static void Prompt()
        {
            Console.Write($"{UserInfo?.Nickname}>>");
        }

        static void Main(string[] args)
        {
            ChatServerTestClient client = new ChatServerTestClient("localhost", 11000);

            while (true)
            {
                try
                {
                    Prompt();
                    var command = Console.ReadLine();
                    if (DoCustomCommand(command))
                    {
                        continue;
                    }

                    var send = client.Connection.Send(command);
                    if (!send.Result)
                    {
                        ShowCommandList();
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
