using System;
using System.Collections.Generic;
using System.Text;
using OrderCommon;

namespace OrderDal
{
    public interface IUserDal :IBaseDal<UserEntity>
    {
        List<UserMessageDto> GetUserMessageVosByUserId(long userId);
    }
}
