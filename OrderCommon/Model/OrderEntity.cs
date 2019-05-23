using System;

namespace OrderCommon
{
    public class OrderEntity
    {
        public long OrderId { get; set; }

        public long OrderNo { get; set; }

        public long UserId { get; set; }

        public string OrderInfo { get; set; }
    }
}
