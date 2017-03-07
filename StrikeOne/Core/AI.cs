using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace StrikeOne.Core
{
    [Serializable]
    public class AI : Player, ISerializable
    {
        public double RadicalRatio { set; get; }
        public Image Drawing { set; get; }
        public List<Skill> SkillPool { set; get; } = new List<Skill>();

        public AI() { }
        protected AI(SerializationInfo info, StreamingContext context)
        {
            Id = info.GetValue<Guid>("Id");
            Name = info.GetString("Name");
            Introduction = info.GetString("Introduction");

            Avator = info.GetValue<Image>("Avator");
            Drawing = info.GetValue<Image>("Drawing");
            AvatorFormat = info.GetValue<ImageFormat>("AvatorFormat");

            Records = info.GetValue<List<Record>>("Records");

            RadicalRatio = info.GetDouble("Radical");
            SkillPool = info.GetValue<List<Guid>>("Skills")
                .Select(O => App.SkillList.Find(P => P.Id == O)).ToList();
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id);
            info.AddValue("Name", Name);
            info.AddValue("Introduction", Introduction);

            MemoryStream Stream = new MemoryStream();
            Avator.Save(Stream, AvatorFormat);
            info.AddValue("Avator", Stream.GetBuffer());
            info.AddValue("AvatorFormat", AvatorFormat.ToString());
            Stream.Close();

            Stream = new MemoryStream();
            Drawing.Save(Stream, AvatorFormat);
            info.AddValue("Drawing", Stream.GetBuffer());
            Stream.Close();

            info.AddValue("Radical", RadicalRatio);
            info.AddValue("Skills", SkillPool.Select(O => O.Id).ToList());

            info.AddValue("Records", Records);
        }
    }
}
