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
    public class User : ISerializable
    {
        public Guid Id { set; get; }
        public string Name { set; get; }
        public Image Avator { set; get; }
        public ImageFormat AvatorFormat { set; get; }
        public string Introduction { set; get; }
        public IPAddress IpAddress { set; get; }

        public List<Record> Records { set; get; } = new List<Record>();
        public List<Achievement> Achievements { set; get; } = new List<Achievement>();

        public User() { }
        private User(SerializationInfo info, StreamingContext context)
        {
            Id = (Guid) info.GetValue("Id", typeof (Guid));
            Name = info.GetString("Name");
            Introduction = info.GetString("Introduction");
            IpAddress = IPAddress.Parse(info.GetString("Ip"));

            MemoryStream Stream = new MemoryStream((byte[]) info.GetValue("Avator", typeof (byte[])));
            Avator = (Bitmap) Image.FromStream(Stream);
            switch (info.GetString("AvatorFormat"))
            {
                case "Png":
                    AvatorFormat = ImageFormat.Png;
                    break;
                case "Bmp":
                    AvatorFormat = ImageFormat.Bmp;
                    break;
                case "Jpeg":
                    AvatorFormat = ImageFormat.Jpeg;
                    break;
                case "Tiff":
                    AvatorFormat = ImageFormat.Tiff;
                    break;
            }
            Stream.Close();

            Records = (List<Record>) info.GetValue("Records", typeof (List<Record>));
            Achievements = (List<Achievement>) info.GetValue("Achievements", typeof (List<Achievement>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id);
            info.AddValue("Name", Name);
            info.AddValue("Introduction", Introduction);
            info.AddValue("Ip", IpAddress.ToString());

            MemoryStream Stream = new MemoryStream();
            Avator.Save(Stream, System.Drawing.Imaging.ImageFormat.Png);
            info.AddValue("Avator", Stream.GetBuffer());
            info.AddValue("AvatorFormat", AvatorFormat.ToString());
            Stream.Close();

            info.AddValue("Records", Records);
            info.AddValue("Achievements", Achievements);
        }

        public override string ToString()
        {
            return Name;
        }

        public double VictoryRatio =>
            Records.Count == 0 ? 0 : Records.Count(O => O.Win)/(double) Records.Count;
        public double LuckRatio
        {
            get
            {
                var DiceRolls = Records.SelectMany(O => O.Participants.Find(P => P.Id == Id).Rolls).ToList();
                return DiceRolls.Count == 0 ? 0
                    : DiceRolls.Where(O => O.Success).Sum(O => (O.Probability.Value - O.Probability.Key)/(double) O.Probability.Value)
                      /DiceRolls.Sum(O => (O.Probability.Value - O.Probability.Key)/(double) O.Probability.Value);
            }
        }
}
}
