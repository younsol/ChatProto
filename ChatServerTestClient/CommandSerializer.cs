using System;
using System.Reflection;

using ChatProtoNetwork;
using NGTUtil;

namespace ChatServerTestClient
{
    public class CommandSerializer : Serializer
    {
        public static CommandSerializer Instance = new CommandSerializer();

        public override byte[] Serialize(object command)
        {
            var tokens = (command as string).Split(' ');
            if (tokens.Length == 0)
                return null;
            try
            {
                var packetType = Type.GetType($"ChatProtoNetwork.{tokens[0]}Request, ChatProtoNetwork");
                if (packetType == null)
                    return null;

                object obj = Activator.CreateInstance(packetType);

                if (obj == null)
                    return null;

                var properties = packetType.GetProperties();
                if (tokens.Length <= properties.Length)
                    return null;

                int count = 0;
                foreach (PropertyInfo pi in properties)
                {
                    pi.SetValue(obj, Convert.ChangeType(tokens[++count], pi.PropertyType));
                }

                return base.Serialize(obj);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            return null;
        }        
    }
}
