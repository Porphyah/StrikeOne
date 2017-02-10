using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace StrikeOne.Core
{
    public static class IO
    {
        public static void LoadUsers()
        {
            App.UserList = Directory.GetFiles("Users")
                .Where(O => O.EndsWith(".data", StringComparison.CurrentCultureIgnoreCase))
                .Select(O =>
            {
                var Stream = new FileStream(O, FileMode.Open);
                BinaryFormatter BF = new BinaryFormatter();
                var User = BF.Deserialize(Stream) as User;
                Stream.Close();
                return User;
            }).ToList();
        }

        public static void SaveUser(User User)
        {
            IFormatter Formatter = new BinaryFormatter();
            Stream Stream = new FileStream("Users/" + User.Id + ".data", FileMode.Create,
                FileAccess.Write, FileShare.ReadWrite);
            Stream.Seek(0, 0);
            Formatter.Serialize(Stream, User);
            Stream.Close();
        }

        public static void LoadConfig()
        {
            StreamReader Reader = new StreamReader("Config.ini", Encoding.UTF8);
            Reader.ReadLine();
            string CurrentLine;
            while ((CurrentLine = Reader.ReadLine()) != null)
            {
                string[] Fragments = CurrentLine.Split(new char[] {'='}, 
                    StringSplitOptions.RemoveEmptyEntries);
                switch (Fragments[0])
                {
                    case "IpAddress":
                        App.IpAddress = IPAddress.Parse(Fragments[1]);
                        break;
                }
            }
            Reader.Close();
        }
    }
}
