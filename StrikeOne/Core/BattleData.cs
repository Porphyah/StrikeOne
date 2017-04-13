using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StrikeOne.Components;

namespace StrikeOne.Core
{
    public class BattleData
    {
        public Room Room { set; get; } = null;
        public Group Group { set; get; } = null;
        public Player Owner { set; get; } = null;
        public Battlefield Battlefield { set; get; } = null;
        public Skill Skill { set; get; } = null;
        public static double TotalHp { set; get; } = 10;
        public Value CurrentHp { private set; get; }
        public double Luck
        {
             get
             {
                var DiceRolls = Battlefield.Record.Participants[Owner.Id].Rolls
                    .Where(O => O.Probability.Key != O.Probability.Value).ToList();
                return DiceRolls.Count == 0 ? 0
                    : DiceRolls.Where(O => O.Success).Sum(O => (O.Probability.Value - O.Probability.Key) / (double)O.Probability.Value)
                      / DiceRolls.Sum(O => (O.Probability.Value - O.Probability.Key) / (double)O.Probability.Value);
             }
        }

        public double Output { internal set; get; }
        public double Capacity { internal set; get; }
        public HashSet<Player> Executed { set; get; } 
        public double BeforeVictoryRatio { private set; get; }
        public double AfterVictoryRatio { private set; get; }
        public double BeforeLuckRatio { private set; get; }
        public double AfterLuckRatio { private set; get; }

        public Value AttackSuccessRatio { private set; get; }
        public Value DefendSuccessRatio { private set; get; }
        public Value CounterSuccessRatio { private set; get; }

        public Value AttackDamage { private set; get; }
        public Value DefenceCapacity { private set; get; }
        public Value CounterDamage { private set; get; }
        public Value CounterPunishment { private set; get; }

        public PlayerCard PlayerCard { set; get; }
        public PlayerItem PlayerItem { set; get; }

        public void Init(Player Owner, Battlefield Battlefield, Group Group)
        {
            this.Room = Battlefield.Room;
            this.Battlefield = Battlefield;
            this.Owner = Owner;
            this.CurrentHp = new Value(TotalHp, 10, 0);
            this.Group = Group;
            if (Skill != null) this.Skill.Init(Owner);
            if (Owner is AI && (((AI) Owner).Inclination == AttackInclination.Vindictive
                            || ((AI) Owner).Inclination == AttackInclination.TargetHard))
                ((AI) Owner).PlayerDataCollection = Battlefield.PlayerList.ToDictionary(O => O, P => 0.0);

            AttackSuccessRatio = new Value(2, 6, 0);
            AttackSuccessRatio.AddVariable(Guid.Parse("00000000-0000-0000-0000-000000000000"), "放弃累积奖励", 0);
            DefendSuccessRatio = new Value(2, 6, 0);
            DefendSuccessRatio.AddVariable(Guid.Parse("00000000-0000-0000-0000-000000000000"), "放弃累积奖励", 0);
            CounterSuccessRatio = new Value(2, 6, 0);

            AttackDamage = new Value(2, null, 0);
            DefenceCapacity = new Value(1, null, 0);
            CounterDamage = new Value(2, null, 0);
            CounterPunishment = new Value(3, null, 0);

            Output = 0.0;
            Capacity = 0.0;
            Executed = new HashSet<Player>();
        }
        public void SetStatisticsRatio()
        {
            BeforeVictoryRatio = Owner.VictoryRatio;
            BeforeLuckRatio = Owner.LuckRatio;

            var CloneRecord = Battlefield.Record.Clone() as Record;
            CloneRecord.Win = Battlefield.GetWinnerGroup().Name == Group.Name;
            Owner.Records.Add(CloneRecord);

            AfterVictoryRatio = Owner.VictoryRatio;
            AfterLuckRatio = Owner.LuckRatio;
        }

        public void SetupUiConnection()
        {
            CurrentHp.Update += delegate
            {
                PlayerCard.UpdateHp();
                PlayerItem.UpdateHp();
            };
        }
        public void SetStatus(string Status)
        {
            PlayerCard.SetStatus(Status);
            PlayerItem.SetStatus(Status);
        }

        public event EventHandler OnTurn;
        public event EventHandler<UnderAttackEventArgs> UnderAttack;
        public event EventHandler<DoingEventArgs> Defending;
        public event EventHandler<DoingEventArgs> CounterAttacking;
        public event EventHandler<DoneEventArgs> Defended;
        public event EventHandler<DoneEventArgs> CounterAttacked;
        public event EventHandler<DoingEventArgs> Attacking;
        public event EventHandler<DoneEventArgs> Attacked;

        private Dictionary<Guid, Delegate> EventDictionary { get; }
            = new Dictionary<Guid, Delegate>(); 

        public void OnTurnAction()
        {
            OnTurn?.Invoke(this, new EventArgs());
        }
        public void OnUnderAttack(UnderAttackEventArgs Args)
        {
            UnderAttack?.Invoke(this, Args);
        }
        public void OnDefending(DoingEventArgs Args)
        {
            Defending?.Invoke(this, Args);
        }
        public void OnDefended(DoneEventArgs Args)
        {
            Defended?.Invoke(this, Args);
        }
        public void OnCounterAttacking(DoingEventArgs Args)
        {
            CounterAttacking?.Invoke(this, Args);
        }
        public void OnCounterAttacked(DoneEventArgs Args)
        {
            CounterAttacked?.Invoke(this, Args);
        }
        public void OnAttacking(DoingEventArgs Args)
        {
            Attacking?.Invoke(this, Args);
        }
        public void OnAttacked(DoneEventArgs Args)
        {
            Attacked?.Invoke(this, Args);
        }

