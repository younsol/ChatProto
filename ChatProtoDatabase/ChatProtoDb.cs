using System.Data.Linq;
using System.Threading.Tasks;

namespace ChatProtoDatabase
{
    public static class ChatProtoDb
    {
        private static readonly string DatabaseConnectionString
            = "Data Source=GT15767-I1;Initial Catalog=ChatProtoDb;Persist Security Info=True;User ID=sa;Password=1q2w#E$R";
        private static ChatProtoDbDataContext Context = new ChatProtoDbDataContext(DatabaseConnectionString);

        public static async Task<ISingleResult<UserInquiryResult>> UserInquiry(string nickname, string password)
        {
            return await Task.Run(() => Context.UserInquiry(nickname, password));
        }

        public static async Task<ISingleResult<ChatInquiryResult>> ChatInquiry(long chatRoomId, long latestChatId = 0, int count = 100)
        {
            return await Task.Run(() => Context.ChatInquiry(chatRoomId, latestChatId, count));
        }
        public static async Task<ISingleResult<ChatRoomCreateResult>> ChatRoomCreate(long userId, string title)
        {
            return await Task.Run(() => Context.ChatRoomCreate(userId, title));
        }
        public static async Task<ISingleResult<ChatRoomJoinResult>> ChatRoomJoin(long chatRoomId, long userId)
        {
            return await Task.Run(() => Context.ChatRoomJoin(chatRoomId, userId));
        }
        public static async Task<ISingleResult<ChatRoomLeaveResult>> ChatRoomLeave(long chatRoomId, long userId)
        {
            return await Task.Run(() => Context.ChatRoomLeave(chatRoomId, userId));
        }
        public static async Task<ISingleResult<UserCreateResult>> UserCreate(string nickname, string password)
        {
            return await Task.Run(() => Context.UserCreate(nickname, password));
        }
        public static async Task<ISingleResult<UserDeleteResult>> UserDelete(long userId)
        {
            return await Task.Run(() => Context.UserDelete(userId));
        }
        public static async Task<ISingleResult<ChatRoomListInquiryResult>> ChatRoomListInquiry()
        {
            return await Task.Run(() => Context.ChatRoomListInquiry());
        }
        public static async Task<ISingleResult<UserChatRoomListInquiryResult>> UserChatRoomListInquiry(long userId)
        {
            return await Task.Run(() => Context.UserChatRoomListInquiry(userId));
        }
        public static async Task<ISingleResult<ChatRoomInquiryResult>> ChatRoomInquiry(long chatRoomId)
        {
            return await Task.Run(() => Context.ChatRoomInquiry(chatRoomId));
        }
        public static async Task<ISingleResult<ChatRoomUserInquiryResult>> ChatRoomUserInquiry(long chatRoomId)
        {
            return await Task.Run(() => Context.ChatRoomUserInquiry(chatRoomId));
        }
        public static async Task<ISingleResult<ChatCreateResult>> ChatCreate(long chatRoomId, long userId, string chatText)
        {
            return await Task.Run(() => Context.ChatCreate(chatRoomId, userId, chatText));
        }
    }

}
