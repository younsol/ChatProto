using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace NGTSqlServer
{
    public abstract class SqlServerDatabase
    {
        private SqlConnection connection;

        protected SqlServerDatabase(string connectionString)
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
        }

        ~SqlServerDatabase()
        {
            connection.Close();
            connection.Dispose();
        }

        public async Task<DataTable> ExecuteAsync(string command)
        {
            using (var executeSql = new SqlCommand(command, connection).ExecuteReaderAsync())
            {
                await executeSql;
                var result = new DataTable();
                result.Load(executeSql.Result);
                return result;
            }
        }
    }
}
