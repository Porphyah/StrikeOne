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
                User.LanIpAddress = App.LanIpAddress;
                User.WanIpAddress = App.WanIpAddress;
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

        public static void LoadAis()
        {
            App.AiList = Directory.GetFiles("AIs")
                .Where(O => O.EndsWith(".data", StringComparison.CurrentCultureIgnoreCase))
                .Select(O =>
                {
                    var Stream = new FileStream(O, FileMode.Open);
                    BinaryFormatter BF = new BinaryFormatter();
                    var AI = BF.Deserialize(Stream) as AI;
                    Stream.Close();
                    return AI;
                }).ToList();
        }

        public static void SaveSkill(Skill Skill)
        {
            IFormatter Formatter = new BinaryFormatter();
            Stream Stream = new FileStream("Skills/" + Skill.Id + ".data", FileMode.Create,
                FileAccess.Write, FileShare.ReadWrite);
            Stream.Seek(0, 0);
            Formatter.Serialize(Stream, Skill);
            Stream.Close();
        }

        public static void DeleteSkill(Skill Skill)
        {
            File.Delete(Directory.GetCurrentDirectory() + "/Skills/" +
                Skill.Id + ".data");
        }

        public static void LoadSkills()
        {
            App.SkillList = Directory.GetFiles("Skills")
                .Where(O => O.EndsWith(".data", StringComparison.CurrentCultureIgnoreCase))
                .Select(O =>
                {
                    var Stream = new FileStream(O, FileMode.Open);
                    BinaryFormatter BF = new BinaryFormatter();
                    var Skill = BF.Deserialize(Stream) as Skill;
                    Stream.Close();
                    return Skill;
                }).ToList();
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
                    case "LanIpAddress":
                        App.LanIpAddress = IPAddress.Parse(Fragments[1]);
                        break;
                    case "WanIpAddress":
                        App.WanIpAddress = IPAddress.Parse(Fragments[1]);
                        break;
                }
            }
            Reader.Close();
        }
    }
}
