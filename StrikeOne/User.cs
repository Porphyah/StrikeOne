using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;

namespace StrikeOne
{
    [Serializable]
    public class User : ISerializable
    {
        public Guid Id { set; get; }
        public string Name { set; get; }
        public Image Avator { set; get; }
        public ImageFormat AvatorFormat { set; get; }
        public string Introduction { set; get; }

        public List<Achievement> Achievements { set; get; } = new List<Achievement>();

        public User() { }
        private User(SerializationInfo info, StreamingContext context)
        {
            Id = (Guid)info.GetValue("Id", typeof (Guid));
            Name = info.GetString("Name");
            Introduction = info.GetString("Introduction");

            MemoryStream Stream = new MemoryStream((byte[])info.GetValue("Avator", typeof(byte[])));
            Avator = (Bitmap)Image.FromStream(Stream);
            switch (info.GetString("AvatorFormat"))
            {
                case "Png": AvatorFormat = ImageFormat.Png; break;
                case "Bmp": AvatorFormat = ImageFormat.Bmp; break;
                case "Jpeg": AvatorFormat = ImageFormat.Jpeg; break;
                case "Tiff": AvatorFormat = ImageFormat.Tiff; break;
            }
            Stream.Close();
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id);
            info.AddValue("Name", Name);
            info.AddValue("Introduction", Introduction);

            MemoryStream Stream = new MemoryStream();
            Avator.Save(Stream, System.Drawing.Imaging.ImageFormat.Png);
            info.AddValue("Avator", Stream.GetBuffer());
            info.AddValue("AvatorFormat", AvatorFormat.ToString());
            Stream.Close();
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