        public void AddEvent(Guid Id, string Occasion, NLua.LuaFunction Func)
        {
            Delegate Handler;
            switch (Occasion)
            {
                case "OnTurn":
                    Handler = new EventHandler((Sender, Args) => { Console.WriteLine(@"CallSuccess"); Func.Call(Sender, Args); });
                    EventDictionary.Add(Id, Handler);
                    OnTurn += (EventHandler)Handler;
                    break;
                case "Attacking":
                    Handler = new EventHandler<DoingEventArgs>((Sender, Args) => { Func.Call(Sender, Args); });
                    EventDictionary.Add(Id, Handler);
                    Attacking += (EventHandler<DoingEventArgs>)Handler;
                    break;
                case "Attacked":
                    Handler = new EventHandler<DoneEventArgs>((Sender, Args) => { Func.Call(Sender, Args); });
                    EventDictionary.Add(Id, Handler);
                    Attacked += (EventHandler<DoneEventArgs>)Handler;
                    break;
                case "UnderAttack":
                    Handler = new EventHandler<UnderAttackEventArgs>((Sender, Args) => { Func.Call(Sender, Args); });
                    EventDictionary.Add(Id, Handler);
                    UnderAttack += (EventHandler<UnderAttackEventArgs>)Handler;
                    break;
                case "Defending":
                    Handler = new EventHandler<DoingEventArgs>((Sender, Args) => { Func.Call(Sender, Args); });
                    EventDictionary.Add(Id, Handler);
                    Defending += (EventHandler<DoingEventArgs>)Handler;
                    break;
                case "Defended":
                    Handler = new EventHandler<DoneEventArgs>((Sender, Args) => { Func.Call(Sender, Args); });
                    EventDictionary.Add(Id, Handler);
                    Defended += (EventHandler<DoneEventArgs>)Handler;
                    break;
                case "CountAttacking":
                    Handler = new EventHandler<DoingEventArgs>((Sender, Args) => { Func.Call(Sender, Args); });
                    EventDictionary.Add(Id, Handler);
                    CounterAttacking += (EventHandler<DoingEventArgs>)Handler;
                    break;
                case "CountAttacked":
                    Handler = new EventHandler<DoneEventArgs>((Sender, Args) => { Func.Call(Sender, Args); });
                    EventDictionary.Add(Id, Handler);
                    CounterAttacked += (EventHandler<DoneEventArgs>)Handler;
                    break;
            }
        }
        public void RemoveEvent(Guid Id, string Occasion)
        {
            Delegate Event = EventDictionary[Id];
            switch (Occasion)
            {
                case "OnTurn":
                    OnTurn -= (EventHandler)Event;
                    break;
                case "Attacking":
                    Attacking -= (EventHandler<DoingEventArgs>)Event;
                    break;
                case "Attacked":
                    Attacked -= (EventHandler<DoneEventArgs>)Event;
                    break;
                case "UnderAttack":
                    UnderAttack -= (EventHandler<UnderAttackEventArgs>)Event;
                    break;
                case "Defending":
                    Defending -= (EventHandler<DoingEventArgs>)Event;
                    break;
                case "Defended":
                    Defended -= (EventHandler<DoneEventArgs>)Event;
                    break;
                case "CountAttacking":
                    CounterAttacking -= (EventHandler<DoingEventArgs>)Event;
                    break;
                case "CountAttacked":
                    CounterAttacked -= (EventHandler<DoneEventArgs>)Event;
                    break;
            }
            EventDictionary.Remove(Id);
        }

        public bool IsExecutioner => Battlefield.PlayerList.Max(O => O.BattleData.Executed.Count) <= Executed.Count;
        public bool IsContributor => Battlefield.Room.BattleType != BattleType.OneVsOne && 
            Battlefield.Room.BattleType != BattleType.TriangleMess && 
            Battlefield.Room.BattleType != BattleType.SquareMess &&
            Battlefield.PlayerList.Where(O => O.BattleData.Group.Name == this.Group.Name)
                .Max(O => O.BattleData.Output*0.6 + O.BattleData.Capacity*0.4) <= Output*0.6 + Capacity*0.4;
        public bool IsLuckyStar => Battlefield.PlayerList.Max(O => O.BattleData.Luck) <= Luck;
        public bool IsVictorious => Group.IsVictorious;
    }

    public class UnderAttackEventArgs : EventArgs
    {
        public double Damage { set; get; }
        public bool IsCounter { set; get; }
        public bool CanDefend { set; get; }
        public bool CanCounter { set; get; }
        public Player Attacker { set; get; }
        public Player Defender { set; get; }
    }

    public class DoingEventArgs : EventArgs
    {
        public double DoingValue { set; get; }
        public double DoingSuccessRatio { set; get; }
        public bool Continue { set; get; }
        public Player Attacker { set; get; }
        public Player Defender { set; get; }
    }

    public class DoneEventArgs : EventArgs
    {
        public double DoneValue { set; get; }
        public bool Success { set; get; }
        public Player Attacker { set; get; }
        public Player Defender { set; get; }
    }

}
