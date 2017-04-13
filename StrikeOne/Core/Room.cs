using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using StrikeOne.Core.Network;

namespace StrikeOne.Core
{
    [Serializable]
    public class Room : ISerializable
    {
        public Guid Id { set; get; }
        public string Name { set; get; }
        public string Description { set; get; }
        public User Host { set; get; }
        public bool LAN { set; get; }
        public bool Private { set; get; }
        public BattleType BattleType { set; get; }
        public Battlefield Battlefield { set; get; }
        public List<Group> Groups { get; } = new List<Group>();
        public List<Player> Members { get; } = new List<Player>();

        public Room() { }
        private Room(SerializationInfo info, StreamingContext context)
        {
            Id = info.GetValue<Guid>("Id");
            Name = info.GetString("Name");
            Description = info.GetString("Description");
            Host = info.GetValue<User>("Host");
            LAN = info.GetBoolean("Lan");
            Private = info.GetBoolean("Private");
            BattleType = info.GetValue<BattleType>("BattleType");
            Members = info.GetValue<List<Player>>("Members");
            Groups = info.GetValue<List<Group>>("Groups");
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id);
            info.AddValue("Name", Name);
            info.AddValue("Description", Description);
            info.AddValue("Host", Host);
            info.AddValue("Lan", LAN);
            info.AddValue("Private", Private);
            info.AddValue("BattleType", BattleType);
            info.AddValue("Members", Members);
            info.AddValue("Groups", Groups);
        }

        public bool HasJoined(Guid UserId)
        {
            return Members.Any(O => O.Id == UserId);
        }
        public bool HasParticipated(Guid UserId)
        {
            return Groups.SelectMany(O => O.Participants).Any(O =>
            {
                if (O != null)
                    return O.Id == UserId;
                return false;
            });
        }
        public bool IsHost(Player User)
        {
            return Host.Id == User.Id;
        }
        public void RemoveUser(Player User)
        {
            Members.Remove(User);
            Groups.ForEach(O =>
            {
                for (int i = 0; i < O.Capacity; i++)
                    if (O.Participants[i].Id == User.Id)
                        O.Participants[i] = null;
            });
        }

        public Player GetPlayer(Guid UserId)
        {
            return Members.Find(O => O.Id == UserId);
        }

        public void AddParticipate(Guid UserId, string GroupName, int GroupIndex)
        {
            Groups.Find(O => O.Name == GroupName).Participants[GroupIndex] =
                Members.Find(O => O.Id == UserId);
        }
        public void RemoveParticipate(Guid UserId)
        {
            foreach (var Group in Groups)
                for (int i = 0; i < Group.Capacity; i++)
                    if (Group.Participants[i] != null &&
                        Group.Participants[i].Id == UserId)
                        Group.Participants[i] = null;
        }
    }

    [Serializable]
    public enum BattleType
    {
        OneVsOne,
        TriangleMess,
        SquareMess,
        TwinningFight,
    }
}
