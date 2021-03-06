﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrikeOne.Core.Network
{
    internal class SocketAsyncState
    {
        /// <summary>
        /// 是否完成。
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data { get; set; }
        /// <summary>
        /// 是否异步
        /// </summary>
        public bool IsAsync { get; set; }
    }
}
