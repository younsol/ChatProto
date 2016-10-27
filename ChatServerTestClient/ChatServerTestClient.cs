using System;
using System.Threading;

using NGTUtil;

using ChatProtoDataStruct;
using ChatProtoNetwork;
using System.Threading.Tasks;

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
                Console.WriteLine($"Recv Packet >> {packet as string}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }
    }

    class ChatServerTestClient
    {
        private Connection connection = new Connection();
        
        static void Main(string[] args)
        {
            ChatServerTestClient client = new ChatServerTestClient();
            client.connection.Connect("localhost", 11000);
            while (true)
            {
                try
                {
                    Console.WriteLine();
                    Console.Write($">>");
                    Task send = client.connection.Send(Console.ReadLine());
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }
    }
}
