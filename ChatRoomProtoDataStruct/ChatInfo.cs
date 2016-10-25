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

        public object Clone()
        {
            var cloned = MemberwiseClone() as ChatInfo;
            cloned.ChatText = cloned.ChatText.Clone() as string;
            cloned.ChatTime = new DateTime(cloned.ChatTime.Ticks);
            return cloned;
        }
    }
}
