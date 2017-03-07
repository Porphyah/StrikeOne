using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrikeOne.Core.Network
{
    internal class SocketHandlerState
    {
        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data { get; set; }
        /// <summary>
        /// 异步结果
        /// </summary>
        public IAsyncResult AsyncResult { get; set; }
        /// <summary>
        /// Socket网络流
        /// </summary>
        public Stream Stream { get; set; }
        /// <summary>
        /// 异步回调函数
        /// </summary>
        public AsyncCallback AsyncCallBack { get; set; }
        /// <summary>
        /// 是否完成
        /// </summary>
        public bool Completed { get; set; }
        /// <summary>
        /// 数据长度
        /// </summary>
        public int DataLength { get; set; }
    }
}
