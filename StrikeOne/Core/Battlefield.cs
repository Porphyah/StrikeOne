﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StrikeOne.Core.Lua;

namespace StrikeOne.Core
{
    public class Battlefield
    {
        public Room Room { set; get; }
        public int Round { private set; get; }
        public List<Player> PlayerList { set; get; }
        public Player CurrentPlayer => PlayerQueue.Peek();
        public Record Record { set; get; }
        private Queue<Player> PlayerQueue { set; get; }

        //public Player CurrentAttacker { set; get; }

        public void Init(Room ParentRoom)
        {
            Room = ParentRoom;
            LuaMain.Run();
            PlayerList = Room.Groups.SelectMany(O => O.Participants).ToList();
            ResortPlayerList();
            PlayerList.ForEach(Player => Player.BattleData.Init(Player, this,
                Room.Groups.First(O => O.Participants.Contains(Player))));
            LuaMain.LuaState["Battlefield"] = this;
            Record = new Record()
            {
                Id = Room.Id,
                Participants = PlayerList.Select(O =>
                    new Participant()
                    {
                        Id = O.Id,
                        Name = O.Name,
                        Avator = O.Avator,
                        Group = O.BattleData.Group.Name,
                        TotalDamage = 0,
                        TotalInjured = 0
                    }).ToDictionary(O => O.Id, P => P),
                Time = DateTime.Now,
                Type = ParentRoom.BattleType
            };

            Round = 0;
        }
        private void ResortPlayerList()
        {
            List<Player> Temp = new List<Player>();
            long Tick = DateTime.Now.Ticks;
            Random Random = new Random(
                (int)(Tick & 0xffffffffL) | (int)(Tick >> 32));
            while (PlayerList.Count != 0)
            {
                int Index = Random.Next(PlayerList.Count);
                Temp.Add(PlayerList[Index]);
                PlayerList.RemoveAt(Index);
            }
            PlayerList = Temp;
        }

        public void NextRound()
        {
            Round++;
            PlayerQueue = new Queue<Player>(PlayerList);
            while (CurrentPlayer.BattleData.CurrentHp.GetInt() == 0)
                PlayerQueue.Dequeue();
        }
        public bool NextPlayer()
        {
            do { PlayerQueue.Dequeue(); }
            while (PlayerQueue.Count != 0 &&
                CurrentPlayer.BattleData.CurrentHp.GetInt() == 0);
            if (PlayerQueue.Count == 0) return false;
            return true;
        }

        public bool IsGameOver()
        {
            return Room.Groups.Count(O => O.Participants.Any(P =>
                P.BattleData.CurrentHp.GetDouble() > 0)) <= 1;
        }
        public Group GetWinnerGroup()
        {
            if (!IsGameOver()) return null;
            return Room.Groups.First(O => O.Participants.Any(P =>
                P.BattleData.CurrentHp.GetDouble() > 0));
        }
        public void SetStatisticsRatio()
        {
            PlayerList.ForEach(O => O.BattleData.SetStatisticsRatio());
        }

        public LuaList<Player> GetLuaPlayerList()
        {
            return new LuaList<Player>(PlayerList);
        } 
    }
}
