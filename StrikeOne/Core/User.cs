using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace StrikeOne.Core
{
    [Serializable]
    public class User : Player, ISerializable
    {
        public IPAddress LanIpAddress { set; get; }
        public IPAddress WanIpAddress { set; get; }

        public List<Achievement> Achievements { set; get; } = new List<Achievement>();

        public User() { }
        private User(SerializationInfo info, StreamingContext context)
        {
            Id = info.GetValue<Guid>("Id");
            Name = info.GetString("Name");
            Introduction = info.GetString("Introduction");
            WanIpAddress = IPAddress.Parse(info.GetString("WanIp"));
            LanIpAddress = IPAddress.Parse(info.GetString("LanIp"));

            Avator = info.GetValue<Image>("Avator");

            Records = info.GetValue<List<Record>>("Records");
            Achievements = info.GetValue<List<Achievement>>("Achievements");
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id);
            info.AddValue("Name", Name);
            info.AddValue("Introduction", Introduction);
            info.AddValue("LanIp", LanIpAddress.ToString());
            info.AddValue("WanIp", WanIpAddress.ToString());

            if (Avator != null)
            {
                MemoryStream Stream = new MemoryStream();
                Avator.Save(Stream, ImageFormat.Png);
                info.AddValue("Avator", Stream.GetBuffer());
                Stream.Close();
            }
            else
                info.AddValue("Avator", Encoding.UTF8.GetBytes("NULL"));

            info.AddValue("Records", Records);
            info.AddValue("Achievements", Achievements);
        }
    }
}
