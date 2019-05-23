using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;

namespace OrderCommon
{
    /// <summary>
    /// 订单消息的的记录
    /// </summary>
    [SugarTable("t_order_message_log")]
    public class OrderMessageLogEntity:BaseMessage
    {
        public OrderMessageLogEntity()
        {

        }


        /// <summary>
        /// Desc:订单id
        /// Default:
        /// Nullable:False
        /// </summary>   
        [SugarColumn(ColumnName = "order_id")]
        public long OrderId { get; set; }

        /// <summary>
        /// Desc:用户id
        /// Default:
        /// Nullable:False
        /// </summary>
        [SugarColumn(ColumnName = "user_id")]
        public long UserId { get; set; }

        /// <summary>
        /// Desc:创建时间
        /// Default:
        /// Nullable:False
        /// </summary>  
        [SugarColumn(ColumnName = "create_time")]
        public DateTime CreateTime { get; set; }




    }
}
