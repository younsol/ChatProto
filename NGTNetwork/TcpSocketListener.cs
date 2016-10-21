using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using NGTUtil;

namespace NGTNetwork
{
    public class TcpSocketListener<ISession, ISerializer>
        where ISession : TcpServerSession<ISerializer>, new()
        where ISerializer : Serializer
    {
        private CancellationTokenSource cancellationTokenSource;
        private TcpListener listener;
        private ulong sessionIndexer;

        public bool StartListeningAsync(IPAddress ipAddress = null, int? port = null)
        {
            if (listener != null) return false;

            cancellationTokenSource = new CancellationTokenSource();
            listener = new TcpListener(ipAddress ?? IPAddress.Any, port ?? 11000);
            sessionIndexer = 0;
            try
            {
                listener.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                StopListening();
                return false;
            }

            Task t = AcceptClientsAsync();
            return true;
        }

        public void StopListening()
        {
            cancellationTokenSource.Cancel();
            listener?.Stop();
            listener = null;
        }

        private async Task AcceptClientsAsync()
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync()
                    .ConfigureAwait(false);

                var session = new ISession();
                session.Index = ++sessionIndexer;
                session.Client = client;
                session.Accept();
            }
        }
    }
}
