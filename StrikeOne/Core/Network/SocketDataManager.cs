using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace StrikeOne.Core.Network
{
    public class SocketDataSender
    {
        public enum SenderStatus
        {
            Ready,
            Preparing,
            Sending
        }
        public class SendOnceEventArgs : OnceEventArgs
        {
            public SenderStatus Status { internal set; get; }
        }

        public SocketDataSender(ISocket Socket)
        {
            this.Socket = Socket;
            Status = SenderStatus.Ready;
        }

        private const int BufferLength = 65535;
        private MemoryStream MemoryStream { set; get; }
        private byte[] Buffer { set; get; }
        public byte[] FullData { private set; get; }
        public int DataLength { set; get; }
        public int SendedLength { set; get; }
        private ISocket Socket { get; }
        public SenderStatus Status { private set; get; }

        public event EventHandler<SendOnceEventArgs> SendOnce; 

        public void SendData<T>(T Data) where T : ISerializable
        {
            Status = SenderStatus.Preparing;
            MemoryStream = new MemoryStream();
            BinaryFormatter BinaryFormatter = new BinaryFormatter();  //二进制序列化类  
            BinaryFormatter.Serialize(MemoryStream, Data); //将消息类转换为内存流
            FullData = MemoryStream.GetBuffer();
            DataLength = FullData.Length;
            SendedLength = 0;
            MemoryStream.Flush();

            Socket.SendAsync(Encoding.UTF8.GetBytes("DataTransmit|*|" + DataLength));
        }

        public void SendCompleted(object Sender, SocketEventArgs E)
        {
            if (Status == SenderStatus.Sending)
            {
                if (SendedLength < DataLength)
                {
                    Buffer = new byte[DataLength - SendedLength < BufferLength ?
                        DataLength - SendedLength : BufferLength];
                    Buffer = FullData.Skip(SendedLength).Take(Buffer.Length).ToArray();
                    E.Socket.SendAsync(Buffer); //从内存中读取二进制流，并发送
                    SendedLength += Buffer.Length;
                }
                else
                    Status = SenderStatus.Ready;
            }
            else if (Status == SenderStatus.Preparing)
            {
                Status = SenderStatus.Sending;
                Buffer = new byte[BufferLength];
                MemoryStream.Position = 0;  //将流的当前位置重新归0，否则Read方法将读取不到任何数据  
                if (SendedLength < DataLength)
                {
                    Buffer = new byte[DataLength - SendedLength < BufferLength ?
                        DataLength - SendedLength : BufferLength];
                    Buffer = FullData.Skip(SendedLength).Take(Buffer.Length).ToArray();
                    E.Socket.SendAsync(Buffer); //从内存中读取二进制流，并发送
                    SendedLength += Buffer.Length;
                }
            }
            SendOnce?.Invoke(this, new SendOnceEventArgs()
            {
                Socket = Socket,
                NewData = Buffer,
                CumulativeLength = SendedLength,
                ReceivedLength = Buffer.Length,
                TotalLength = DataLength,
                Status = Status
            });
        }
    }

    public class SocketDataReceiver
    {
        public enum ReceiverStatus
        {
            Ready,
            Preparing,
            Receiving,
            Extractable
        }
        public class ReceiveOnceEventArgs : OnceEventArgs
        {
            public ReceiverStatus Status { internal set; get; }
        }

        public SocketDataReceiver(ISocket Socket)
        {
            this.Socket = Socket;
            Status = ReceiverStatus.Ready;
        }

        private MemoryStream MemoryStream { set; get; }
        private byte[] Buffer { set; get; }
        public byte[] FullData { private set; get; }
        public int ReceivedLength { set; get; }
        public int DataLength { private set; get; }
        private ISocket Socket { get; }
        public ReceiverStatus Status { private set; get; }

        public event EventHandler<ReceiveOnceEventArgs> ReceiveOnce;

        public void BeginReceiveData(int DataLength)
        {
            FullData = null;
            Status = ReceiverStatus.Preparing;
            this.DataLength = DataLength;
            FullData = new byte[DataLength];
            ReceivedLength = 0;
        }

        public T ReceiveData<T>() where T : ISerializable
        {
            if (Status != ReceiverStatus.Extractable)
                throw new NullReferenceException("数据尚未接收完毕。");
            MemoryStream = new MemoryStream(FullData) {Position = 0};
            BinaryFormatter BinaryFormatter = new BinaryFormatter();
            if (MemoryStream.Capacity > 0)
            {
                Status = ReceiverStatus.Ready;
                return BinaryFormatter.Deserialize<T>(MemoryStream);
            }
            throw new EndOfStreamException("内存流字节为0。");
        }

        public void ReceiveCompleted(object Sender, SocketEventArgs E)
        {
            Status = ReceiverStatus.Receiving;
            int ReceiveCount = E.DataLength;
            if (ReceiveCount == 0)
            {
                Status = ReceiverStatus.Extractable;
                ReceiveOnce?.Invoke(this, new ReceiveOnceEventArgs()
                {
                    Socket = Socket,
                    NewData = null,
                    CumulativeLength = ReceivedLength,
                    ReceivedLength = 0,
                    TotalLength = DataLength,
                    Status = Status
                });
                return;
            }
            //var Temp = Buffer.Clone() as byte[];
            //Buffer = new byte[ReceivedLength + ReceiveCount];
            //Temp.CopyTo(Buffer, 0);
            //E.Data.CopyTo(Buffer, ReceivedLength);
            Buffer = E.Data;
            Buffer.CopyTo(FullData, ReceivedLength);
            ReceivedLength += ReceiveCount;
            ReceiveOnce?.Invoke(this, new ReceiveOnceEventArgs()
            {
                Socket = Socket,
                NewData = Buffer,
                CumulativeLength = ReceivedLength,
                ReceivedLength = ReceiveCount,
                TotalLength = DataLength,
                Status = Status
            });
            if (ReceivedLength >= DataLength)
                Status = ReceiverStatus.Extractable;
        }
    }

    public abstract class OnceEventArgs : EventArgs
    {
        public ISocket Socket { internal set; get; }
        public byte[] NewData { internal set; get; }
        public int ReceivedLength { internal set; get; }
        public int CumulativeLength { internal set; get; }
        public int TotalLength { internal set; get; }
    }
   
}
