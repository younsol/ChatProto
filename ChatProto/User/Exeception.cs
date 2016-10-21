using System;
using System.Runtime.Serialization;

namespace ChatProto
{
    [Serializable]
    internal class Exeception : Exception
    {
        public Exeception()
        {
        }

        public Exeception(string message) : base(message)
        {
        }

        public Exeception(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected Exeception(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}