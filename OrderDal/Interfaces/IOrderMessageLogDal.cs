using System;
using System.Collections.Generic;
using System.Text;
using OrderCommon;

namespace OrderDal
{
    public interface IOrderMessageLogDal:IBaseDal<OrderMessageLogEntity>
    {
        void InsertOrderMessageLog(OrderMessageLogEntity orderMessageLogEntity);
    }
}
