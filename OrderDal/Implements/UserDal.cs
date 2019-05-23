using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OrderCommon;
using SqlSugar;

namespace OrderDal
{
    public class UserDal: BaseDal<UserEntity>,IUserDal
    {
        public UserDal(IEnumerable<DbSqlSugarClient> clients):base(clients)
        {

        }

        public List<UserMessageDto> GetUserMessageVosByUserId(long userId)
        {
            List<UserMessageDto> userMessageVos = DbContext.Queryable<UserEntity, OrderMessageLogEntity>((a, b) => new object[] {
                JoinType.Left,a.UserId==b.UserId
            }).Where((a, b) => a.UserId == userId)
            .Select((a, b) => new UserMessageDto { Message=b.MessageInfo,UserName=a.UserName }).ToList();

            return userMessageVos;
        }
    }
}
