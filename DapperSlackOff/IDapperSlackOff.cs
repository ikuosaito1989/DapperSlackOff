using System.Collections.Generic;
using System.Data.SqlClient;

namespace Dapper
{
    public interface IDapperSlackOff
    {
        SqlConnection GetOpenConnection();
        IEnumerable<T> Query<T>(string sql, object param = null);
        int Execute(string sql, object param);
        IEnumerable<T> Get<T>(object entity = null, bool conditions = true);
        IEnumerable<T1> GetList<T1, T2>(object lists, string keyName = null);
        int Insert<T>(object entity);
        int Update<T>(object entity);
        int Delete<T>(object entity = null, bool conditions = true);
        int CreateOrUpdate<T>(T entity);
    }
}
