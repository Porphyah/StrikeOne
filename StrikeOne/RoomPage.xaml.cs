using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using StrikeOne.Components;
using StrikeOne.Core;
using StrikeOne.Core.Network;

namespace StrikeOne
{
    /// <summary>
    /// RoomPage.xaml 的交互逻辑
    /// </summary>
    public partial class RoomPage : UserControl
    {
        private static readonly Dictionary<BattleType, string> BattleTypeDictionary = new Dictionary<BattleType, string>()
        {
            { BattleType.OneVsOne, "One vs One" }
        };

        public Room CurrentRoom { set; get; }
        public Action LeaveAction { set; private get; }
        public ChatWindow ChatWindow { get; }

        public RoomPage()
        {
            InitializeComponent();
            ChatWindow = new ChatWindow();

            TitleGrid.Opacity = 0;
            ContentGrid.Opacity = 0;
        }

        public void PageEnter(Room Target)
        {
            CurrentRoom = Target;
            if (CurrentRoom.IsHost(App.CurrentUser))
                App.Server.AcceptCompleted += ServerAccepted;
            ChatWindow.Closing += HideSyncAction;

            RoomNameLabel.Content = Target.Name;
            BattleTypeLabel.Content = BattleTypeDictionary[Target.BattleType];
            if (CurrentRoom.IsHost(App.CurrentUser))
            {
                ChatWindow.Title = Target.Name;
                ChatWindow.CurrentRoom = CurrentRoom;
                HostNameBox.Text = Target.Host.Name;
                HostIntroBox.Text = Target.Host.Introduction;
                if (Target.Host.Avator != null)
                    using (MemoryStream Stream = new MemoryStream())
                    {
                        Target.Host.Avator.Save(Stream, Target.Host.AvatorFormat);
                        BitmapImage Temp = new BitmapImage();
                        Temp.BeginInit();
                        Temp.CacheOption = BitmapCacheOption.OnLoad;
                        Temp.StreamSource = Stream;
                        Temp.EndInit();
                        AvatorImage.ImageSource = Temp;
                    }
                DescriptionBlock.Text = Target.Description;

                foreach (var Group in Target.Groups)
                {
                    var GroupItem = new Components.GroupItem();
                    GroupItem.JoinSyncAction = delegate(Player TargetUser)
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
                    GroupItem.Init(Group, Target);
                    GroupItem.Padding = new Thickness(0, 10, 0, 0);
                    GroupStack.Children.Add(GroupItem);
                }
            }
            else if (App.Client["ReadyToReceive"] != null && (bool) App.Client["ReadyToReceive"])
            {
                App.Client.Send(Encoding.UTF8.GetBytes("ReadyToReceive"));
                App.Client["ReadyToReceive"] = null;
                App.Client.ReceiveCompleted += BeginUpdateRoom;
            }
            else
                App.Client.ReceiveCompleted += ReadyToUpdateRoom;

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
                if (CurrentRoom.IsHost(App.CurrentUser))
                {
                    ContentGrid.Visibility = Visibility.Visible;
                    ContentGrid.BeginAnimation(OpacityProperty, new DoubleAnimation()
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.75),
                        EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut}
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
                        EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut}
                    });
                }
                else
                {
                    WaitingGrid.Visibility = Visibility.Visible;
                    WaitingGrid.BeginAnimation(OpacityProperty, new DoubleAnimation()
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.75),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    });
                    WaitingGrid.BeginAnimation(MarginProperty, new ThicknessAnimation()
                    {
                        From = new Thickness(
                            WaitingGrid.Margin.Left - 50,
                            WaitingGrid.Margin.Top,
                            WaitingGrid.Margin.Right + 50,
                            WaitingGrid.Margin.Bottom),
                        To = WaitingGrid.Margin,
                        Duration = TimeSpan.FromSeconds(0.75),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    });
                }
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

        private void EndWaiting(Room Target)
        {
            ChatWindow.Title = Target.Name;
            ChatWindow.CurrentRoom = CurrentRoom;
            RoomNameLabel.Content = Target.Name;
            BattleTypeLabel.Content = BattleTypeDictionary[Target.BattleType];
            HostNameBox.Text = Target.Host.Name;
            HostIntroBox.Text = Target.Host.Introduction;
            if (Target.Host.Avator != null)
                using (MemoryStream Stream = new MemoryStream())
                {
                    Target.Host.Avator.Save(Stream, Target.Host.AvatorFormat);
                    BitmapImage Temp = new BitmapImage();
                    Temp.BeginInit();
                    Temp.CacheOption = BitmapCacheOption.OnLoad;
                    Temp.StreamSource = Stream;
                    Temp.EndInit();
                    AvatorImage.ImageSource = Temp;
                }
            DescriptionBlock.Text = Target.Description;

            foreach (var Group in Target.Groups)
            {
                var GroupItem = new Components.GroupItem();
                GroupItem.JoinSyncAction = delegate (Player TargetUser)
                {
                    foreach (var TempGroupItem in GroupStack.Children.OfType<Components.GroupItem>())
                        foreach (var ParticipantItem in TempGroupItem.ParticipantStack.Children.OfType<ParticipantItem>())
                        {
                            if ((ParticipantItem.Participant != null && ParticipantItem.Participant.Id != TargetUser.Id) ||
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
                GroupItem.Init(Group, Target);
                GroupItem.Padding = new Thickness(0, 10, 0, 0);
                GroupStack.Children.Add(GroupItem);
            }

            DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.5) };
            Timer.Tick += delegate
            {
                WaitingGrid.Visibility = Visibility.Hidden;
                ContentGrid.Visibility = Visibility.Visible;
                ContentGrid.BeginAnimation(OpacityProperty, new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.5),
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
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });
                Timer.Stop();
            };
            WaitingGrid.BeginAnimation(OpacityProperty, new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            });
            WaitingGrid.BeginAnimation(MarginProperty, new ThicknessAnimation()
            {
                From = WaitingGrid.Margin,
                To = new Thickness(
                    WaitingGrid.Margin.Left - 50,
                    WaitingGrid.Margin.Top,
                    WaitingGrid.Margin.Right + 50,
                    WaitingGrid.Margin.Bottom),
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            });
            Timer.Start();
        }

        private void QuitRoom_Click(object Sender, RoutedEventArgs E)
        {
            if (App.CurrentUser.Id == CurrentRoom.Host.Id)
            {
                if (MessageBox.Show("作为一名房主，如果您退出房间，则意味着房间将被解散。" +
                                    "其他加入该房间的角色用户将被强制踢出。您确定要解散房间吗？", "退出房间",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) return;
                foreach (var TempGroupItem in GroupStack.Children.OfType<Components.GroupItem>())
                    foreach (var ParticipantItem in TempGroupItem.ParticipantStack.Children.OfType<ParticipantItem>())
                        ParticipantItem.Join(null);
                CurrentRoom.Groups.ForEach(O =>
                {
                    for (int i = 0; i < O.Capacity; i++)
                        O.Participants[i] = null;
                });
                
                LeaveAction = delegate
                {
                    App.CurrentRoom = null;
                    App.Server.Stop();
                    App.Server = null;
                    MainWindow.Instance.EnterUserPage(false);
                };
                ChatWindow.AllowToClose = true;
                ChatWindow.Closing -= HideSyncAction;
                ChatWindow.Close();
                this.PageLeave();
            }
            else
            {
                if (MessageBox.Show("您确定要放弃这场比赛并退出房间吗？", "退出房间",
                   MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) return;
                foreach (var TempGroupItem in GroupStack.Children.OfType<Components.GroupItem>())
                    foreach (var ParticipantItem in TempGroupItem.ParticipantStack.Children.OfType<ParticipantItem>())
                        if (ParticipantItem.Participant != null &&
                            ParticipantItem.Participant.Id == App.CurrentUser.Id)
                            ParticipantItem.Join(null);
                CurrentRoom.RemoveUser(App.CurrentUser);
                App.CurrentRoom = null;
                LeaveAction = delegate { MainWindow.Instance.EnterUserPage(false); };
                ChatWindow.AllowToClose = true;
                ChatWindow.Closing -= HideSyncAction;
                ChatWindow.Close();
                this.PageLeave();
            }
        }

        private void Invite_Click(object Sender, RoutedEventArgs E)
        {
            string Code = (CurrentRoom.LAN ? App.LanIpAddress : App.WanIpAddress) + ";" +
                CurrentRoom.Id + ";" + CurrentRoom.Name + ";" + CurrentRoom.Host.Name;
            InvitationWindow Dialog = new InvitationWindow();
            Dialog.Init(Code);
            Dialog.Show();
        }

        private void OpenChatWindow(object Sender, RoutedEventArgs E)
        {
            ChatWindow.Show();
        }
        private void CloseChatWindow(object Sender, RoutedEventArgs E)
        {
            ChatWindow.Hide();
        }
        private void HideSyncAction(object Sender, System.ComponentModel.CancelEventArgs E)
        {
            ChatSwitch.IsChecked = false;
        }

        private void Active_Click(object Sender, RoutedEventArgs E)
        {
            if (!CurrentRoom.HasJoined(App.CurrentUser.Id) || App.CurrentUser.Id != CurrentRoom.Host.Id)
            {
                MessageBox.Show("您当前没有加入任何一个角色槽，也不是该房间的房主，故无法发出准备就绪之命令。",
                    "准备就绪？", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ActiveButton.Content = "等待中...";
            ActiveButton.IsEnabled = false;
            ActiveButton.ToolTip = "正等待所有加入角色槽的玩家准备就绪。";

            foreach (var TempGroupItem in GroupStack.Children.OfType<Components.GroupItem>())
                foreach (var ParticipantItem in TempGroupItem.ParticipantStack.Children.OfType<ParticipantItem>())
                    if (ParticipantItem.Participant != null && ParticipantItem.Participant.Id == App.CurrentUser.Id)
                        ParticipantItem.Ready();
            DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.35) };
            Timer.Tick += delegate
            {
                ActiveButton.Foreground = new SolidColorBrush(Color.FromRgb(173, 173, 173)); 
                Timer.Stop();
            };
            Timer.Start();
        }

        #region 服务器端
        private void ServerAccepted(object Sender, SocketEventArgs E)
        {
            MessageBox.Show("连接成功。", "来自" + E.Socket["RemoteEndPoint"] + "的信息",
                MessageBoxButton.OK, MessageBoxImage.Information);
            E.Socket.Send(CurrentRoom);
            E.Socket.ReceiveCompleted += LetInReceived;
        }
        private void LetInReceived(object Sender, SocketEventArgs E)
        {
            string[] Fragments = Encoding.UTF8.GetString(E.Data).Split
                (new[] { "|*|" }, StringSplitOptions.RemoveEmptyEntries);
            switch (Fragments[0])
            {
                case "LetMeIn":
                    E.Socket.ReceiveCompleted -= LetInReceived;
                    E.Socket["UserName"] = Fragments[1];
                    E.Socket.Send(Encoding.UTF8.GetBytes("Confirm"));
                    E.Socket.ReceiveCompleted += BeginGetUserData;
                    break;
                case "Disconnect":
                    E.Socket.DisconnectAsync();
                    break;
            }
        }
        private void BeginGetUserData(object Sender, SocketEventArgs E)
        {
            int DataLength = int.Parse(Encoding.UTF8.GetString(E.Data).Split(new[] { "|*|" },
                StringSplitOptions.RemoveEmptyEntries)[1]);
            E.Socket.BeginReceive(DataLength);
            this.Dispatcher.Invoke(() =>
            E.Socket["Progress"] = ChatWindow.BeginProgress("新玩家角色：正在接入" + 
               E.Socket["UserName"] +  "的信息...", DataLength));

            E.Socket.ReceiveCompleted -= BeginGetUserData;
            E.Socket.DataReceiver.ReceiveOnce += ReceiveUserData;
            E.Socket.ReceiveCompleted += EndGetUserData;
        }
        private void ReceiveUserData(object Sender, SocketDataReceiver.ReceiveOnceEventArgs E)
        {
            this.Dispatcher.Invoke(() =>
            {
                var ProgressItem = ((TcpListenerClient) E.Socket)["Progress"] as ProgressItem;
                ProgressItem.SetProgress(E.CumulativeLength);
            });
        }
        private void EndGetUserData(object Sender, SocketEventArgs E)
        {
            var NewUser = E.Socket.EndReceive<User>();
            E.Socket["User"] = NewUser;
            CurrentRoom.Members.Add(NewUser);
            E.Socket["Progress"] = null;
            E.Socket["UserName"] = null;
            //E.Socket.Send(Encoding.UTF8.GetBytes("GetUserDataCompleted"));
            E.Socket.Send(Encoding.UTF8.GetBytes("PrepareToReceive"));
            this.Dispatcher.Invoke(() =>
                ChatWindow.SendMessageAlone("等待" + NewUser.Name + "更新房间信息..."));
            E.Socket.DataReceiver.ReceiveOnce -= ReceiveUserData;
            E.Socket.ReceiveCompleted -= EndGetUserData;
            E.Socket.ReceiveCompleted += SendRoomData;
        }

        private void SendRoomData(object Sender, SocketEventArgs E)
        {
            E.Socket.Send(CurrentRoom);
            E.Socket.ReceiveCompleted -= SendRoomData;
            E.Socket.ReceiveCompleted += FinishToWork;
        }
        private void FinishToWork(object Sender, SocketEventArgs E)
        {
            this.Dispatcher.Invoke(() =>
                ChatWindow.SendMessage("玩家：" + ((TcpListenerClient)E.Socket)["User"] + "已上线！"));

            E.Socket.ReceiveCompleted -= FinishToWork;
            E.Socket.ReceiveCompleted += ChatWindow.MessageReceived;
            E.Socket.ReceiveCompleted += ParticipateChanged;
        }

        private void ParticipateChanged(object Sender, SocketEventArgs E)
        {
            string Data = Encoding.UTF8.GetString(E.Data);
            string[] Fragments = Data.Split(new[] { "|*|" },
                StringSplitOptions.RemoveEmptyEntries);
            if (Fragments[0] != "ParticipateChanged") return;

            switch (Fragments[1])
            {
                case "Add":
                    CurrentRoom.AddParticipate(Guid.Parse(Fragments[2]), 
                        Fragments[3], int.Parse(Fragments[4]));
                    this.Dispatcher.Invoke(() =>
                    {
                        ChatWindow.SendMessage("玩家角色：" + CurrentRoom.GetPlayer(Guid.Parse(Fragments[2]))
                                               + "已加入队伍：" + Fragments[3] + "的" + (int.Parse(Fragments[4]) + 1) + "号位。");
                        GroupStack.Children.OfType<Components.GroupItem>().First(O => O.Group.Name == Fragments[3])
                            .ParticipantStack.Children.OfType<ParticipantItem>()
                            .First(O => O.GroupIndex == int.Parse(Fragments[4]))
                            .Join(CurrentRoom.GetPlayer(Guid.Parse(Fragments[2])), false);
                    });
                    break;
                case "Remove":
                    CurrentRoom.RemoveParticipate(Guid.Parse(Fragments[2]));
                    this.Dispatcher.Invoke(() =>
                    {
                        ChatWindow.SendMessage("玩家角色：" + CurrentRoom.GetPlayer(Guid.Parse(Fragments[2]))
                                               + "已退出队伍：" + Fragments[3] + "的" + (int.Parse(Fragments[4]) + 1) + "号位。");
                        GroupStack.Children.OfType<Components.GroupItem>().First(O => O.Group.Name == Fragments[3])
                            .ParticipantStack.Children.OfType<ParticipantItem>()
                            .First(O => O.GroupIndex == int.Parse(Fragments[4]))
                            .Join(null, false);
                    });
                    break;
            }

            if (CurrentRoom.IsHost(App.CurrentUser))
            {
                var From = E.Socket["User"] as User;
                foreach (var Client in App.Server
                    .Where(Client => !From.Equals(Client["User"])))
                    Client.SendAsync(E.Data);
            }
        }
        #endregion

        #region 客户端
        private void ReadyToUpdateRoom(object Sender, SocketEventArgs E)
        {
            E.Socket.Send(Encoding.UTF8.GetBytes("ReadyToReceive"));
            E.Socket["ReadyToReceive"] = null;
            E.Socket.ReceiveCompleted -= ReadyToUpdateRoom;
            E.Socket.ReceiveCompleted += BeginUpdateRoom;
        }

        private void BeginUpdateRoom(object Sender, SocketEventArgs E)
        {
            int DataLength = int.Parse(Encoding.UTF8.GetString(E.Data).Split(new[] { "|*|" },
                StringSplitOptions.RemoveEmptyEntries)[1]);
            E.Socket.BeginReceive(DataLength);
            this.Dispatcher.Invoke(() =>
                WaitText.Text = "正在更新房间信息...");

            E.Socket.ReceiveCompleted -= BeginUpdateRoom;
            E.Socket.DataReceiver.ReceiveOnce += ReceiveRoomData;
            E.Socket.ReceiveCompleted += EndUpdateRoom;
        }
        private void ReceiveRoomData(object Sender, SocketDataReceiver.ReceiveOnceEventArgs E)
        {
            double Progress = E.CumulativeLength / (double)E.TotalLength * 100;
            this.Dispatcher.Invoke(() =>
                WaitText.Text = "正在更新房间信息...（" + Progress.ToString("0.0") + "%）");
        }
        private void EndUpdateRoom(object Sender, SocketEventArgs E)
        {
            CurrentRoom = E.Socket.EndReceive<Room>();
            this.Dispatcher.Invoke(() => 
                EndWaiting(CurrentRoom));
            E.Socket.Send(Encoding.UTF8.GetBytes("Completed"));

            E.Socket.DataReceiver.ReceiveOnce -= ReceiveRoomData;
            E.Socket.ReceiveCompleted -= EndUpdateRoom;
            E.Socket.ReceiveCompleted += ChatWindow.MessageReceived;
            E.Socket.ReceiveCompleted += ParticipateChanged;
        }
        #endregion

    }
}
