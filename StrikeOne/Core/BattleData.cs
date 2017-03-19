using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StrikeOne.Components;

namespace StrikeOne.Core
{
    public class BattleData
    {
        private double _Hp = 0;
        private double _Luck = 0;

        public Room Room { set; get; } = null;
        public Group Group { set; get; } = null;
        public Player Owner { set; get; } = null;
        public Battlefield Battlefield { set; get; } = null;
        public Skill Skill { set; get; } = null;
        public double TotalHp { set; get; } = 10;
        public double CurrentHp {
            set
            {
                _Hp = value;
                UpdateHp?.Invoke();
            }
            get { return _Hp; }
        }
        public double Luck
        {
            set
            {
                _Luck = value;
                UpdateLuck?.Invoke();
            }
            get { return _Luck; }
        }

        public int AttackSuccessRatio { set; get; } = 2;
        public int DefendSuccessRatio { set; get; } = 2;
        public int CounterSuccessRatio { set; get; } = 2;

        public double AttackDamage { set; get; } = 2;
        public double DefenceCapacity { set; get; } = 1;
        public double CounterDamage { set; get; } = 2;
        public double CounterPunishment { set; get; } = 3;

        public PlayerCard PlayerCard { set; get; }
        public PlayerItem PlayerItem { set; get; }

        public void Init(Player Owner, Battlefield Battlefield, Group Group)
        {
            this.Room = Battlefield.Room;
            this.Battlefield = Battlefield;
            this.Owner = Owner;
            this.TotalHp = 10;
            this._Hp = 10;
            this.Group = Group;
            this.Skill.Init(Owner);
            this._Luck = 0.0;
        }
        public void SetupUiConnection()
        {
            UpdateHp += delegate
            {
                PlayerCard.UpdateHp();
                PlayerItem.UpdateHp();
            };
            UpdateLuck += delegate
            {
                PlayerCard.UpdateLuck();
            };
        }
        public void SetStatus(string Status)
        {
            PlayerCard.SetStatus(Status);
            PlayerItem.SetStatus(Status);
        }

        public event Action UpdateHp;
        public event Action UpdateLuck;

        public event EventHandler<UnderAttackEventArgs> UnderAttack;
        public event EventHandler<DoingEventArgs> Defending;
        public event EventHandler<DoingEventArgs> CounterAttacking;
        public event EventHandler<DoneEventArgs> Defended;
        public event EventHandler<DoneEventArgs> CounterAttacked;
        public event EventHandler<DoingEventArgs> Attacking;
        public event EventHandler<DoneEventArgs> Attacked;

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
    }

    public class UnderAttackEventArgs : EventArgs
    {
        public double Damage { set; get; }
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
