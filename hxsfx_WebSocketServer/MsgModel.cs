using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hxsfx_WebSocketServer
{
    /// <summary>
    /// 消息模型
    /// </summary>
    class MsgModel
    {
        /// <summary>
        /// 昵称
        /// </summary>
        public string nickName { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 消息日期（年月日）
        /// </summary>
        public string date { get; set; }
        /// <summary>
        /// 消息时间（时分秒）
        /// </summary>
        public string time { get; set; }
    }
}
