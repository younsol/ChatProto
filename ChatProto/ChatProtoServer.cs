using System;
using System.Threading;

using ChatServerListener = NGTNetwork.TcpSocketListener<ChatProto.User, NGTNetwork.PacketSerializer>;

namespace ChatProto
{

    class ChatProtoServer
    {
        static void Main(string[] args)
        {
            ChatServerListener listener = new ChatServerListener();
            listener.StartListeningAsync();
            Console.WriteLine("Start Listen!!");
            while(true)
            {
                Thread.Sleep(5000);
            }
        }
    }
}
