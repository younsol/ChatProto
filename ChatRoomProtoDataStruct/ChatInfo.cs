using System;

namespace ChatProtoDataStruct
{
    [Serializable]
    public class ChatInfo : ICloneable
    {
        public long ChatId { get; set; }
        public long ChatRoomId { get; set; }
        public long UserId { get; set; }
        public string ChatText { get; set; }
        public DateTime ChatTime { get; set; }

        public object Clone() { return MemberwiseClone(); }
    }
}
