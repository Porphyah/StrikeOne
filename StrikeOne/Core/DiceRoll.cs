using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StrikeOne.Core
{
    [Serializable]
    public class DiceRoll
    {
        public enum RollType
        {
            Attack,
            Defense,
            Counter,
            UseSkill
        }

        public bool Success { set; get; }
        public KeyValuePair<int, int> Probability { set; get; }
        public RollType Type { set; get; }
        public int SkillId { set; get; } = -1;
        public DateTime Time { set; get; }
    }
}
