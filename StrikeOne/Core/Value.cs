using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrikeOne.Core
{
    public class Value
    {
        public Value(double basic, double? max = null, double? min = null)
        {
            this.Basic = basic;
            this.Variables = new Dictionary<Guid, Variable>();
            this.Maximum = max;
            this.Minimum = min;
        }

        public override string ToString()
        {
            return GetDouble().ToString();
        }

        public double Basic { private set; get; }
        public Dictionary<Guid, Variable> Variables { private set; get; }
        public double? Maximum { set; get; }
        public double? Minimum { set; get; }
        public object Tag { set; get; }

        public event Action Update;

        public double GetDouble()
        {
            double Result = Basic;
            foreach (var Variable in Variables.Values)
                if (Variable.IsMultiplier)
                    Result *= Variable.Value;
                else
                    Result += Variable.Value;
            if (Maximum != null && Result > Maximum.Value)
                Result = Maximum.Value;
            else if (Minimum != null && Result < Minimum.Value)
                Result = Minimum.Value;

            return Result;
        }
        public int GetInt()
        {
            return (int)Math.Round(GetDouble());
        }

        public void SetBasic(double value)
        {
            Basic = value;
            Update?.Invoke();
        }
        public void AddVariable(Guid Id, string Name, double Value, bool IsMultiplier = false)
        {
            Variables.Add(Id, new Variable()
            {
                Id = Id,
                Name = Name,
                Value = Value,
                IsMultiplier = IsMultiplier
            });
            Update?.Invoke();
        }
        public void UpdateVariable(Guid Id, double Value)
        {
            Variables[Id].Value = Value;
            Update?.Invoke();
        }
        public void UpdateVariable(Guid Id, double Change, bool IsMultiplier)
        {
            if (IsMultiplier)
                Variables[Id].Value *= Change;
            else
                Variables[Id].Value += Change;
            Update?.Invoke();
        }
        public void RemoveVariable(Guid Id)
        {
            Variables.Remove(Id);
            Update?.Invoke();
        }
    }

    public class Variable
    {
        public Guid Id { set; get; }
        public string Name { set; get; }
        public bool IsMultiplier { set; get; } = false;
        public double Value { set; get; }
    }
}
