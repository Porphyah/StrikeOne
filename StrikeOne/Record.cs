﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrikeOne
{
    public class Record
    {
        public Guid Id { set; get; }
        public bool Win { set; get; }
        public DateTime Time { set; get; }
        public List<Participant> Participants { set; get; } = new List<Participant>();
        public int Rounds { set; get; }
    }

    public class Participant
    {
        public Guid Id { set; get; }
        public string Name { set; get; }
        public Bitmap Avator { set; get; }
        public bool Allied { set; get; }
        public int Rounds { set; get; }
        public int TotalDamage { set; get; }
        public int TotalInjured { set; get; }
    }
}
