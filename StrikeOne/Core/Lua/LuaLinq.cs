using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrikeOne.Core.Lua
{
    public class LuaList<T> : IEnumerable<T>
    {
        public LuaList() { Core = new List<T>(); } 
        public LuaList(List<T> Source) { Core = Source; }

        private List<T> Core { set; get; }

        public void Add(T Item) { Core.Add(Item); }
        public T First() { return Core.First(); }
        public LuaList<T> Where(NLua.LuaFunction Function)
        {
            return new LuaList<T>(Core.Where(O =>
                (bool)Function.Call(O)[0]).ToList());
        } 
        public LuaList<T> OrderByDescending(NLua.LuaFunction Function)
        {
            return new LuaList<T>(Core.OrderByDescending(O =>
                Function.Call(O)[0]).ToList());
        }
        public LuaList<T> Take(int Count)
        {
            return new LuaList<T>(Core.Take(Count).ToList());
        } 

        public IEnumerator<T> GetEnumerator()
        {
            return Core.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class LuaDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public TValue Get(TKey Key) { return this[Key]; }
    }
}
