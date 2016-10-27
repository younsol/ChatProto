using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using NGTUtil;

namespace NGTNetwork
{
    public interface ISession
    {
        Task<bool> Send(object obj);
        void Close();
        void OnPacket(dynamic packet);
    }

    public interface IServerSession
    {
        void Accept();
    }

    public interface IClientSession
    {
        void Connect(string host, int port);
    }

    public abstract class Session : ISession
    {
        private static volatile int sessionIndexer = 0;
        public readonly int Index;

        protected readonly Serializer serializer;

        public Session(Serializer serializer)
        {
            Index = Interlocked.Increment(ref sessionIndexer);
            this.serializer = serializer;
        }

        public abstract Task<bool> Send(object obj);
        public abstract void Close();
        protected abstract void OnClose();
        public abstract void OnPacket(dynamic packet);
    }

    public abstract class TcpSession : Session
    {
        private ConcurrentQueue<byte[]> writeQueue = new ConcurrentQueue<byte[]>();

        protected readonly TcpClient client;

        public TcpSession(TcpClient client, Serializer serializer) : base(serializer)
        {
            this.client = client;
        }

        // connection이 끊겼거나, Serialize 실패했을 때에만
        // Synced로 false가 return 된다.
        public override async Task<bool> Send(object packet)
        {
            var data = serializer.Serialize(packet);
            if (data == null)
                return false;

            return await WriteAsync(data);
        }

        public override void Close()
        {
            OnClose();
            client.Close();
        }

        protected async Task<bool> WriteAsync(byte[] data)
        {
            if (data == null || !client.Connected)
            {
                Close();
                return false;
            }

            byte[][] byteArrays = new byte[2][];
            byteArrays[0] = BitConverter.GetBytes(data.Length);
            byteArrays[1] = data;
            byte[] dataWithLengthPrefix = StaticUtility.Combine(byteArrays);
            int queueCount = 0;

            lock (writeQueue)
            {
                writeQueue.Enqueue(dataWithLengthPrefix);
                queueCount = writeQueue.Count;
            }

            while (queueCount == 1)
            {
                byte[] sendData;
                writeQueue.TryPeek(out sendData);
                await client.GetStream().WriteAsync(sendData, 0, sendData.Length);
                lock (writeQueue)
                {
                    writeQueue.TryDequeue(out sendData);
                    queueCount = writeQueue.Count;
                }
            }
            return true;
        }

        protected async void ReadAsync()
        {
            byte[] dataLengthData = new byte[4];
            try
            {
                while (true)
                {
                    await client.GetStream().ReadAsync(dataLengthData, 0, 4);
                    if (!client.Connected)
                    {
                        throw new Exception("Connection Closed!");
                    }
                    else
                    {
                        int dataLength = BitConverter.ToInt32(dataLengthData, 0);
                        if (dataLength == 0)
                        {
                            throw new Exception("Unexpected DataLength!");
                        }
                        else
                        {
                            byte[] data = new byte[dataLength];
                            await client.GetStream().ReadAsync(data, 0, data.Length);
                            OnPacket(serializer.Deserialize(data));
                        }
                    }
                }
            }
            catch(Exception)
            {
                Close();
            }
        }
    }

    public abstract class TcpServerSession : TcpSession, IServerSession
    {
        public TcpServerSession(TcpClient client, Serializer serializer) : base(client, serializer) { }

        public void Accept()
        {
            OnAccept();
            Task.Run(() => ReadAsync());
        }
        protected abstract void OnAccept();
    }

    public abstract class TcpClientSession : TcpSession, IClientSession
    {
        public TcpClientSession(Serializer serializer) : base(new TcpClient(), serializer) { }

        public void Connect(string hostname, int port)
        {
            client.Connect(hostname, port);
            OnConnect();
            Task.Run(() => ReadAsync());
        }

        public async void ConnectAsync(string host, int port)
        {
            await client.ConnectAsync(host, port);
            OnConnect();
            Task t = Task.Run(() => ReadAsync());
        }

        protected abstract void OnConnect();
    }
}