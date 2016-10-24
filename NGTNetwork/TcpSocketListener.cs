using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NGTNetwork
{
    public class TcpSocketListener<ISession>
        where ISession : TcpServerSession
    {
        private CancellationTokenSource cancellationTokenSource;
        private TcpListener listener;

        public bool StartListeningAsync(Func<TcpClient, ISession> sessionConstructor, IPAddress ipAddress = null, int? port = null)
        {
            if (listener != null) return false;

            cancellationTokenSource = new CancellationTokenSource();
            listener = new TcpListener(ipAddress ?? IPAddress.Any, port ?? 11000);
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

            Task t = AcceptClientsAsync(sessionConstructor);
            return true;
        }

        public void StopListening()
        {
            cancellationTokenSource.Cancel();
            listener?.Stop();
            listener = null;
        }

        private async Task AcceptClientsAsync(Func<TcpClient, ISession> sessionConstructor)
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync()
                    .ConfigureAwait(false);

                var session = sessionConstructor(client);
                session.Accept();
            }
        }
    }
}
