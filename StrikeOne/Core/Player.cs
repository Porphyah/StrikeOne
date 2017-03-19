using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace StrikeOne.Core
{
    public abstract class Player
    {
        public Guid Id { set; get; }
        public string Name { set; get; }
        public Image Avator { set; get; }
        public string Introduction { set; get; }
        public List<Record> Records { set; get; } = new List<Record>();

        public override string ToString()
        {
            return Name;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (!(obj is User)) return false;
            return ((User)obj).Id == this.Id;
        }

        public double VictoryRatio =>
            Records.Count == 0 ? 0 : Records.Count(O => O.Win) / (double)Records.Count;
        public double LuckRatio
        {
            get
            {
                var DiceRolls = Records.SelectMany(O => O.Participants.Find(P => P.Id == Id).Rolls).ToList();
                return DiceRolls.Count == 0 ? 0
                    : DiceRolls.Where(O => O.Success).Sum(O => (O.Probability.Value - O.Probability.Key) / (double)O.Probability.Value)
                      / DiceRolls.Sum(O => (O.Probability.Value - O.Probability.Key) / (double)O.Probability.Value);
            }
        }

        public BattleData BattleData { set; get; }

    }
}
