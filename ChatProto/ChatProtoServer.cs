using System;
using System.Threading;

using NGTNetwork;

namespace ChatProto
{
    using ChatProtoServerListener = TcpSocketListener<User>;

    class ChatProtoServer
    {
        static void Main(string[] args)
        {
            ChatProtoServerListener listener = new ChatProtoServerListener();
            listener.StartListeningAsync(client => new User(client));
            Console.WriteLine("Start Listen!!");
            while(true)
            {
                Thread.Sleep(5000);
            }
        }
    }
}
