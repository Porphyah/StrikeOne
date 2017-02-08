using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace StrikeOne
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
                User User = BF.Deserialize(Stream) as User;
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
    }
}
