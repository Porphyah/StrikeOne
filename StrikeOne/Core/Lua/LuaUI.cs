using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using StrikeOne.Components;

namespace StrikeOne.Core.Lua
{
    public class LuaUI
    {
        internal Action<string, Color> SetGlobalStatusAction { set; private get; }
        internal Action<int, Action> DiceControlAction { set; private get; }
        internal Action<PlayerCard, Guid, BitmapImage, string> SetStatusImgAction { set; private get; }
        internal Action<PlayerCard, Guid> RemoveStatusImgAction { set; private get; }

        public void SetGlobalStatus(string Text, string Color)
        {
            //Text = Encoding.UTF8.GetString(Encoding.Convert(Encoding.ASCII, Encoding.UTF8, Encoding.ASCII.GetBytes(Text)));
            SetGlobalStatusAction?.Invoke(Text, (Color) typeof(Colors).GetProperties(BindingFlags.Public |
                    BindingFlags.Static).First(O => O.Name.Equals(Color, StringComparison.CurrentCultureIgnoreCase)).
                    GetValue(typeof(Colors)));
        }
        public void DiceControl(int Probability, NLua.LuaFunction EndAction)
        {
            DiceControlAction?.Invoke(Probability, delegate { EndAction.Call(); });
        }
        public void SetStatusImg(Player Target, Guid Id, string Uri, string Description)
        {
            //Description = Encoding.UTF8.GetString(Encoding.Convert(Encoding.ASCII, Encoding.UTF8, Encoding.ASCII.GetBytes(Description)));
            SetStatusImgAction?.Invoke(Target.BattleData.PlayerCard, Id, new BitmapImage(
                new Uri(Directory.GetCurrentDirectory() + "/Skills/StatusImgs/" + Uri)), Description);
        }
        public void RemoveStatusImg(Player Target, Guid Id)
        {
            RemoveStatusImgAction?.Invoke(Target.BattleData.PlayerCard, Id);
        }

        public void Wait(double Seconds, NLua.LuaFunction Action)
        {
            DispatcherTimer Timer = new DispatcherTimer()
            { Interval = System.TimeSpan.FromSeconds(Seconds) };
            Timer.Tick += delegate
            {
                Action.Call();
                Timer.Stop();
            };
            Timer.Start();
        }


    }
}
