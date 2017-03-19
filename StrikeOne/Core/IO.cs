﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
                var BF = new BinaryFormatter();
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

        public static void SaveAi(AI Ai)
        {
            //IFormatter Formatter = new BinaryFormatter();
            //Stream Stream = new FileStream("AIs/" + Ai.Id + ".data", FileMode.Create,
            //    FileAccess.Write, FileShare.ReadWrite);
            //Stream.Seek(0, 0);
            //Formatter.Serialize(Stream, Ai);
            //Stream.Close();

            XmlTextWriter Writer = new XmlTextWriter(
                new FileStream(Directory.GetCurrentDirectory() + "/AIs/" + Ai.Id + ".data",
                    FileMode.Create, FileAccess.Write), Encoding.UTF8)
            {
                IndentChar = '\t',
                Indentation = 1,
                Formatting = Formatting.Indented
            };
            Writer.WriteStartDocument(true);
            Writer.WriteStartElement("StrikeOne.AI");

            Writer.WriteAttributeString("Id", Ai.Id.ToString());
            Writer.WriteAttributeString("Name", Ai.Name);
            Writer.WriteElementString("Introduction", Ai.Introduction);

            if (Ai.Avator != null)
            {
                MemoryStream ms = new MemoryStream();
                Ai.Avator.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int) ms.Length);
                ms.Close();
                Writer.WriteElementString("Avator", Convert.ToBase64String(arr));
            }

            if (Ai.Drawing != null)
            {
                MemoryStream ms = new MemoryStream();
                Ai.Drawing.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                Writer.WriteElementString("Drawing", Convert.ToBase64String(arr));
            }

            Writer.WriteElementString("Radical", Ai.RadicalRatio.ToString());
            
            Writer.WriteStartElement("SkillPool");
            foreach (var Skill in Ai.SkillPool)
            {
                Writer.WriteStartElement("Skill");
                Writer.WriteAttributeString("Id", Skill.Key.Id.ToString());
                Writer.WriteStartElement("ConditionScript");
                Writer.WriteCData(Skill.Value[0]);
                Writer.WriteEndElement();
                Writer.WriteStartElement("TargetScript");
                Writer.WriteCData(Skill.Value[1]);
                Writer.WriteEndElement();
                Writer.WriteEndElement();
            }
            Writer.WriteEndElement();

            Writer.WriteEndElement();
            Writer.WriteEndDocument();
            Writer.Flush();
            Writer.Close();
        }

        public static void DeleteAi(AI Ai)
        {
            File.Delete(Directory.GetCurrentDirectory() + "/AIs/" +
                Ai.Id + ".data");
        }

        public static void LoadAis()
        {
            App.AiList = Directory.GetFiles("AIs")
                .Where(O => O.EndsWith(".data", StringComparison.CurrentCultureIgnoreCase))
                .Select(O =>
                {
                    //var Stream = new FileStream(O, FileMode.Open);
                    //BinaryFormatter BF = new BinaryFormatter();
                    //var AI = BF.Deserialize(Stream) as AI;
                    //Stream.Close();
                    //return AI;

                    XmlDocument Document = new XmlDocument();
                    Document.Load(O);
                    var RootNode = Document.ChildNodes[1];
                    var NewAi = new AI
                    {
                        Id = Guid.Parse(RootNode.Attributes["Id"].Value),
                        Name = RootNode.Attributes["Name"].Value
                    };
                    foreach (XmlElement Element in RootNode.ChildNodes)
                    {
                        switch (Element.Name)
                        {
                            case "Introduction":
                                NewAi.Introduction = Element.InnerText;
                                break;
                            case "Avator":
                                var AvatorBytes = Convert.FromBase64String(Element.InnerText);
                                MemoryStream MS1 = new MemoryStream(AvatorBytes, 0, AvatorBytes.Length);
                                MS1.Write(AvatorBytes, 0, AvatorBytes.Length);
                                NewAi.Avator = Image.FromStream(MS1);
                                break;
                            case "Drawing":
                                var DrawingBytes = Convert.FromBase64String(Element.InnerText);
                                MemoryStream MS2 = new MemoryStream(DrawingBytes, 0, DrawingBytes.Length);
                                MS2.Write(DrawingBytes, 0, DrawingBytes.Length);
                                NewAi.Drawing = Image.FromStream(MS2);
                                break;
                            case "Radical":
                                NewAi.RadicalRatio = Convert.ToDouble(Element.InnerText);
                                break;
                            case "SkillPool":
                                foreach (XmlElement SkillElement in Element.ChildNodes)
                                {
                                    var TempSkill = App.SkillList.Find(P => P.Id == 
                                        Guid.Parse(SkillElement.Attributes["Id"].Value));
                                    var ScriptString = new string[2];
                                    foreach (XmlElement ScriptElement in SkillElement.ChildNodes)
                                        switch (ScriptElement.Name)
                                        {
                                            case "ConditionScript":
                                                ScriptString[0] = ScriptElement.InnerXml.ToString()
                                                    .Replace("<![CDATA[", "")
                                                    .Replace("]]>", "");
                                                break;
                                            case "TargetScript":
                                                ScriptString[1] = ScriptElement.InnerXml.ToString()
                                                    .Replace("<![CDATA[", "")
                                                    .Replace("]]>", "");
                                                break;
                                        }
                                    NewAi.SkillPool.Add(TempSkill, ScriptString);
                                }
                                break;
                        }
                    }

                    return NewAi;
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
