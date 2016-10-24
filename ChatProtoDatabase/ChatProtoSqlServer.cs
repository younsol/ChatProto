using NGTSqlServer;

namespace ChatProtoDatabase
{
    public sealed class ChatProtoSqlServer : SqlServerDatabase
    {
        private static ChatProtoSqlServer instance;
        private static object instanceLock = new object();
        public static ChatProtoSqlServer Instance
        {
            get {
                lock(instanceLock)
                {
                    if (instance == null)
                        instance = new ChatProtoSqlServer();
                }
                return instance;
            }
        }

        private static readonly string DatabaseConnectionString
            = "Data Source=GT15767-I1;Initial Catalog=ChatProtoDb;Persist Security Info=True;User ID=sa;Password=1q2w#E$R";

        private ChatProtoSqlServer() : base(DatabaseConnectionString) { }
    }
}
