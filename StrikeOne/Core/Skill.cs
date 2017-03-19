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
        public string AffectScript { set; get; }
        public SkillOccasion Occasion { set; get; } = SkillOccasion.BeforeAttacking;
        public List<SkillTarget> TargetSelections { set; get; } = new List<SkillTarget>();
        public int Probability { set; get; } = 2;
        public int TotalCount { set; get; } = -1;
        public int Duration { set; get; } = 0;
        public int CoolDown { set; get; } = 0;
        
        public Player Owner { set; get; }
        public int RemainedCount { set; get; }
        public int ContinuedDuration { set; get; } = -1;
        public int CoolDownStartRound { set; get; } = -1;
        public object Tag { set; get; }
        public event EventHandler<SkillLaunchEventArgs> SkillLaunch;
        public event EventHandler<SkillLaunchEventArgs> SkillAffect; 

        public Skill() { }
        public void Init(Player Player)
        {
            Owner = Player;
            RemainedCount = TotalCount;
            ContinuedDuration = -1;
            CoolDownStartRound = -1;
            Tag = null;

            LuaMain.LuaState["CurrentSkill"] = this;
            LuaMain.LuaState.ExecuteString("CurrentSkill.SkillLaunch:Add(function(Skill, E)\n" +
                LaunchScript + "\nend)");
            LuaMain.LuaState.ExecuteString("CurrentSkill.SkillAffect:Add(function(Skill, E)\n" +
                AffectScript + "\nend)");
            LuaMain.LuaState["CurrentSkill"] = null;
        }

        public bool Enable =>
            !IsCoolingDown() && RemainedCount != 0 && !IsAffecting();
        public void Launch(List<Player> TargetPlayers)
        {
            SkillLaunch?.Invoke(this, new SkillLaunchEventArgs()
            {
                Skill = this,
                Launcher = Owner,
                Targets = TargetPlayers
            });
            if (Duration == 0)
            {
                if (TotalCount != -1) RemainedCount--;
                CoolDownStartRound = Owner.BattleData.Battlefield.Round;
            }
            else
                ContinuedDuration = 0;
        }
        public void Affect()
        {
            if (Duration == 0) return;
            SkillAffect?.Invoke(this, new SkillLaunchEventArgs()
            {
                Skill = this,
                Launcher = Owner
            });
            ContinuedDuration++;
            if (ContinuedDuration >= Duration)
            {
                if (TotalCount != -1) RemainedCount--;
                ContinuedDuration = 0;
                CoolDownStartRound = Owner.BattleData.Battlefield.Round;
            }
        }

        public bool IsRemainingCount()
        {
            if (TotalCount == -1) return true;
            return RemainedCount != 0;
        }
        public bool IsAffecting()
        {
            if (Duration == 0) return false;
            return ContinuedDuration != -1;
        }
        public bool IsCoolingDown()
        {
            if (CoolDownStartRound == -1) return false;
            if (Owner.BattleData.Battlefield.Round > CoolDownStartRound + CoolDown)
            {
                CoolDownStartRound = -1;
                return false;
            }
            return true;
        }

        private Skill(SerializationInfo info, StreamingContext context)
        {
            Id = info.GetValue<Guid>("Id");
            Name = info.GetString("Name");
            Image = info.GetValue<Image>("Image");
            Description = info.GetString("Description");
            LaunchScript = info.GetString("LaunchScript");
            AffectScript = info.GetString("AffectScript");
            Occasion = info.GetValue<SkillOccasion>("Occasion");
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
                Description = Description,
                LaunchScript = LaunchScript,
                AffectScript = AffectScript,
                Occasion = Occasion,
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
            info.AddValue("AffectScript", AffectScript);
            info.AddValue("Occasion", Occasion);
            info.AddValue("TargetSelections", TargetSelections);
            info.AddValue("Probability", Probability);
            info.AddValue("TotalCount", TotalCount);
            info.AddValue("Duration", Duration);
            info.AddValue("CoolDown", CoolDown);
        }
    }

    [Serializable]
    public enum SkillOccasion
    {
        UnderAttack,
        Defending,
        CounterAttacking,
        Defended,
        CounterAttacked,
        BeforeAttacking,
        AfterAttacking
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
