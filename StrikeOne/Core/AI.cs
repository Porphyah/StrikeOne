using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NLua;
using StrikeOne.Core.Lua;

namespace StrikeOne.Core
{
    [Serializable]
    public class AI : Player, ISerializable
    {
        public double RadicalRatio { set; get; }
        public Image Drawing { set; get; }
        public System.Collections.Generic.Dictionary<Skill, string[]> SkillPool { set; get; } = new System.Collections.Generic.Dictionary<Skill, string[]>();

        public override bool Equals(object obj)
        {
            if (!(obj is Player)) return false;
            return ((Player) obj).Id == this.Id;
        }
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public AI() { }
        protected AI(SerializationInfo info, StreamingContext context)
        {
            Id = info.GetValue<Guid>("Id");
            Name = info.GetString("Name");
            Introduction = info.GetString("Introduction");

            Avator = info.GetValue<Image>("Avator");
            Drawing = info.GetValue<Image>("Drawing");

            Records = info.GetValue<List<Record>>("Records");

            RadicalRatio = info.GetDouble("Radical");
            SkillPool = info.GetValue<System.Collections.Generic.Dictionary<Guid, string[]>>("Skills")
                .ToDictionary(O => App.SkillList.Find(P => P.Id == O.Key), 
                Q => Q.Value);
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id);
            info.AddValue("Name", Name);
            info.AddValue("Introduction", Introduction);

            if (Avator != null)
            {
                MemoryStream Stream = new MemoryStream();
                Avator.Save(Stream, ImageFormat.Png);
                info.AddValue("Avator", Stream.GetBuffer());
                Stream.Close();
            }
            else
                info.AddValue("Avator", Encoding.UTF8.GetBytes("NULL"));

            if (Drawing != null)
            {
                MemoryStream Stream = new MemoryStream();
                Drawing.Save(Stream, Drawing.RawFormat);
                info.AddValue("Drawing", Stream.GetBuffer());
                Stream.Close();
            }
            else
                info.AddValue("Drawing", Encoding.UTF8.GetBytes("NULL"));

            info.AddValue("Radical", RadicalRatio);
            info.AddValue("Skills", SkillPool.ToDictionary(O => O.Key.Id, P => P.Value));

            info.AddValue("Records", Records);
        }

        public Skill ChooseSkill()
        {
            long Tick = DateTime.Now.Ticks;
            Random Temp = new Random(
                (int)(Tick & 0xffffffffL) | (int)(Tick >> 32));
            return SkillPool.Count == 0 ? null : 
                SkillPool.Keys.ToArray()[Temp.Next(SkillPool.Count)];
        }
        public bool JudgeSkillCondition(Skill TargetSkill)
        {
            return (bool)((NLua.LuaFunction)LuaMain.LuaState.ExecuteString("return function(Skill)\n" +
                SkillPool[TargetSkill][0] + "\nend")[0]).Call(TargetSkill)[0];
        }
        public List<Player> GetSkillTargets(Skill TargetSkill)
        {
            return (((NLua.LuaFunction)LuaMain.LuaState.ExecuteString("return function(Skill)\n" +
                SkillPool[TargetSkill][1] + "\nend")[0]).Call(TargetSkill)).Cast<Player>().ToList();
        }
    }
}
