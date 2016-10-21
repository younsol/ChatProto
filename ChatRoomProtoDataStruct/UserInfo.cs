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
            UserInfo cloned = new UserInfo();
            cloned.UserId = UserId;
            cloned.Nickname = Nickname.Clone() as string;
            return cloned;
        }
    }
}
