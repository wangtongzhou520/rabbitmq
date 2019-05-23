using System;
using System.Collections.Generic;
using System.Text;

namespace OrderCommon
{
    /// <summary>
    /// 消息投递的状态
    /// </summary>
    public enum MessageStatusEnum
    {
        /// <summary>
        /// 发送中
        /// </summary>
        SENDING,
        /// <summary>
        /// 发送成功
        /// </summary>
        SEND_SUCCESS,
        /// <summary>
        /// 发送失败
        /// </summary>
        SEND_FAILURE
    }
}
