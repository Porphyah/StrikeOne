using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrikeOne.Core
{
    [Serializable]
    public class Record
    {
        public Guid Id { set; get; }
        public BattleType Type { set; get; }
        public bool Win { set; get; }
        public DateTime Time { set; get; }
        public TimeSpan Duration { set; get; }
        public Dictionary<Guid, Participant> Participants { set; get; } = new Dictionary<Guid, Participant>();
        public int Rounds { set; get; }

    }

    [Serializable]
    public class Participant
    {
        public Guid Id { set; get; }
        public string Name { set; get; }
        public Image Avator { set; get; }
        public string Group { set; get; }
        public int Rounds { set; get; }
        public int TotalDamage { set; get; }
        public int TotalInjured { set; get; }
        public List<DiceRoll> Rolls { set; get; } = new List<DiceRoll>(); 
    }
}
