using NGTUtil;
using System;

namespace NGTNetwork
{
    // Don't forget to inherit Packet and specify Serializable!

    [Serializable]
    public abstract class Packet { }

    public sealed class PacketSerializer
        : Serializer
    {
        private static readonly PacketSerializer instance = new PacketSerializer();
        public static PacketSerializer Instance { get { return instance; } }

        public new byte[] Serialize(object obj)
        {
            var packet = obj as Packet;
            byte[][] byteArray = new byte[4][];
            byteArray[1] = base.Serialize(packet.GetType().FullName);
            byteArray[0] = BitConverter.GetBytes(byteArray[1].Length);
            byteArray[3] = base.Serialize(packet);
            byteArray[2] = BitConverter.GetBytes(byteArray[3].Length);
            return StaticUtility.Combine(byteArray);
        }

        public new Packet Deserialize(byte[] data)
        {
            int packetNameDataLength = BitConverter.ToInt32(data, 0);
            byte[] packetNameData = new byte[packetNameDataLength];
            Buffer.BlockCopy(data, 4, packetNameData, 0, packetNameData.Length);

            string packetName = base.Deserialize(packetNameData) as string;
            var packetType = Type.GetType(packetName);

            int packetDataLength = BitConverter.ToInt32(data, packetNameDataLength + 4);
            byte[] packetData = new byte[packetDataLength];
            Buffer.BlockCopy(data, packetNameDataLength + 8, packetData, 0, packetData.Length);

            return base.Deserialize(packetData) as Packet;
        }
    }
}
