using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StrikeOne.Core;

namespace StrikeOne.Components
{
    /// <summary>
    /// ParticipantItem.xaml 的交互逻辑
    /// </summary>
    public partial class ParticipantItem : UserControl
    {
        public ParticipantItem()
        {
            InitializeComponent();
        }
        public bool Local { set; get; }
        public int GroupIndex { set; get; }
        public Player Participant { set; get; }
        public Group Group { set; get; }
        public Action<Player> JoinSyncAction { set; private get; }
        public Action QuitSyncAction { set; private get; }

        public void Init(int GroupIndex, Group ParentGroup)
        {
            this.GroupIndex = GroupIndex;
            this.Group = ParentGroup;
            StatusImg.Source = Resources["Nobody"] as BitmapImage;
            StatusImg.ToolTip = "该角色槽当前正等待玩家的加入。";
            UserGrid.Visibility = Visibility.Hidden;
            EmptyText.Visibility = Visibility.Visible;
            if (!Group.Room.HasParticipate(App.CurrentUser.Id))
            {
                ActionButton.Style = Resources["DefaultGreenButtonStyle"] as Style;
                ActionButton.Content = "+";
                ActionButton.ToolTip = "将您的角色加入到该位置。";
            }
            else
                ActionButton.Visibility = Visibility.Hidden;
        }
        public void Init(Player User, int GroupIndex, Group ParentGroup)
        {
            this.GroupIndex = GroupIndex;
            this.Group = ParentGroup; 
            this.Participant = User;

            StatusImg.Source = Resources["Joined"] as BitmapImage;
            StatusImg.ToolTip = "该角色槽已有玩家加入，且通信良好。";
            UserGrid.Visibility = Visibility.Visible;
            EmptyText.Visibility = Visibility.Hidden;
            if (User.Avator != null)
                using (MemoryStream Stream = new MemoryStream())
                {
                    User.Avator.Save(Stream, ImageFormat.Png);
                    BitmapImage Temp = new BitmapImage();
                    Temp.BeginInit();
                    Temp.CacheOption = BitmapCacheOption.OnLoad;
                    Temp.StreamSource = Stream;
                    Temp.EndInit();
                    AvatorImage.ImageSource = Temp;
                }
            if (User is User && Group.Room.IsHost((User)User))
                HostImg.Visibility = Visibility.Visible;
            NameBox.Text = User.Name;

            SkillSelector.EnableSelect(User.Id == Group.Room.Host.Id);

            MatchesBox.Text = User.Records.Count.ToString();

            double VictoryRatio = User.VictoryRatio;
            VictoryRatioBox.Text = (VictoryRatio * 100).ToString("0.0") + "%";
            if (User.Records.Count == 0)
                VictoryRatioBox.Foreground = Brushes.Gray;
            else if (VictoryRatio <= 0.25)
                VictoryRatioBox.Foreground = Brushes.Red;
            else if (VictoryRatio <= 0.5)
                VictoryRatioBox.Foreground = Brushes.DarkOrange;
            else if (VictoryRatio <= 0.75)
                VictoryRatioBox.Foreground = Brushes.YellowGreen;
            else
                VictoryRatioBox.Foreground = Brushes.LimeGreen;

            double LuckRatio = User.LuckRatio;
            LuckRatioBox.Text = (LuckRatio * 100).ToString("0.0") + "%";
            if (User.Records.Count == 0)
                LuckRatioBox.Foreground = Brushes.Gray;
            else if (LuckRatio <= 0.25)
                LuckRatioBox.Foreground = Brushes.Red;
            else if (LuckRatio <= 0.5)
                LuckRatioBox.Foreground = Brushes.DarkOrange;
            else if (LuckRatio <= 0.75)
                LuckRatioBox.Foreground = Brushes.YellowGreen;
            else
                LuckRatioBox.Foreground = Brushes.LimeGreen;

            if (Participant.Id == App.CurrentUser.Id || Group.Room.IsHost(App.CurrentUser))
            {
                ActionButton.Style = Resources["DefaultRedButtonStyle"] as Style;
                ActionButton.Content = "×";
                ActionButton.ToolTip = "将角色：" + Participant.Name + "从当前位置移除。";
            }
            else
                ActionButton.Visibility = Visibility.Hidden;
        }
        public void Join(Player User, bool NeedTransmission = true)
        {
            if (User != null)
            {
                Participant = User;
                StatusImg.Source = Resources["Joined"] as BitmapImage;
                StatusImg.ToolTip = "该角色槽已有玩家加入，且通信良好。";
                UserGrid.Visibility = Visibility.Visible;
                EmptyText.Visibility = Visibility.Hidden;
                if (Participant.Avator != null)
                    using (MemoryStream Stream = new MemoryStream())
                    {
                        Participant.Avator.Save(Stream, ImageFormat.Png);
                        BitmapImage Temp = new BitmapImage();
                        Temp.BeginInit();
                        Temp.CacheOption = BitmapCacheOption.OnLoad;
                        Temp.StreamSource = Stream;
                        Temp.EndInit();
                        AvatorImage.ImageSource = Temp;
                    }
                if (User is User && Group.Room.IsHost((User)User))
                    HostImg.Visibility = Visibility.Visible;
                else if (User is AI)
                {
                    HostImg.Source = Resources["AI"] as BitmapImage;
                    HostImg.Visibility = Visibility.Visible;
                    HostImg.ToolTip = "AI标识";
                }
                NameBox.Text = Participant.Name;

                SkillSelector.EnableSelect(Participant.Id == App.CurrentUser.Id);

                MatchesBox.Text = Participant.Records.Count.ToString();

                double VictoryRatio = Participant.VictoryRatio;
                VictoryRatioBox.Text = (VictoryRatio * 100).ToString("0.0") + "%";
                if (Participant.Records.Count == 0)
                    VictoryRatioBox.Foreground = Brushes.Gray;
                else if (VictoryRatio <= 0.25)
                    VictoryRatioBox.Foreground = Brushes.Red;
                else if (VictoryRatio <= 0.5)
                    VictoryRatioBox.Foreground = Brushes.DarkOrange;
                else if (VictoryRatio <= 0.75)
                    VictoryRatioBox.Foreground = Brushes.YellowGreen;
                else
                    VictoryRatioBox.Foreground = Brushes.LimeGreen;

                double LuckRatio = Participant.LuckRatio;
                LuckRatioBox.Text = (LuckRatio * 100).ToString("0.0") + "%";
                if (Participant.Records.Count == 0)
                    LuckRatioBox.Foreground = Brushes.Gray;
                else if (LuckRatio <= 0.25)
                    LuckRatioBox.Foreground = Brushes.Red;
                else if (LuckRatio <= 0.5)
                    LuckRatioBox.Foreground = Brushes.DarkOrange;
                else if (LuckRatio <= 0.75)
                    LuckRatioBox.Foreground = Brushes.YellowGreen;
                else
                    LuckRatioBox.Foreground = Brushes.LimeGreen;

                if (Participant.Id == App.CurrentUser.Id || Group.Room.IsHost(App.CurrentUser))
                {
                    ActionButton.Style = Resources["DefaultRedButtonStyle"] as Style;
                    ActionButton.Content = "×";
                    ActionButton.ToolTip = "将角色：" + Participant.Name + "从当前位置移除。";
                }
                else
                    ActionButton.Visibility = Visibility.Hidden;

                if (NeedTransmission)
                {
                    if (Group.Room.IsHost(App.CurrentUser))
                        foreach (var Client in App.Server)
                            Client.SendAsync(Encoding.UTF8.GetBytes("ParticipateChanged|*|Add|*|" +
                                Participant.Id + "|*|" + Group.Name + "|*|" + GroupIndex));
                    else
                        App.Client.SendAsync(Encoding.UTF8.GetBytes("ParticipateChanged|*|Add|*|" + 
                            Participant.Id + "|*|" + Group.Name + "|*|" + GroupIndex));
                }
            }
            else
            {
                StatusImg.Source = Resources["Nobody"] as BitmapImage;
                StatusImg.ToolTip = "该角色槽当前正等待玩家的加入。";
                UserGrid.Visibility = Visibility.Hidden;
                EmptyText.Visibility = Visibility.Visible;
                if (!Group.Room.HasParticipate(App.CurrentUser.Id) || Local)
                {
                    ActionButton.Style = Resources["DefaultGreenButtonStyle"] as Style;
                    ActionButton.Content = "+";
                    ActionButton.ToolTip = Local ? "将您的角色或者一个AI加入到该位置。" : "将您的角色加入到该位置。";
                }
                else
                    ActionButton.Visibility = Visibility.Hidden;

                if (NeedTransmission)
                {
                    if (Group.Room.IsHost(App.CurrentUser))
                        foreach (var Client in App.Server)
                            Client.SendAsync(Encoding.UTF8.GetBytes("ParticipateChanged|*|Remove|*|" +
                                Participant.Id + "|*|" + Group.Name + "|*|" + GroupIndex));
                    else
                        App.Client.SendAsync(Encoding.UTF8.GetBytes("ParticipateChanged|*|Remove|*|" +
                            Participant.Id + "|*|" + Group.Name + "|*|" + GroupIndex));
                }

                Participant = null;
            }
        }

