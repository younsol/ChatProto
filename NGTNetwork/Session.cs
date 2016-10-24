using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;

using NGTUtil;

namespace NGTNetwork
{
    public sealed class NetworkSerializer : Serializer
    {
        public static NetworkSerializer Instance = new NetworkSerializer();
    }

    public interface Session
    {
        bool Send(object obj);
        void Close();
        void OnPacket(dynamic packet);
    }

    public interface ServerSession
    {
        void Accept();
    }

    public interface ClientSession
    {
        void Connect(string host, int port);
    }

    public abstract class Session<ISerializer> : Session
        where ISerializer : Serializer
    {
        private ulong? index;
        public ulong? Index
        {
            get { return index; }
            set { index = index ?? value; }
        }

        private ISerializer serializer;
        public ISerializer Serializer
        {
            protected get { return serializer; }
            set { serializer = serializer ?? value; }
        }

        public abstract bool Send(object obj);
        public abstract void Close();
        protected abstract void OnClose();
        public abstract void OnPacket(dynamic packet);
    }

    public abstract class TcpSession<ISerializer>
        : Session<ISerializer>
        where ISerializer : Serializer
    {
        private ConcurrentQueue<byte[]> writeQueue = new ConcurrentQueue<byte[]>();

        private TcpClient client;
        public TcpClient Client
        {
            protected get { return client; }
            set { client = client ?? value; }
        }

        // connection이 끊겼거나, Serialize 실패했을 때에만
        // Synced로 false가 return 된다.
        public override bool Send(object packet)
        {
            return WriteAsync(Serializer.Serialize(packet)).Result;
        }

        public override void Close()
        {
            OnClose();
            Client.Close();
        }

        protected async Task<bool> WriteAsync(byte[] data)
        {
            if (data == null || !Client.Connected)
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
                await Client.GetStream().WriteAsync(sendData, 0, sendData.Length);
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
                await Client.GetStream().ReadAsync(dataLengthData, 0, 4);
                if (!Client.Connected)
                {
                    Close();
                }
                else
                {
                    int dataLength = BitConverter.ToInt32(dataLengthData, 0);
                    if (dataLength == 0)
                    {
                        Close();
                    }
                    else
                    {
                        byte[] data = new byte[dataLength];
                        Client.GetStream().Read(data, 0, data.Length);

                        ReadAsync();
                        OnPacket(Serializer.Deserialize(data));
                    }
                }
            }
            catch(Exception)
            {
                Close();
            }
        }
    }

    public abstract class TcpServerSession<ISerializer>
        : TcpSession<ISerializer>, ServerSession
        where ISerializer : Serializer
    {
        public void Accept()
        {
            OnAccept();
            Task.Run(() => ReadAsync());
        }
        protected abstract void OnAccept();
    }

    public abstract class TcpClientSession<ISerializer>
        : TcpSession<ISerializer>, ClientSession
        where ISerializer : Serializer
    {
        public TcpClientSession() : base()
        {
            Client = new TcpClient();
        }

        public void Connect(string hostname, int port)
        {
            Client.Connect(hostname, port);
            OnConnect();
            Task.Run(() => ReadAsync());
        }

        public async void ConnectAsync(string host, int port)
        {
            await Client.ConnectAsync(host, port);
            OnConnect();
            Task t = Task.Run(() => ReadAsync());
        }

        protected abstract void OnConnect();
    }
}