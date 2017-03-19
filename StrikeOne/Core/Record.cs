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
        public List<Participant> Participants { set; get; } = new List<Participant>();
        public int Rounds { set; get; }

        
    }

    [Serializable]
    public class Participant
    {
        public Guid Id { set; get; }
        public string Name { set; get; }
        public Bitmap Avator { set; get; }
        public string Group { set; get; }
        public int Rounds { set; get; }
        public int TotalDamage { set; get; }
        public int TotalInjured { set; get; }
        public List<DiceRoll> Rolls { set; get; } = new List<DiceRoll>(); 
    }
}
