using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;

namespace OrderDal
{
    public class DbSqlSugarClient:SqlSugarClient
    {
        public DbSqlSugarClient(ConnectionConfig config) : base(config)
        {
            DbName = string.Empty;
            Default = true;
        }
        public DbSqlSugarClient(ConnectionConfig config, string dbName) : base(config)
        {
            DbName = dbName;
            Default = true;
        }
        public DbSqlSugarClient(ConnectionConfig config, string dbName, bool isDefault) : base(config)
        {
            DbName = dbName;
            Default = isDefault;
        }
        public string DbName { set; get; }
        public bool Default { set; get; }
    }
}
