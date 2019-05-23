using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;

namespace OrderDal
{
    public class DbConnectionOption
    {
        public string Name { set; get; }
        public string ConnectionString { set; get; }
        public DbType DbType { set; get; }
        public bool IsAutoCloseConnection { set; get; }
        public bool Default { set; get; } = true;
    }
}
