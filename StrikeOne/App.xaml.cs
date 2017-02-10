using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using StrikeOne.Core;

namespace StrikeOne
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static User CurrentUser { set; get; }
        public static List<User> UserList { set; get; } = new List<User>();
        public static IPAddress IpAddress { set; get; }
    }
}
