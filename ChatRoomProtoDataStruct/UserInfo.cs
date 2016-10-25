using System;

namespace ChatProtoDataStruct
{
    [Serializable]
    public class UserInfo : ICloneable
    {
        public long UserId { get; set; }
        public string Nickname { get; set; }

        public object Clone()
        {
            var cloned = MemberwiseClone() as UserInfo;
            cloned.Nickname = cloned.Nickname as string;
            return cloned;
        }
    }
}
