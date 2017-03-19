using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace StrikeOne.Core
{
    [Serializable]
    public class Group : ISerializable
    {
        public string Name { set; get; }
        public Room Room { set; get; }
        public Color Color { set; get; }
        public int Capacity {
            set { Participants = new Player[value]; }
            get { return Participants.Length; } }
        public Player[] Participants { private set; get; }

        private readonly Guid?[] ParticipantIds;

        public Group() { }
        private Group(SerializationInfo info, StreamingContext context)
        {
            Name = info.GetString("Name");
            Color = info.GetValue<Color>("Color");
            Capacity = info.GetInt32("Capacity");
            ParticipantIds = info.GetValue<Guid?[]>("Participants");
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Name);
            info.AddValue("Color", new[] { Color.A, Color.R, Color.G, Color.B });
            info.AddValue("Capacity", Capacity);
            info.AddValue("Participants", Participants.Select(O => O?.Id).ToArray());
        }

        public void InitParticipants()
        {
            Participants = ParticipantIds
                .Select(O => O != null ? Room.Members.Find(P => P.Id == O) : null)
                .ToArray();
        }
    }
}
