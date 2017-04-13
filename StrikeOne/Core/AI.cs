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
        public Dictionary<Skill, string[]> SkillPool { set; get; } = new System.Collections.Generic.Dictionary<Skill, string[]>();
        public AttackInclination Inclination { set; get; } = AttackInclination.Random;
        public Dictionary<Player, double> PlayerDataCollection { set; get; }

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

        public string ChooseAttackChoice()
        {
            if (BattleData.Skill != null && BattleData.Skill.IsActive &&
                BattleData.Skill.Enable && JudgeSkillCondition(BattleData.Skill))
                return "Skill";
            else
            {
                if (Inclination == AttackInclination.Vindictive &&
                    PlayerDataCollection.Where(O => O.Key.BattleData.Group.Name != BattleData.Group.Name &&
                         O.Key.BattleData.CurrentHp.GetInt() > 0).All(O => O.Value <= 0))
                    return "Abondon";
                double Rand = new Random((int)(DateTime.Now.Ticks & 0xffffffffL) | 
                    (int)(DateTime.Now.Ticks >> 32)).NextDouble();
                return Rand >= RadicalRatio ? "Abondon" : "Attack";
            }
        }
        public string ChooseDefendChoice()
        {
            if (BattleData.Skill != null && BattleData.Skill.IsActive &&
                BattleData.Skill.Enable && JudgeSkillCondition(BattleData.Skill))
                return "Skill";
            else
                return BattleData.CurrentHp.GetDouble()/BattleData.TotalHp >= RadicalRatio ? "Defend" : "Counter";
        }

        public Player GetAttackTarget()
        {
            var PlayerList = BattleData.Battlefield.PlayerList.Where(O => O.BattleData.Group.Name !=
                            this.BattleData.Group.Name && O.BattleData.CurrentHp.GetDouble() > 0).ToList();
            long Tick = DateTime.Now.Ticks;
            Random Temp = new Random(
                (int)(Tick & 0xffffffffL) | (int)(Tick >> 32));
            if (BattleData.Battlefield.Round == 0 && Inclination != AttackInclination.Vindictive)
                return PlayerList[Temp.Next(PlayerList.Count)];
            else
                switch (Inclination)
                {
                    default:
                    case AttackInclination.Random:
                        return PlayerList[Temp.Next(PlayerList.Count)];
                    case AttackInclination.Bloody:
                        var Bloodless = PlayerList.Where(O => O.BattleData.CurrentHp.GetDouble() <= 
                            PlayerList.Min(P => P.BattleData.CurrentHp.GetDouble())).ToList();
                        return Bloodless[Temp.Next(Bloodless.Count)];
                    case AttackInclination.Relentless:
                        var Luckless = PlayerList.Where(O => O.BattleData.Luck <=
                            PlayerList.Min(P => P.BattleData.Luck)).ToList();
                        return Luckless[Temp.Next(Luckless.Count)];
                    case AttackInclination.Jealous:
                        var Lucky = PlayerList.Where(O => O.BattleData.Luck >=
                            PlayerList.Max(P => P.BattleData.Luck)).ToList();
                        return Lucky[Temp.Next(Lucky.Count)];
                    case AttackInclination.Vindictive:
                        var Vindictive = PlayerDataCollection.Where(O => PlayerList.Contains(O.Key)).ToList();
                        Vindictive = Vindictive.Where(O => O.Value >= Vindictive.Max(P => P.Value)).ToList();
                        return Vindictive[Temp.Next(Vindictive.Count)].Key;
                    case AttackInclination.TargetHard:
                        var Targets = PlayerDataCollection.Where(O => PlayerList.Contains(O.Key)).ToList();
                        Targets = Targets.Where(O => O.Value >= Targets.Max(P => P.Value)).ToList();
                        return Targets[Temp.Next(Targets.Count)].Key;
                }
        }
        private bool JudgeSkillCondition(Skill TargetSkill)
        {
            return (bool)((NLua.LuaFunction)LuaMain.LuaState.ExecuteString("return function(Skill)\n" +
                SkillPool[TargetSkill][0] + "\nend")[0]).Call(TargetSkill)[0];
        }
        public LuaList<Player> GetSkillTargets(Skill TargetSkill)
        {
            return (LuaList<Player>)((NLua.LuaFunction)LuaMain.LuaState.ExecuteString("return function(Skill)\n" +
                SkillPool[TargetSkill][1] + "\nend")[0]).Call(TargetSkill)[0];
        }
    }

    public enum AttackInclination
    {
        Random,
        Bloody,
        Relentless,
        Jealous,
        Vindictive,
        TargetHard
    }
}
