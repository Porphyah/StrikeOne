using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace StrikeOne.Core
{
    public static class SerializableExtension
    {
        public static T Deserialize<T>(this BinaryFormatter Formatter, Stream Stream)
        {
            if (IndirectTypes.Contains(typeof (T)))
                return (T)DeserializeIndirectly<T>(Formatter, Stream);

            return (T)Formatter.Deserialize(Stream);
        }
        public static T GetValue<T>(this SerializationInfo info, string Name)
        {
            if (IndirectTypes.Contains(typeof(T)))
                return (T)GetIndirectValue<T>(info, Name);
         
            return (T) info.GetValue(Name, typeof (T));
        }

        private static object GetIndirectValue<T>(SerializationInfo info, string Name)
        {
            if (typeof (T) == typeof (System.Drawing.Image))
            {
                var Buffer = (byte[]) info.GetValue(Name, typeof (byte[]));
                return Encoding.UTF8.GetString(Buffer).Equals("NULL", 
                    StringComparison.CurrentCultureIgnoreCase) ? null : 
                    Image.FromStream(new MemoryStream(Buffer));
            }
            if (typeof (T) == typeof (System.Windows.Media.Color))
            {
                var Color = info.GetValue<byte[]>(Name);
                return System.Windows.Media.Color.FromArgb(
                    Color[0], Color[1], Color[2], Color[3]);
            };
            if (typeof (T) == typeof (System.Drawing.Imaging.ImageFormat))
            {
                var Format = info.GetString(Name);
                return typeof (ImageFormat).GetProperties(BindingFlags.Public |
                    BindingFlags.Static).First(O => O.Name.Equals(Format, StringComparison.CurrentCultureIgnoreCase)).
                    GetValue(typeof(ImageFormat));
            }
            if (typeof (T) == typeof (Room))
            {
                Room TempRoom = (Room)info.GetValue(Name, typeof(Room));
                TempRoom.Groups.ForEach(O => O.Room = TempRoom);
                return TempRoom;
            }
            return null;
        }
        private static object DeserializeIndirectly<T>(BinaryFormatter Formatter, Stream Stream)
        {
            if (typeof(T) == typeof(Room))
            {
                Room TempRoom = (Room)Formatter.Deserialize(Stream);
                TempRoom.Groups.ForEach(O =>
                {
                    O.Room = TempRoom;
                    O.InitParticipants();
                });
                return TempRoom;
            }
            return null;
        }


        private static readonly List<Type> IndirectTypes = new List<Type>()
        {
            typeof (System.Drawing.Image),
            typeof (System.Windows.Media.Color),
            typeof (System.Drawing.Imaging.ImageFormat),
            typeof (Room)
        };   
    }

                
}
