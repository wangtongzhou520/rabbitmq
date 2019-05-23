using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;

namespace OrderCommon
{
    [SugarTable("t_user")]
    public class UserEntity
    {
        public UserEntity()
        {

        }

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "user_id")]
        public long UserId { get; set; }

        [SugarColumn(ColumnName = "user_name")]
        public string UserName { get; set; }
    }
}
