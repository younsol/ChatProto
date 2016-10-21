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
            ChatRoomInfo cloned = new ChatRoomInfo();
            cloned.ChatRoomId = ChatRoomId;
            cloned.Title = Title.Clone() as string;
            return cloned;
        }
    }
}
