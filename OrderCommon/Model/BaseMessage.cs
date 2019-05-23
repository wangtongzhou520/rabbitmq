using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;

namespace OrderCommon
{
    public class BaseMessage
    {
        [SugarColumn(ColumnName = "status")]
        public int Status { get; set; }
        /// <summary>
        /// Desc:下一次重试时间
        /// Default:
        /// Nullable:False
        /// </summary>
        [SugarColumn(ColumnName = "next_retry_time")]
        public DateTime NextRetryTime { get; set; }

        /// <summary>
        /// Desc:更新时间
        /// Default:
        /// Nullable:False
        /// </summary>  
        [SugarColumn(ColumnName = "update_time")]
        public DateTime UpdateTime { get; set; }

        [SugarColumn(ColumnName = "try_count")]
        public int TryCount { get; set; }

        /// <summary>
        /// Desc:消息内容
        /// Default:
        /// Nullable:True
        /// </summary>
        [SugarColumn(ColumnName = "message_info")]
        public string MessageInfo { get; set; }

        /// <summary>
        /// Desc:消息id
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, ColumnName = "message_id")]
        public long MessageId { get; set; }
    }
}
