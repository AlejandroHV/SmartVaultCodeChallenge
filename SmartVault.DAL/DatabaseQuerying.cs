using Dapper;
using System.Data.SQLite;

namespace SmartVault.DAL
{
    public class DatabaseQuerying
    {
        private readonly SQLiteConnection? _connection;
        public DatabaseQuerying(SQLiteConnection connection)
        {
            _connection = connection;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="filter"></param>
        /// <param name="orderByColumnName"> </param>
        /// <param name="orderByType">DESC / ASC</param>
        /// <param name="limit">Include LIMIT clause in the query</param>
        /// <param name="nValue">Get the Nth value using the Limit clause</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetEntity<T>(string tableName, string filter = "1=1" , string orderByColumnName ="Id",string orderByType = "DESC",bool limit = false, int nValue =-1 )
        {
            string limitClause = limit ? $"LIMIT 1 OFFSET {nValue}" : "";
            string query = $"SELECT * FROM {tableName} WHERE {filter} ORDER BY {orderByColumnName} {orderByType}  {limitClause}";
            
            var results = _connection.Query<T>(query);

            return results;
        }

        public async Task<T> GetScalar<T>(string query)
        {
            var scalar = await _connection.ExecuteScalarAsync<T>(query);

            return scalar;

        }

    }
}