        public void Ready()
        {
            StatusImg.Source = Resources["Ready"] as BitmapImage;
            StatusImg.ToolTip = "该角色已做好了准备。";
            ActionButton.IsEnabled = false;
        }

        private void Action_Click(object Sender, RoutedEventArgs E)
        {
            if (Participant == null)
            {
                Group.Participants[GroupIndex] = App.CurrentUser;
                this.Join(App.CurrentUser);
                JoinSyncAction?.Invoke(App.CurrentUser);
            }
            else
            {
                if (Participant.Id != App.CurrentUser.Id)
                {
                    var Result = MessageBox.Show("确实要把角色：" + Participant.Name + "踢出房间么？\n" +
                                                 "选是，则将" + Participant.Name + "踢出房间；\n" +
                                                 "选否，则仅将" + Participant.Name + "移出当前角色槽；\n" +
                                                 "选取消，则返回不作任何操作。", "踢翻友谊的小船？", 
                                                 MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    switch (Result)
                    {
                        default:
                            return;
                        case MessageBoxResult.Yes:
                            Group.Participants[GroupIndex] = null;
                            this.Join(null);
                            QuitSyncAction?.Invoke();
                            break;
                        case MessageBoxResult.No:
                            Group.Participants[GroupIndex] = null;
                            this.Join(null);
                            QuitSyncAction?.Invoke();
                            break;
                    }
                }
                else
                {
                    if (MessageBox.Show("确实要将自己移出当前角色槽么？", "移出角色槽",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
                    Group.Participants[GroupIndex] = null;
                    this.Join(null);
                    QuitSyncAction?.Invoke();
                }
            }
        }
        private void LocalAction_Click(object Sender, RoutedEventArgs E)
        {
            if (Participant == null)
            {
                AiWindow AiWindow = new AiWindow();
                AiWindow.ShowDialog();
                if (AiWindow.Canceled) return;

                if (!AiWindow.IsSelectingAi)
                {
                    Group.Participants[GroupIndex] = App.CurrentUser;
                    this.Join(App.CurrentUser, false);
                    JoinSyncAction?.Invoke(App.CurrentUser);
                }
                else
                {
                    Group.Participants[GroupIndex] = AiWindow.SelectedAi;
                    this.Join(AiWindow.SelectedAi, false);
                    JoinSyncAction?.Invoke(AiWindow.SelectedAi);
                    SkillSelector.Select(AiWindow.SelectedAi.ChooseSkill());
                    //this.Ready();
                }
            }
            else
            {
                if (MessageBox.Show("确实要将" + (Participant.Id == App.CurrentUser.Id ? "自己" : "AI：" +  Participant.Name) + "移出当前角色槽么？", 
                    "移出角色槽", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
                Group.Participants[GroupIndex] = null;
                this.Join(null, false);
                QuitSyncAction?.Invoke();
            }
        }

        public void LocalInit(int GroupIndex, Group ParentGroup)
        {
            this.Local = true;
            this.GroupIndex = GroupIndex;
            this.Group = ParentGroup;
            StatusImg.Source = Resources["Nobody"] as BitmapImage;
            StatusImg.ToolTip = "该角色槽当前正等待玩家的加入。";
            UserGrid.Visibility = Visibility.Hidden;
            EmptyText.Visibility = Visibility.Visible;
            if (Participant == null)
            {
                ActionButton.Style = Resources["DefaultGreenButtonStyle"] as Style;
                ActionButton.Content = "+";
                ActionButton.ToolTip = "将您的角色或者一个AI加入到该位置。";
            }
            else
                ActionButton.Visibility = Visibility.Hidden;
            ActionButton.Click -= Action_Click;
            ActionButton.Click += LocalAction_Click;
        }
    }
}
