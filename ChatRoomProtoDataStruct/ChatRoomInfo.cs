using System;

namespace ChatProtoDataStruct
{
    [Serializable]
    public class ChatRoomInfo : ICloneable
    {
        public long ChatRoomId { get; set; }
        public string Title { get; set; }

        public object Clone()
        {
            var cloned = MemberwiseClone() as ChatRoomInfo;
            cloned.Title = cloned.Title.Clone() as string;
            return cloned;
        }
    }
}
