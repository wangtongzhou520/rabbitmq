using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;

namespace OrderCommon
{
    [SugarTable("t_user")]
    public class UserRoleEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "user_role_id")]
        public long UserRoleId { get; set; }

        [SugarColumn(ColumnName = "user_id")]
        public string UserId { get; set; }


        [SugarColumn(ColumnName = "role_id")]
        public string RoleId { get; set; }


    }
}
