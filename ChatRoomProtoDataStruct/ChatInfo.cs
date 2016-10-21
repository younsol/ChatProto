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
            ChatInfo cloned = new ChatInfo();
            cloned.ChatId = ChatId;
            cloned.ChatRoomId = ChatRoomId;
            cloned.UserId = UserId;
            cloned.ChatText = ChatText.Clone() as string;
            cloned.ChatTime = ChatTime;
            return cloned;
        }
    }
}
