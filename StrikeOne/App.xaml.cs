using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
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
        public App()
        {
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

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

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string LogFileName = "Log\\" + "StrikeOne_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".txt";
            StreamWriter Writer = new StreamWriter(LogFileName);
            Writer.WriteLine("Log Generate Date：" + DateTime.Now);
            Writer.WriteLine();
            Writer.WriteLine("Error Message：" + e.Exception.Message + "\n");
            Writer.WriteLine();
            Writer.WriteLine("Error StackTrace：\n" + e.Exception.StackTrace + "\n");
            Writer.Close();

            MessageBox.Show("StrikeOne遇到了问题无法续命，把以下的信息甩给开发者背锅：\n" + e.Exception.Message + "\n\n以上信息已保存到" + LogFileName + "，将该文件发给开发者即可。",
                "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;//使用这一行代码告诉运行时，该异常被处理了，不再作为UnhandledException抛出了。
        }
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var Exception = (Exception)e.ExceptionObject;

            string LogFileName = "Log\\" + "StrikeOne_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".txt";
            StreamWriter Writer = new StreamWriter(LogFileName);
            Writer.WriteLine("Log Generate Date：" + DateTime.Now);
            Writer.WriteLine();
            Writer.WriteLine("Error Message：" + Exception.Message + "\n");
            Writer.WriteLine();
            Writer.WriteLine("Error StackTrace：\n" + Exception.StackTrace + "\n");
            Writer.Close();

            MessageBox.Show("StrikeOne遇到了问题无法续命，把以下的信息甩给开发者背锅：\n" + Exception.Message + "\n\n以上信息已保存到" + LogFileName + "，将该文件发给开发者即可。",
                "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
