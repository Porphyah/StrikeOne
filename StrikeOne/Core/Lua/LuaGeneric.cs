using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrikeOne.Core.Lua
{
    public class LuaGeneric
    {
        public LuaList<object> List() { return new LuaList<object>(); }
        public HashSet<object> HashSet() { return new HashSet<object>(); }
        public Stack<object> Stack() { return new Stack<object>(); }
        public Queue<object> Queue() { return new Queue<object>(); }
        public LuaDictionary<object, object> Dictionary() { return new LuaDictionary<object, object>(); }

        public DateTime DateTime(int Year, int Month, int Day) { return new DateTime(Year, Month, Day); }
        public TimeSpan TimeSpan { get; } = new TimeSpan();
        public Random Random() { return new Random
            ((int)(System.DateTime.Now.Ticks & 0xffffffffL) | (int)(System.DateTime.Now.Ticks >> 32)); }
        public Guid Guid() { return System.Guid.NewGuid(); }

        public void ForEach(IEnumerable List, NLua.LuaFunction Function)
        {
            try
            {
                foreach (var Object in List)
                    Function.Call(Object);
            }
            catch (Exception ex)
            {
                ((LuaDebug)LuaMain.LuaState["Debug"]).Log(
                    ex.Message, "Red");
            }
        }
    }

    

    public class TimeSpan
    {
        public System.TimeSpan FromSeconds(double Seconds) { return System.TimeSpan.FromSeconds(Seconds); }
        public System.TimeSpan FromMinutes(double Minutes) { return System.TimeSpan.FromMinutes(Minutes); }
        public System.TimeSpan FromHours(double Hours) { return System.TimeSpan.FromHours(Hours); }
        public System.TimeSpan FromDays(double Days) { return System.TimeSpan.FromDays(Days); }
    }
}
