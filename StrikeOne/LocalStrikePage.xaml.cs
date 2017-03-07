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
                BattleType = Item.BattleType
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
                    foreach (var TempGroupItem in GroupStack.Children.OfType<Components.GroupItem>())
                        foreach (var ParticipantItem in TempGroupItem.ParticipantStack.Children.OfType<ParticipantItem>())
                        {
                            if ((ParticipantItem.Participant != null &&
                                 ParticipantItem.Participant.Id != TargetUser.Id) ||
                                (CurrentRoom.Host.Id == App.CurrentUser.Id && ParticipantItem.Participant == null))
                                ParticipantItem.ActionButton.Visibility = Visibility.Hidden;
                            else
                                ParticipantItem.ActionButton.Visibility = Visibility.Visible;
                        }
                };
                GroupItem.QuitSyncAction = delegate
                {
                    foreach (var TempGroupItem in GroupStack.Children.OfType<Components.GroupItem>())
                        foreach (var ParticipantItem in TempGroupItem.ParticipantStack.Children.OfType<ParticipantItem>())
                        {
                            if (ParticipantItem.Participant == null ||
                                (ParticipantItem.Participant != null && CurrentRoom.Host.Id == App.CurrentUser.Id))
                                ParticipantItem.ActionButton.Visibility = Visibility.Visible;
                            else
                                ParticipantItem.ActionButton.Visibility = Visibility.Hidden;
                        }
                };
                GroupItem.LocalInit(Group, CurrentRoom);
                GroupItem.Padding = new Thickness(0, 10, 0, 0);
                GroupStack.Children.Add(GroupItem);
            }
        }
    }
}
