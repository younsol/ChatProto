using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NGTUtil
{
    public abstract class Serializer
    {
        public byte[] Serialize(object obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public object Deserialize(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(ms);
            }
        }
    }
}
