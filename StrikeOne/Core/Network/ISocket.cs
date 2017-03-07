using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace StrikeOne.Core.Network
{
    public interface ISocket
    {
        object this[string key] { set; get; }

        /// <summary>
        /// 获取是否已连接。
        /// </summary>
        bool IsConnected { get; }

        SocketDataReceiver DataReceiver { get; }
        SocketDataSender DataSender { get; }
        void Send<T>(T Data) where T : ISerializable;
        void BeginReceive(int DataLength);
        T EndReceive<T>() where T : ISerializable;

        /// <summary>
        /// 发送数据。
        /// </summary>
        /// <param name="data">要发送的数据。</param>
        void Send(byte[] data);
        /// <summary>
        /// 异步发送数据。
        /// </summary>
        /// <param name="data">要发送的数据。</param>
        void SendAsync(byte[] data);
        /// <summary>
        /// 断开连接。
        /// </summary>
        void Disconnect();
        /// <summary>
        /// 异步断开连接。
        /// </summary>
        void DisconnectAsync();
        /// <summary>
        /// 断开完成时引发事件。
        /// </summary>
        event EventHandler<SocketEventArgs> DisconnectCompleted;
        /// <summary>
        /// 接收完成时引发事件。
        /// </summary>
        event EventHandler<SocketEventArgs> ReceiveCompleted;
        /// <summary>
        /// 发送完成时引发事件。
        /// </summary>
        event EventHandler<SocketEventArgs> SendCompleted;
    }

    /// <summary>
    /// Socket事件参数
    /// </summary>
    public class SocketEventArgs : EventArgs
    {
        /// <summary>
        /// 实例化Socket事件参数
        /// </summary>
        /// <param name="socket">相关Socket</param>
        /// <param name="operation">操作类型</param>
        public SocketEventArgs(ISocket socket, SocketAsyncOperation operation)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));
            Socket = socket;
            Operation = operation;
        }

        public SocketEventArgs(ISocket socket, SocketAsyncOperation operation, string Message)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));
            Socket = socket;
            Operation = operation;
            this.Message = Message;
        }

        /// <summary>
        /// 获取或设置事件相关数据。
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// 获取数据长度。
        /// </summary>
        public int DataLength => Data?.Length ?? 0;

        /// <summary>
        /// 获取事件相关Socket
        /// </summary>
        public ISocket Socket { get; private set; }

        /// <summary>
        /// 获取事件操作类型。
        /// </summary>
        public SocketAsyncOperation Operation { get; private set; }

        /// <summary>
        /// 获取附加信息。
        /// </summary>
        public string Message { get; private set; }
    }
}
