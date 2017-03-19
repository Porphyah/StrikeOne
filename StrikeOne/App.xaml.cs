using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using StrikeOne.Core;
using StrikeOne.Core.Network;

namespace StrikeOne
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static User CurrentUser { set; get; }
        public static List<User> UserList { set; get; } = new List<User>();
        public static List<AI> AiList { set; get; } = new List<AI>();
        public static List<Skill> SkillList { set; get; } = new List<Skill>(); 
        public static IPAddress LanIpAddress { set; get; }
        public static IPAddress WanIpAddress { set; get; }
        public const int Port = 4000;
        public static Room CurrentRoom { set; get; }
        public static TcpClient Client { set; get; }
        public static TcpListener Server { set; get; }

        public static bool EditorMode { private set; get; } = false;
        public static bool DebugMode { private set; get; } = false;
        private void AppStartup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length != 0)
            {
                if (e.Args[0].Equals("EditorMode", 
                    StringComparison.CurrentCultureIgnoreCase))
                    EditorMode = true;
                else if (e.Args[0].Equals("DebugMode",
                    StringComparison.CurrentCultureIgnoreCase))
                    DebugMode = true;
            }
        }
    }
}
