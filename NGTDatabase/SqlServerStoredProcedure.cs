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
        public IList<TResult> Result { get; private set; }

        private static PropertyInfo[] propertyInfos = SetPropertyInfos();

        private static PropertyInfo[] SetPropertyInfos()
        {
            return typeof(TResult).GetProperties()
                .Where(pi => pi.PropertyType.IsPrimitive ||
                pi.PropertyType == typeof(string) ||
                pi.PropertyType == typeof(DateTime)).ToArray();
        }

        public abstract string GetCommand();

        protected bool LoadResult(DataTable dataTable)
        {
            try
            {
                var result = new List<TResult>(dataTable.Rows.Count);
                foreach (DataRow row in dataTable.Rows)
                {
                    var item = new TResult();
                    foreach (PropertyInfo propertyInfo in propertyInfos)
                    {
                        if (dataTable.Columns.Contains(propertyInfo.Name))
                        {
                            propertyInfo.SetValue(item, row[propertyInfo.Name]);
                        }
                    }
                    result.Add(item);
                }
                Result = result.AsReadOnly();
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
            using (var executeCommand = database.ExecuteAsync(GetCommand()))
            {
                await executeCommand;
                return LoadResult(executeCommand.Result);
            }
        }

        public void Dispose() {}
    }
}
