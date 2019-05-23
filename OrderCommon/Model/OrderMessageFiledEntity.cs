using System;
using System.Collections.Generic;
using System.Text;

namespace OrderCommon
{
    /// <summary>
    /// 订单消费失败存储表
    /// </summary>
    public class OrderMessageFiledEntity
    {
        public string FileId { get; set; }

        public string MessageId { get; set; }

        public string FialDesc { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

    }
}
