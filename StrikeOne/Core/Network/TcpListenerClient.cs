using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace StrikeOne.Core.Network
{
    public class TcpListenerClient : ISocket, IDisposable
    {
        private Socket Socket { get; }
        private Stream Stream { get; set; }
        public SocketDataSender DataSender { get; }
        public SocketDataReceiver DataReceiver { get; }

        /// <summary>
        /// 实例化TCP客户端。
        /// </summary>
        internal TcpListenerClient(TcpListener listener, Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));
            Socket = socket;
            Handler = listener.Handler;
            DataSender = new SocketDataSender(this);
            DataReceiver = new SocketDataReceiver(this);

            Data = new Dictionary<string, object>();
            this["RemoteEndPoint"] = socket.RemoteEndPoint;
            //创建Socket网络流
            Stream = new NetworkStream(socket);
            //设置服务器
            Listener = listener;

            //开始异步接收数据
            SocketAsyncState state = new SocketAsyncState();
            Handler.BeginReceive(Stream, EndReceive, state);
        }

        public TcpListener Listener { get; private set; }
        private Dictionary<string, object> Data { get; }

        public object this[string key]
        {
            get
            {
                key = key.ToLower();
                if (Data.ContainsKey(key))
                    return Data[key];
                return null;
            }
            set
            {

                key = key.ToLower();
                if (value == null)
                {
                    if (Data.ContainsKey(key))
                        Data.Remove(key);
                    return;
                }
                if (Data.ContainsKey(key))
                    Data[key] = value;
                else
                    Data.Add(key, value);
            }
        }

        /// <summary>
        /// Socket处理程序
        /// </summary>
        public ISocketHandler Handler { get; set; }

        /// <summary>
        /// 获取是否已连接。
        /// </summary>
        public bool IsConnected => Socket.Connected;

        #region 断开连接

        /// <summary>
        /// 断开与服务器的连接。
        /// </summary>
        public void Disconnect()
        {
            //判断是否已连接
            if (!IsConnected)
                throw new SocketException(10057);
            lock (this)
            {
                //Socket异步断开并等待完成
                Socket.BeginDisconnect(true, EndDisconnect, true).AsyncWaitHandle.WaitOne();
            }
        }

        /// <summary>
        /// 异步断开与服务器的连接。
        /// </summary>
        public void DisconnectAsync()
        {
            //判断是否已连接
            if (!IsConnected)
                throw new SocketException(10057);
            lock (this)
            {
                //Socket异步断开
                Socket.BeginDisconnect(true, EndDisconnect, false);
            }
        }

        private void EndDisconnect(IAsyncResult result)
        {
            try
            {
                Socket.EndDisconnect(result);
            }
            catch
            {

            }
            //是否同步
            bool sync = (bool)result.AsyncState;

            if (!sync)
                DisconnectCompleted?.Invoke(this, new SocketEventArgs(this, SocketAsyncOperation.Disconnect));
        }

        //这是一个给收发异常准备的断开引发事件方法
        private void Disconnected(bool raiseEvent)
        {
            if (raiseEvent)
                DisconnectCompleted?.Invoke(this, new SocketEventArgs(this, SocketAsyncOperation.Disconnect));
        }

        #endregion

        #region 发送数据

        /// <summary>
        /// 发送数据。
        /// </summary>
        /// <param name="data">要发送的数据。</param>
        public void Send(byte[] data)
        {
            //是否已连接
            if (!IsConnected)
                throw new SocketException(10057);
            //发送的数据不能为null
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            //发送的数据长度不能为0
            if (data.Length == 0)
                throw new ArgumentException("data的长度不能为0");

            //设置异步状态
            SocketAsyncState state = new SocketAsyncState
            {
                IsAsync = false,
                Data = data
            };
            try
            {
                //开始发送数据
                Handler.BeginSend(data, 0, data.Length, Stream, EndSend, state).AsyncWaitHandle.WaitOne();
            }
            catch
            {
                //出现异常则断开Socket连接
                Disconnected(true);
            }
        }

        /// <summary>
        /// 异步发送数据。
        /// </summary>
        /// <param name="data">要发送的数据。</param>
        public void SendAsync(byte[] data)
        {
            //是否已连接
            if (!IsConnected)
                throw new SocketException(10057);
            //发送的数据不能为null
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            //发送的数据长度不能为0
            if (data.Length == 0)
                throw new ArgumentException("data的长度不能为0");

            //设置异步状态
            SocketAsyncState state = new SocketAsyncState
            {
                IsAsync = true,
                Data = data
            };
            try
            {
                //开始发送数据并等待完成
                Handler.BeginSend(data, 0, data.Length, Stream, EndSend, state);
            }
            catch
            {
                //出现异常则断开Socket连接
                Disconnected(true);
            }
        }

        public void Send<T>(T BinData) where T : ISerializable
        {
            DataSender.SendData(BinData);
        }

        private void EndSend(IAsyncResult result)
        {
            SocketAsyncState state = (SocketAsyncState)result.AsyncState;

            //是否完成
            state.Completed = Handler.EndSend(result);
            //没有完成则断开Socket连接
            if (!state.Completed)
                Disconnected(true);
            //引发发送结束事件
            if (state.IsAsync)
            {
                if (DataSender.Status != SocketDataSender.SenderStatus.Preparing &&
                    DataSender.Status != SocketDataSender.SenderStatus.Sending)
                    SendCompleted?.Invoke(this, new SocketEventArgs(this, SocketAsyncOperation.Send) { Data = state.Data });
                else
                {
                    DataSender.SendCompleted(this, new SocketEventArgs(this, SocketAsyncOperation.Send) { Data = state.Data });
                    if (DataSender.Status == SocketDataSender.SenderStatus.Ready)
                        SendCompleted?.Invoke(this, new SocketEventArgs(this, SocketAsyncOperation.Send) { Data = DataSender.FullData });
                }
            }
        }

        #endregion

        #region 接收数据

        public void BeginReceive(int DataLength)
        {
            DataReceiver.BeginReceiveData(DataLength);
        }
        public T EndReceive<T>() where T : ISerializable
        {
            return DataReceiver.ReceiveData<T>();
        }

        private void EndReceive(IAsyncResult result)
        {
            SocketAsyncState state = (SocketAsyncState)result.AsyncState;
            //接收到的数据
            byte[] data = Handler.EndReceive(result);
            //如果数据长度为0，则断开Socket连接
            if (data.Length == 0)
            {
                Disconnected(true);
                return;
            }

            if (DataReceiver.Status != SocketDataReceiver.ReceiverStatus.Preparing
                && DataReceiver.Status != SocketDataReceiver.ReceiverStatus.Receiving)
                //引发接收完成事件
                ReceiveCompleted?.Invoke(this, new SocketEventArgs(this, SocketAsyncOperation.Receive) {Data = data});
            else
            {
                DataReceiver.ReceiveCompleted(this, new SocketEventArgs(this, SocketAsyncOperation.Receive) {Data = data});
                if (DataReceiver.Status == SocketDataReceiver.ReceiverStatus.Extractable)
                    ReceiveCompleted?.Invoke(this, new SocketEventArgs(this, SocketAsyncOperation.Receive) { Data = DataReceiver.FullData });
            }
            //再次开始接收数据
            Handler.BeginReceive(Stream, EndReceive, state);
        }



        #endregion

        #region 事件

        ///// <summary>
        ///// 断开完成时引发事件。
        ///// </summary>
        public event EventHandler<SocketEventArgs> DisconnectCompleted;
        ///// <summary>
        ///// 接收完成时引发事件。
        ///// </summary>
        public event EventHandler<SocketEventArgs> ReceiveCompleted;
        ///// <summary>
        ///// 发送完成时引发事件。
        ///// </summary>
        public event EventHandler<SocketEventArgs> SendCompleted;

        #endregion

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            lock (this)
            {
                if (IsConnected)
                    Socket.Disconnect(false);
                Socket.Close();
            }
        }
    }
}
