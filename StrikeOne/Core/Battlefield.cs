using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrikeOne.Core
{
    public class Battlefield
    {
        public int Round { private set; get; }
        public List<Player> PlayerList { set; get; }
        public Player CurrentPlayer => PlayerQueue.Peek();
        private Queue<Player> PlayerQueue { set; get; }

        public void NextRound()
        {
            Round++;
            PlayerQueue = new Queue<Player>(PlayerList);
        }
        public void NextPlayer()
        {
            PlayerQueue.Dequeue();
        }
    }
}
