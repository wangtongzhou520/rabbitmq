using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrderCommon;

namespace OrderService
{
    
    public interface IMessageService<T> where T: class, new()
    {
        void SendMessage(Message message,T tMessageLog);

        void BatchSendMessage(Message message,List<T> ts);

        void SendDelayMessage(Message message,T tMessageLog);


    }
}
