using NGTSqlServer;

namespace ChatProtoDatabase
{
    public sealed class ChatProtoSqlServer : SqlServerDatabase
    {
        public static ChatProtoSqlServer Instance = new ChatProtoSqlServer();
        private static readonly string DatabaseConnectionString
            = "Data Source=GT15767-I1;Initial Catalog=ChatProtoDb;Persist Security Info=True;User ID=sa;Password=1q2w#E$R";

        private ChatProtoSqlServer() : base(DatabaseConnectionString) { }
    }
}
