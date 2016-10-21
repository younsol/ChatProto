﻿using NGTSqlServer;

namespace ChatProtoDatabase
{
    public sealed class ChatProtoSqlServer : SqlServerDatabase
    {
        private static ChatProtoSqlServer instance;
        public static ChatProtoSqlServer Instance { get { if (instance == null) instance = new ChatProtoSqlServer(); return instance; } }
        private static readonly string DatabaseConnectionString
            = "Data Source=GT15767-I1;Initial Catalog=ChatProtoDb;Persist Security Info=True;User ID=sa;Password=1q2w#E$R";

        private ChatProtoSqlServer() : base(DatabaseConnectionString) { }

    }
}
