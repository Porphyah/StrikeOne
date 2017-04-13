 using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
 using StrikeOne.Core.Lua;

namespace StrikeOne.Core
{
    [Serializable]
    public class Skill : ICloneable, ISerializable
    {
        public Guid Id { set; get; }
        public string Name { set; get; }
        public Image Image { set; get; }
        public string Description { set; get; }
        public string LaunchScript { set; get; }
        public bool IsActive { set; get; } = true;
        public bool? IsDefendable { set; get; } = false;
        public List<SkillTarget> TargetSelections { set; get; } = new List<SkillTarget>();
        public int Probability { set; get; } = 2;
        public int TotalCount { set; get; } = -1;
        public int Duration { set; get; } = 0;
        public int CoolDown { set; get; } = 0;
        
        public Player Owner { set; get; }
        private NLua.LuaFunction LaunchFunc { set; get; }
        public int RemainedCount { set; get; }
        public List<Player> TargetCollection { set; get; } = new List<Player>();
        public KeyValuePair<SkillTarget, int>? CurrentTarget { set; get; } = null;
        public int DurationStartRound { set; get; } = -1;
        public int CoolDownStartRound { set; get; } = -1;
        public object Tag { set; get; }

        public Skill() { }

        public override string ToString()
        {
            return Name;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Skill)) return false;
            return this.Id == ((Skill) obj).Id;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public void Init(Player Player)
        {
            Owner = Player;
            RemainedCount = TotalCount;
            DurationStartRound = -1;
            CoolDownStartRound = -1;
            Tag = null;

            LaunchFunc = (NLua.LuaFunction)LuaMain.LuaState
                .ExecuteString("return function(Skill, E)\n" +
                               LaunchScript + "\nend")[0];

            //LuaMain.LuaState["CurrentSkill"] = this;
            //LuaMain.LuaState.ExecuteString("CurrentSkill.SkillLaunch:Add(function(Skill, E)\n" +
            //    LaunchScript + "\nend)");
            //LuaMain.LuaState.ExecuteString("CurrentSkill.SkillAffect:Add(function(Skill, E)\n" +
            //    AffectScript + "\nend)");
            //LuaMain.LuaState["CurrentSkill"] = null;
        }

        public bool Enable =>
            IsRemainingCount() && !IsAffecting() && !IsCoolingDown() ;
        public void NextTarget()
        {
            if (CurrentTarget == null)
                CurrentTarget = new KeyValuePair<SkillTarget, int>(TargetSelections[0], 0);
            else if (CurrentTarget.Value.Value == TargetSelections.Count - 1)
                CurrentTarget = null;
            else
                CurrentTarget = new KeyValuePair<SkillTarget, int>
                    (TargetSelections[CurrentTarget.Value.Value + 1], CurrentTarget.Value.Value + 1);
        }
        public void Launch()
        {
            LaunchFunc.Call(this, new SkillLaunchEventArgs()
            {
                Skill = this,
                Launcher = Owner,
                Targets = TargetCollection
            });
            if (TotalCount != -1) RemainedCount--;
            if (Duration == 0)
                CoolDownStartRound = Owner.BattleData.Battlefield.Round;
            else
                DurationStartRound = Owner.BattleData.Battlefield.Round;
        }
        private void EndAffect()
        {
            DurationStartRound = -1;
            if (CoolDown != 0)
                CoolDownStartRound = Owner.BattleData.Battlefield.Round;
        }

        public bool IsRemainingCount()
        {
            if (TotalCount == -1) return true;
            return RemainedCount != 0;
        }
        public bool IsAffecting()
        {
            if (DurationStartRound == -1 || Duration == 0) return false;
            if (Owner.BattleData.Battlefield.Round > DurationStartRound + Duration)
            {
                EndAffect();
                return false;
            }
            return true;
        }
        public bool IsCoolingDown()
        {
            if (CoolDownStartRound == -1 || CoolDown == 0) return false;
            if (Owner.BattleData.Battlefield.Round > CoolDownStartRound + CoolDown)
            {
                CoolDownStartRound = -1;
                return false;
            }
            return true;
        }

        private Skill(SerializationInfo info, StreamingContext context)
        {
            //foreach (var InfoPair in info)
            //{
            //    GetType().GetProperty(InfoPair.Name)
            //        .SetValue(this, InfoPair.Value);
            //}
            Id = info.GetValue<Guid>("Id");
            Name = info.GetString("Name");
            Image = info.GetValue<Image>("Image");
            Description = info.GetString("Description");
            LaunchScript = info.GetString("LaunchScript");
            IsActive = info.GetBoolean("IsActive");
            IsDefendable = info.GetBoolean("IsDefendable");
            TargetSelections = info.GetValue<List<SkillTarget>>("TargetSelections");
            Probability = info.GetInt32("Probability");
            TotalCount = info.GetInt32("TotalCount");
            Duration = info.GetInt32("Duration");
            CoolDown = info.GetInt32("CoolDown");
        }

        public object Clone()
        {
            return new Skill()
            {
                Id = Id,
                Name = Name,
                Image = Image,
                Description = Description,
                LaunchScript = LaunchScript,
                IsActive = IsActive,
                IsDefendable = IsDefendable,
                Probability = Probability,
                TotalCount = TotalCount,
                Duration = Duration,
                CoolDown = CoolDown,
                TargetSelections = TargetSelections.ToList()
            };
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id);
            info.AddValue("Name", Name);
            if (Image != null)
            {
                MemoryStream Stream = new MemoryStream();
                Image.Save(Stream, ImageFormat.Png);
                info.AddValue("Image", Stream.GetBuffer());
            }
            else
                info.AddValue("Image", Encoding.UTF8.GetBytes("NULL"));
            info.AddValue("Description", Description);
            info.AddValue("LaunchScript", LaunchScript);
            info.AddValue("IsActive", IsActive);
            info.AddValue("IsDefendable", IsDefendable);
            info.AddValue("TargetSelections", TargetSelections);
            info.AddValue("Probability", Probability);
            info.AddValue("TotalCount", TotalCount);
            info.AddValue("Duration", Duration);
            info.AddValue("CoolDown", CoolDown);
        }
    }

    [Serializable]
    public enum SkillTarget
    {
        Self,
        Ally,
        AllyWithoutSelf,
        Enemy,
        SelfGroup,
        EnemyGroup,
        AllEnemies,
        AllPlayers
    }

    public class SkillLaunchEventArgs : EventArgs
    {
        public Skill Skill { set; get; }
        public Player Launcher { set; get; }
        public List<Player> Targets { set; get; }
    }
}
