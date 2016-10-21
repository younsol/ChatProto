using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NGTSqlServer
{
    public abstract class SqlServerStoredProcedure<TResult> : IDisposable where TResult : new()
    {
        protected List<TResult> result;
        public List<TResult> Result { get { return result; } }

        private PropertyInfo[] reflection { get; set; }

        public SqlServerStoredProcedure()
        {
            reflection = typeof(TResult).GetProperties()
                .Where(pi => pi.PropertyType.IsPrimitive ||
                pi.PropertyType == typeof(string) ||
                pi.PropertyType == typeof(DateTime)).ToArray();
        }
        
        public abstract string GetCommand();

        protected bool LoadResult(DataTable dataTable)
        {
            result = new List<TResult>();
            try
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    var item = new TResult();
                    foreach (PropertyInfo pi in reflection)
                    {
                        if (dataTable.Columns.Contains(pi.Name))
                        {
                            pi.SetValue(item, row[pi.Name]);
                        }
                    }
                    result.Add(item);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return false;
            }
            return true;
        }

        public async Task<bool> ExecuteAsync(SqlServerDatabase database)
        {
            var executeCommand = database.ExecuteAsync(GetCommand());
            await executeCommand;
            return LoadResult(executeCommand.Result);
        }

        public void Dispose() {}
    }
}
