using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using StrikeOne.Components;
using StrikeOne.Core;
using GroupItem = StrikeOne.Components.GroupItem;

namespace StrikeOne
{
    /// <summary>
    /// LocalStrikePage.xaml 的交互逻辑
    /// </summary>
    public partial class LocalStrikePage : UserControl
    {
        public Action LeaveAction { set; private get; }
        public Room CurrentRoom { set; get; }

        public LocalStrikePage()
        {
            InitializeComponent();

            TitleGrid.Opacity = 0;
            ContentGrid.Opacity = 0;
        }

        public void PageEnter()
        {
            BattleTypeComboBox.Items.Add(new BattleTypeItem()
            {
                Height = 100, Width = 780,
                Image = { Source = Resources["OneVsOne"] as BitmapImage },
                TypeText = { Text = "One Vs One" },
                DescriptionText = { Text = "进行一对一的骰子对战。" },
                BattleType = BattleType.OneVsOne 
            });

            DoubleAnimation OpacityAnimation = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.75),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            };
            ThicknessAnimation MarginAnimation = new ThicknessAnimation()
            {
                From = new Thickness(
                    TitleGrid.Margin.Left - 50,
                    TitleGrid.Margin.Top,
                    TitleGrid.Margin.Right + 50,
                    TitleGrid.Margin.Bottom),
                To = TitleGrid.Margin,
                Duration = TimeSpan.FromSeconds(0.75),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            };
            MarginAnimation.Completed += delegate
            {
                ContentGrid.BeginAnimation(OpacityProperty, new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.75),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });
                ContentGrid.BeginAnimation(MarginProperty, new ThicknessAnimation()
                {
                    From = new Thickness(
                        ContentGrid.Margin.Left - 50,
                        ContentGrid.Margin.Top,
                        ContentGrid.Margin.Right + 50,
                        ContentGrid.Margin.Bottom),
                    To = ContentGrid.Margin,
                    Duration = TimeSpan.FromSeconds(0.75),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });
            };
            TitleGrid.BeginAnimation(OpacityProperty, OpacityAnimation);
            TitleGrid.BeginAnimation(MarginProperty, MarginAnimation);
        }

        public void PageLeave()
        {
            DoubleAnimation OpacityAnimation = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.75),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            };
            ThicknessAnimation MarginAnimation = new ThicknessAnimation()
            {
                From = ContentGrid.Margin,
                To = new Thickness(
                    ContentGrid.Margin.Left + 50,
                    ContentGrid.Margin.Top,
                    ContentGrid.Margin.Right - 50,
                    ContentGrid.Margin.Bottom),
                Duration = TimeSpan.FromSeconds(0.75),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            };
            MarginAnimation.Completed += delegate
            {
                DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.5) };
                Timer.Tick += delegate
                {
                    DoubleAnimation FadeAnimation = new DoubleAnimation()
                    {
                        From = 1,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(1),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                    };
                    FadeAnimation.Completed += delegate
                    {
                        LeaveAction?.Invoke();
                    };
                    this.BeginAnimation(OpacityProperty, FadeAnimation);
                    Timer.Stop();
                };
                TitleGrid.BeginAnimation(OpacityProperty, new DoubleAnimation()
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.75),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                });
                TitleGrid.BeginAnimation(MarginProperty, new ThicknessAnimation()
                {
                    From = TitleGrid.Margin,
                    To = new Thickness(
                        TitleGrid.Margin.Left + 50,
                        TitleGrid.Margin.Top,
                        TitleGrid.Margin.Right - 50,
                        TitleGrid.Margin.Bottom),
                    Duration = TimeSpan.FromSeconds(0.75),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                });
                Timer.Start();
            };
            ContentGrid.BeginAnimation(OpacityProperty, OpacityAnimation);
            ContentGrid.BeginAnimation(MarginProperty, MarginAnimation);
        }

        private void Active_Click(object Sender, RoutedEventArgs E)
        {
            if (GroupStack.Children.OfType<GroupItem>()
                .SelectMany(GroupItem => GroupItem.ParticipantStack.Children.OfType<ParticipantItem>())
                .First(O => O.Participant.Id == App.CurrentUser.Id).SkillSelector.SelectedSkill == null
                && MessageBox.Show("您还没有为自己选择技能，确定继续？", "开始对战",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            GroupStack.Children.OfType<GroupItem>()
                .SelectMany(GroupItem => GroupItem.ParticipantStack.Children.OfType<ParticipantItem>())
                .ToList().ForEach(O =>
                {
                    O.Participant.BattleData = new BattleData()
                    { Skill = O.SkillSelector.SelectedSkill };
                    O.Ready();
                });
            ActiveButton.IsEnabled = false;
            LeaveAction = delegate
            {
                MainWindow.Instance.HideInfoGrid();
                MainWindow.Instance.PrepareStrike(CurrentRoom);
            };
            this.PageLeave();
        }

        private void QuitRoom_Click(object Sender, RoutedEventArgs E)
        {
            LeaveAction = delegate
            {
                MainWindow.Instance.EnterUserPage(false);
            };
            this.PageLeave();
        }

        private void BattleTypeSelected(object Sender, SelectionChangedEventArgs E)
        {
            BattleTypeItem Item = BattleTypeComboBox.SelectedItem as BattleTypeItem;
            if (Item == null) return;

            CurrentRoom = new Room()
            {
                Id = Guid.NewGuid(),
                BattleType = Item.BattleType,
                Host = App.CurrentUser
            };
            switch (Item.BattleType)
            {
                case BattleType.OneVsOne:
                    CurrentRoom.Groups.Add(new Group()
                    {
                        Name = "Group A",
                        Color = Colors.DodgerBlue,
                        Room = CurrentRoom,
                        Capacity = 1
                    });
                    CurrentRoom.Groups.Add(new Group()
                    {
                        Name = "Group B",
                        Color = Colors.Red,
                        Room = CurrentRoom,
                        Capacity = 1
                    });


                    break;
            }

            foreach (var Group in CurrentRoom.Groups)
            {
                var GroupItem = new Components.GroupItem();
                GroupItem.JoinSyncAction = delegate (Player TargetUser)
                {
                    if (GroupStack.Children.OfType<Components.GroupItem>()
                        .All(O => O.ParticipantStack.Children.OfType<ParticipantItem>()
                            .All(P => P.Participant != null)))
                        ActiveButton.IsEnabled = true;
                    else
                        ActiveButton.IsEnabled = false;
                };
                GroupItem.QuitSyncAction = delegate
                {
                    ActiveButton.IsEnabled = false;
                };
                GroupItem.LocalInit(Group, CurrentRoom);
                GroupItem.Padding = new Thickness(0, 10, 0, 0);
                GroupStack.Children.Add(GroupItem);
            }
        }
    }
}
