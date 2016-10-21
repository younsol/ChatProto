using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace NGTUtil
{
    public static class StaticUtility
    {
        public static SHA256 Sha256 = new SHA256Managed();

        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        public static T[] Combine<T>(T[] front, T[] back)
        {
            return front.Concat(back).ToArray();
        }

        public static string Sha256Crypt(string text)
        {
            return Encoding.Default.GetString(Sha256.ComputeHash(Encoding.ASCII.GetBytes(text)));
        }

        public static string GetObjectContent(object obj)
        {
            if (obj == null)
                return "null";

            if (obj.GetType().IsPrimitive)
            {
                return $"{obj.ToString()}";
            }

            if (obj.GetType() == typeof(string))
            {
                return $"'{obj as string}'";
            }

            if (obj.GetType() == typeof(DateTime))
            {
                return $"'{obj.ToString()}'";
            }

            StringBuilder sb = new StringBuilder();
            if (obj is IEnumerable)
            {
                var enumerable = obj as IEnumerable;
                sb.Append($"{{{Environment.NewLine}");
                foreach (var item in enumerable)
                {
                    sb.Append($"{GetObjectContent(item)},{Environment.NewLine}");
                }
                sb.Append($"}}");
            }
            else
            {
                var reflection = obj.GetType().GetProperties();
                foreach (PropertyInfo pi in reflection)
                {
                    sb.Append($"[{pi.Name}:{GetObjectContent(pi.GetValue(obj, null))}]{Environment.NewLine}");
                }
            }
            return sb.ToString();
        }
    }
}
