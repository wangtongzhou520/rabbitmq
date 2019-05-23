using System;
using System.Collections.Generic;
using System.Text;
using OrderCommon;

namespace OrderDal
{
    public class OrderMessageLogDal: BaseDal<OrderMessageLogEntity>, IOrderMessageLogDal
    {
        public OrderMessageLogDal(IEnumerable<DbSqlSugarClient> clients) : base(clients)
        {

        }

        public void InsertOrderMessageLog(OrderMessageLogEntity orderMessageLogEntity)
        {
            DbContext.Insertable<OrderMessageLogEntity>(orderMessageLogEntity);
        }
    }
}
