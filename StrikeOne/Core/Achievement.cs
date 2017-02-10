using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrikeOne.Core
{
    [Serializable]
    public abstract class Achievement
    {
        public string Name { set; get; }
        public string Image { set; get; }
        public string Intro { set; get; }

        public abstract bool Check();
    }
}
