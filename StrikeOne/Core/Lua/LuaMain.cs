using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace StrikeOne.Core.Lua
{
    public static class LuaMain
    {
        public static LuaFramework LuaState { private set; get; }
        public static bool Ready { private set; get; } = false;

        public static void Run()
        {
            Ready = false;
            LuaState = new LuaFramework
            {
                //["UI"] = new LuaUI(MainWindow),
                ["Debug"] = new LuaDebug(),
                ["Math"] = new LuaMath(),
                ["Generic"] = new LuaGeneric(),
            };
            LuaState["Core"] = LuaState;
            LuaState.LuaState.DebugHook += (sender, e) => { MainWindow.Instance?.SetLog(e.LuaDebug.source, Colors.Black); };
            LuaState.LuaState.HookException += (sender, e) => { MainWindow.Instance?.SetLog(e.Exception.Message, Colors.Red); };
            //LuaState.ExecuteFile("Object.lua");
            //CompileLuaFiles(Script.CompileOrders);
            //LuaState.ExecuteFunction(Script.Path, "Main");
            Ready = true;
        }

        //private static void CompileLuaFiles(List<KeyValuePair<string, string>> CompileOrders)
        //{
        //    foreach (var Order in CompileOrders)
        //        switch (Order.Key)
        //        {
        //            case "CompileFile":
        //                LuaState.ExecuteFile(Path + "/" + Order.Value);
        //                break;
        //            case "CompileFolder":
        //                foreach (var File in Directory.GetFiles(Path + "/" + Order.Value).Where(O => O.EndsWith(".lua")))
        //                    LuaState.ExecuteFile(File);
        //                break;
        //        }
        //}
    }

    public class LuaDebug
    {
        public event Action<string> ConsoleAction;

        public void Log(string Text, string Color = null, bool Bold = false, bool Italic = false)
        {
            MainWindow.Instance?.SetLog(Text, Color == null ? Colors.Black : (Color)typeof(Colors)
                .GetProperties(BindingFlags.Static | BindingFlags.Public)
                .First(O => O.Name == Color)
                .GetValue(typeof(Colors)), Bold, Italic);
        }

        public void Console(string Message)
        {
            ConsoleAction?.Invoke(Message);
        }
    }
}
