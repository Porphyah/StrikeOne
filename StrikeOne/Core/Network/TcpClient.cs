using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StrikeOne.Core.Network
{
    /// <summary>
    /// TCP客户端
    /// </summary>
    public class TcpClient : ISocket, IDisposable
    {
        private Socket Socket { get; }
        private Stream Stream { set; get; }
        public SocketDataSender DataSender { get; }
        public SocketDataReceiver DataReceiver { get; }
        /// <summary>
        /// Socket处理程序
        /// </summary>
        public ISocketHandler Handler { get; set; }
        /// <summary>
        /// 实例化TCP客户端。
        /// </summary>
        public TcpClient()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Handler = new SocketHandler();
            Data = new Dictionary<string, object>();
            DataSender = new SocketDataSender(this);
            DataReceiver = new SocketDataReceiver(this);
        }

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
        /// 获取是否已连接。
        /// </summary>
        public bool IsConnected => Socket.Connected;
        public bool IsUseAuthenticate { get; set; }

        /// <summary>
        /// 连接至服务器。
        /// </summary>
        /// <param name="endpoint">服务器终结点。</param>
        public void Connect(IPEndPoint endpoint)
        {
            //判断是否已连接
            if (IsConnected)
                throw new InvalidOperationException("已连接至服务器。");
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));
            //锁定自己，避免多线程同时操作
            lock (this)
            {
                SocketAsyncState state = new SocketAsyncState();
                //Socket异步连接
                Socket.BeginConnect(endpoint, EndConnect, state).AsyncWaitHandle.WaitOne();
                //等待异步全部处理完成
                while (!state.Completed) { }
            }
        }

        /// <summary>
        /// 异步连接至服务器。
        /// </summary>
        /// <param name="endpoint"></param>
        public void ConnectAsync(IPEndPoint endpoint)
        {
            //判断是否已连接
            if (IsConnected)
                throw new InvalidOperationException("已连接至服务器。");
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));
            //锁定自己，避免多线程同时操作
            lock (this)
            {
                byte[] Punch = Encoding.UTF8.GetBytes("Punch!");
                Socket.SendTo(Punch, endpoint);

                SocketAsyncState state = new SocketAsyncState {IsAsync = true};
                //设置状态为异步
                //Socket异步连接
                Socket.BeginConnect(endpoint, EndConnect, state);
            }
        }

        private void EndConnect(IAsyncResult result)
        {
            SocketAsyncState state = (SocketAsyncState)result.AsyncState;

            try
            {
                Socket.EndConnect(result);
            }
            catch (Exception ex)
            {
                //出现异常，连接失败。
                state.Completed = true;
                //判断是否为异步，异步则引发事件
                if (state.IsAsync)
                    ConnectFailed?.Invoke(this, new SocketEventArgs(this, SocketAsyncOperation.Connect, ex.Message));
                return;
            }

            //连接成功。
            //创建Socket网络流
            Stream = new NetworkStream(Socket);
            if (IsUseAuthenticate)
            {
                NegotiateStream negotiate = new NegotiateStream(Stream);
                negotiate.AuthenticateAsClient();
                while (!negotiate.IsMutuallyAuthenticated)
                {
                    Thread.Sleep(10);
                }
            }
            //连接完成
            state.Completed = true;
            if (state.IsAsync)
                ConnectCompleted?.Invoke(this, new SocketEventArgs(this, SocketAsyncOperation.Connect));
            
            //开始接收数据
            Handler.BeginReceive(Stream, EndReceive, state);
        }

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

        /// <summary>
        /// 连接完成时引发事件。
        /// </summary>
        public event EventHandler<SocketEventArgs> ConnectCompleted;
        /// <summary>
        /// 连接失败时引发事件。
        /// </summary>
        public event EventHandler<SocketEventArgs> ConnectFailed;

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
            SocketAsyncState state = new SocketAsyncState();
            state.IsAsync = false;
            state.Data = data;
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
            SocketAsyncState state = new SocketAsyncState();
            state.IsAsync = true;
            state.Data = data;
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
        /// <summary>
        /// 断开与服务器的连接。
        /// </summary>
        public void Disconnect()
        {
            //判断是否已连接
            if (!IsConnected)
                throw new InvalidOperationException("未连接至服务器。");
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
                throw new InvalidOperationException("未连接至服务器。");
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
                // ignored
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
        /// <summary>
        /// 断开完成时引发事件。
        /// </summary>
        public event EventHandler<SocketEventArgs> DisconnectCompleted;
        /// <summary>
        /// 接收完成时引发事件。
        /// </summary>
        public event EventHandler<SocketEventArgs> ReceiveCompleted;
        /// <summary>
        /// 发送完成时引发事件。
        /// </summary>
        public event EventHandler<SocketEventArgs> SendCompleted;

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
