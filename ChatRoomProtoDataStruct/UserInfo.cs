using System;

namespace ChatProtoDataStruct
{
    [Serializable]
    public class UserInfo : ICloneable
    {
        public long UserId { get; set; }
        public string Nickname { get; set; }

        public object Clone() { return MemberwiseClone(); }
    }
}
