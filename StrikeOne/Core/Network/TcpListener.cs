using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NATUPNPLib;

namespace StrikeOne.Core.Network
{
    /// <summary>
    /// TCP监听端
    /// </summary>
    public class TcpListener : IEnumerable<TcpListenerClient>, IDisposable
    {
        private Socket Socket { set; get; }
        private HashSet<TcpListenerClient> Clients { get; }

        /// <summary>
        /// 实例化TCP监听者。
        /// </summary>
        public TcpListener()
        {
            Clients = new HashSet<TcpListenerClient>();
            IsStarted = false;
            Handler = new SocketHandler();
        }

        public ISocketHandler Handler { get; set; }
        public bool Lan { set; get; } = false;
        private int _Port;
        /// <summary>
        /// 监听端口。
        /// </summary>
        public int Port
        {
            get { return _Port; }
            set
            {
                if (value < 0 || value > 65535)
                    throw new ArgumentOutOfRangeException(_Port + "不是有效端口。");
                _Port = value;
            }
        }

        /// <summary>
        /// 服务启动中
        /// </summary>
        public bool IsStarted { get; private set; }

        /// <summary>
        /// 开始服务。
        /// </summary>
        public void Start()
        {
            lock (this)
            {
                if (IsStarted)
                    throw new InvalidOperationException("已经开始服务。");

                //if (!Lan)
                //{
                //    var Mappings = new UPnPNAT().StaticPortMappingCollection;
                //    if (Mappings == null)
                //        throw new NotSupportedException("没有检测到路由器，或者路由器不支持UPnP功能。");
                //    Mappings.Add(App.Port, "TCP", App.Port, App.LanIpAddress.ToString(), true, "StrikeOne");
                //}

                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //绑定端口
                //可以引发端口被占用异常
                //Socket.Bind(new IPEndPoint(Lan ? App.LanIpAddress : App.WanIpAddress, _Port));
                Socket.Bind(new IPEndPoint(IPAddress.Any, _Port));
                //监听队列
                Socket.Listen(512);
                //如果端口是0，则是随机端口，把这个端口赋值给port
                _Port = ((IPEndPoint)Socket.LocalEndPoint).Port;
                //服务启动中设置为true
                IsStarted = true;
                //开始异步监听
                Socket.BeginAccept(EndAccept, null);
            }
        }

        //异步监听结束
        private void EndAccept(IAsyncResult result)
        {
            if (Socket == null) return;
            //获得客户端Socket
            Socket clientSocket = Socket.EndAccept(result);
            //实例化客户端类
            TcpListenerClient client = new TcpListenerClient(this, clientSocket);
            //增加事件钩子
            client.SendCompleted += client_SendCompleted;
            client.ReceiveCompleted += client_ReceiveCompleted;
            client.DisconnectCompleted += client_DisconnectCompleted;
            Socket.BeginAccept(EndAccept, null);

            //增加客户端
            lock (Clients)
                Clients.Add(client);

            //客户端连接事件
            AcceptCompleted?.Invoke(this, new SocketEventArgs(client, SocketAsyncOperation.Accept));
        }
        //客户端断开连接
        private void client_DisconnectCompleted(object sender, SocketEventArgs e)
        {
            //移除客户端
            lock (Clients)
                Clients.Remove((TcpListenerClient)e.Socket);

            e.Socket.DisconnectCompleted -= client_DisconnectCompleted;
            e.Socket.ReceiveCompleted -= client_ReceiveCompleted;
            e.Socket.SendCompleted -= client_SendCompleted;
            DisconnectCompleted?.Invoke(this, e);
        }
        //收到客户端发送的数据
        private void client_ReceiveCompleted(object sender, SocketEventArgs e)
        {
            ReceiveCompleted?.Invoke(this, e);
        }
        //向客户端发送数据完成
        private void client_SendCompleted(object sender, SocketEventArgs e)
        {
            SendCompleted?.Invoke(this, e);
        }

        /// <summary>
        /// 停止服务。
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                if (!IsStarted)
                    throw new InvalidOperationException("没有开始服务。");
                foreach (TcpListenerClient client in Clients)
                {
                    client.Disconnect();
                    client.DisconnectCompleted -= client_DisconnectCompleted;
                    client.ReceiveCompleted -= client_ReceiveCompleted;
                    client.SendCompleted -= client_SendCompleted;
                }
                Socket.Close();
                Socket = null;
                IsStarted = false;
            }
        }

        /// <summary>
        /// 接收完成时引发事件。
        /// </summary>
        public event EventHandler<SocketEventArgs> ReceiveCompleted;
        /// <summary>
        /// 接受客户完成时引发事件。
        /// </summary>
        public event EventHandler<SocketEventArgs> AcceptCompleted;
        /// <summary>
        /// 客户断开完成时引发事件。
        /// </summary>
        public event EventHandler<SocketEventArgs> DisconnectCompleted;
        /// <summary>
        /// 发送完成时引发事件。
        /// </summary>
        public event EventHandler<SocketEventArgs> SendCompleted;

        /// <summary>
        /// 获取客户端枚举。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TcpListenerClient> GetEnumerator()
        {
            return Clients.GetEnumerator();
        }

        /// <summary>
        /// 获取客户端泛型。
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Clients.GetEnumerator();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (Socket == null)
                return;
            Stop();
        }
    }
}
