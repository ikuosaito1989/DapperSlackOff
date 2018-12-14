using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Dapper;

namespace Dapper
{
    public partial class DapperSlackOff : IDapperSlackOff
    {
        private string _connectionString { get; set; }
        private string[] _creationDateColumns { get; set; }
        private string[] _updateDateColumn { get; set; }

        public DapperSlackOff(string connectionString, string[] creationDateColumns, string[] updateDateColumn)
        {
            _connectionString = connectionString;
            _creationDateColumns = creationDateColumns;
            _updateDateColumn = updateDateColumn;
        }

        public SqlConnection GetOpenConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public IEnumerable<T> Get<T>(object entity = null, bool conditions = true)
        {
            var searchCriteria = GetSearchCriteria(entity, conditions);
            searchCriteria = searchCriteria ?? $"WHERE {searchCriteria}";
            return Query<T>($"SELECT * FROM {typeof(T).Name} {searchCriteria}", entity);
        }

        public IEnumerable<T1> GetList<T1, T2>(object lists, string keyName = null)
        {
            var type = typeof(T1);
            var keyPropertyName = keyName ?? GetKeyProperty(type.GetProperties()).Name;
            var param = new { Lists = (IEnumerable<T2>)lists };
            return Query<T1>($"SELECT * FROM {type.Name} WHERE {keyPropertyName} IN @Lists", param);
        }

        public int Insert<T>(object entity)
        {
            var entityModel = ConvertEntity<T>(entity);
            var values = GetInsertValues<T>(typeof(T).GetProperties());
            return Execute($"INSERT INTO {typeof(T).Name} ({values.Key}) VALUES ({values.Value})", entityModel);
        }

        public int Update<T>(object entity)
        {
            var values = GetUpdateValues<T>(entity.GetType().GetProperties());
            return Execute($"UPDATE {typeof(T).Name} SET {values.Key} WHERE {values.Value}", entity);
        }

        public int Delete<T>(object entity, bool conditions = true)
        {
            var searchCriteria = GetSearchCriteria(entity, conditions);
            return Execute($"DELETE FROM {typeof(T).Name} WHERE {searchCriteria}", entity);
        }

        public int CreateOrUpdate<T>(T entity)
        {
            var keyProperty = GetKeyProperty(typeof(T).GetProperties());
            return IsPropertyDefaultValue(keyProperty, entity) ? Insert<T>(entity) : Update<T>(entity);
        }

        public IEnumerable<T> Query<T>(string sql, object param = null)
        {
            using (var connection = GetOpenConnection())
            {
                return connection.Query<T>(sql, param);
            }
        }

        public int Execute(string sql, object param)
        {
            using (var connection = GetOpenConnection())
            {
                return connection.Execute(sql, param, commandType: CommandType.Text);
            }
        }

        private KeyValuePair<string, string> GetInsertValues<T>(IEnumerable<PropertyInfo> propertyInfo)
        {
            Func<CustomAttributeData, bool> predicate = (CustomAttributeData c) => c.AttributeType.ToString().Contains(nameof(KeyAttribute));
            var properties = propertyInfo.Where(p => p.IsBuiltInType() && !p.CustomAttributes.Where(predicate).Any());
            var insertColumnes = properties.Select(x => $"{x.Name}");
            var insertValues = properties.Select(x => _creationDateColumns.Contains(x.Name) ? $"'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}'" : $"@{x.Name}");
            return new KeyValuePair<string, string>(string.Join(",", insertColumnes), string.Join(",", insertValues));
        }

        private KeyValuePair<string, string> GetUpdateValues<T>(IEnumerable<PropertyInfo> propertyInfo)
        {
            var ketProperty = GetKeyProperty(typeof(T).GetProperties());
            var updateSentence = propertyInfo.Where(p => p.IsBuiltInType() && !p.Name.Equals(ketProperty.Name) && !_updateDateColumn.Contains(p.Name)).Select(x => $"{x.Name}=@{x.Name}");
            var updateData = typeof(T).GetProperties().Where(x => _updateDateColumn.Contains(x.Name)).Select(x => $"{x.Name}='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}'");
            var searchCriteria = $"{ketProperty.Name}=@{ketProperty.Name}";
            return new KeyValuePair<string, string>(string.Join(",", updateSentence.Concat(updateData)), searchCriteria);
        }

        private string GetSearchCriteria(object entity, bool conditions = true)
        {
            if (entity == null) return null;
            var properties = entity?.GetType().GetProperties().Where(p => p.IsBuiltInType());
            return string.Join(conditions ? " AND " : " OR ", properties?.Select(p => $"{p.Name}=@{p.Name}"));
        }

        private PropertyInfo GetKeyProperty(PropertyInfo[] properties)
        {
            return properties.Where(p => p.CustomAttributes.Where(c => c.AttributeType.ToString().Contains(nameof(KeyAttribute))).Any()).SingleOrDefault();
        }

        private T ConvertEntity<T>(object entity)
        {
            var entityModel = Activator.CreateInstance(typeof(T));
            foreach (var property in entity.GetType().GetProperties())
            {
                var value = GetValueByKeyName(entity, property.Name);
                if (value != null)
                {
                    var model = entityModel.GetType().GetProperties().Where(y => y.Name == property.Name).First();
                    model.SetValue(entityModel, value);
                }
            }
            return (T)entityModel;
        }

        private object GetValueByKeyName(object entity, string name)
        {
            var property = entity.GetType().GetProperties().Where(x => x.GetIndexParameters().Length == 0 && x.Name.Equals(name));
            return property.Any() ? property.First().GetValue(entity) : null;
        }

        private bool IsPropertyDefaultValue(PropertyInfo property, object entity)
        {
            var value = property.GetValue(entity);
            if (value == null) return true;
            var type = value.GetType();
            return type.IsValueType ? value.Equals(Activator.CreateInstance(type)) : false;
        }
    }

    public static class PropertyInfoExtensions
    {
        public static bool IsBuiltInType(this PropertyInfo value)
        {
            return value.GetType().Namespace == nameof(System);
        }
    }
}
