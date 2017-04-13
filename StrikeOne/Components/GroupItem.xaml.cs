using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using StrikeOne.Core;

namespace StrikeOne.Components
{
    /// <summary>
    /// GroupItem.xaml 的交互逻辑
    /// </summary>
    public partial class GroupItem : UserControl
    {
        public GroupItem()
        {
            InitializeComponent();
        }

        public Group Group { set; get; }
        public Room Room { set; get; }
        public Action<Player> JoinSyncAction { set; private get; }
        public Action QuitSyncAction { set; private get; }

        public void Init(Group Target, Room ParentRoom)
        {
            this.Group = Target;
            this.Room = ParentRoom;

            GlowEffect.Color = Target.Color;
            GroupName.Foreground = new SolidColorBrush(Target.Color);
            GroupName.Text = Target.Name;

            for (int i = 0; i < Target.Capacity; i++)
            {
                var ParticipantItem = new ParticipantItem()
                { Height = 50 };
                if (Target.Participants[i] != null)
                    ParticipantItem.Init(Target.Participants[i], i, Target);
                else
                    ParticipantItem.Init(i, Target);
                ParticipantItem.JoinSyncAction = JoinSyncAction;
                ParticipantItem.QuitSyncAction = QuitSyncAction;
                ParticipantStack.Children.Add(ParticipantItem);
            }
            this.Height = 50 + ParticipantStack.Children.Count*50;
        }
        public void LocalInit(Group Target, Room ParentRoom)
        {
            this.Group = Target;
            this.Room = ParentRoom;

            GlowEffect.Color = Target.Color;
            GroupName.Foreground = new SolidColorBrush(Target.Color);
            GroupName.Text = Target.Name;

            for (int i = 0; i < Target.Capacity; i++)
            {
                var ParticipantItem = new ParticipantItem()
                { Height = 50 };
                ParticipantItem.LocalInit(i, Target);
                ParticipantItem.JoinSyncAction = JoinSyncAction;
                ParticipantItem.QuitSyncAction = QuitSyncAction;
                ParticipantStack.Children.Add(ParticipantItem);
            }
            this.Height = 50 + ParticipantStack.Children.Count * 50;
        }
    }
}
