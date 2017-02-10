using System;
using System.Collections.Generic;
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
        public User Participant { set; get; }

        public void Init()
        {
            StatusImg.Source = Resources["Nobody"] as BitmapImage;
            StatusImg.ToolTip = "该角色槽当前正等待其他玩家的加入。";
            UserGrid.Visibility = Visibility.Hidden;
            EmptyText.Visibility = Visibility.Visible;
        }
        public void Init(User User, bool Host = false)
        {
            Participant = User;

            StatusImg.Source = Resources["Joined"] as BitmapImage;
            StatusImg.ToolTip = "该角色槽已有玩家加入，且通信良好。";
            UserGrid.Visibility = Visibility.Visible;
            EmptyText.Visibility = Visibility.Hidden;
            if (User.Avator != null)
                using (MemoryStream Stream = new MemoryStream())
                {
                    User.Avator.Save(Stream, User.AvatorFormat);
                    BitmapImage Temp = new BitmapImage();
                    Temp.BeginInit();
                    Temp.CacheOption = BitmapCacheOption.OnLoad;
                    Temp.StreamSource = Stream;
                    Temp.EndInit();
                    AvatorImage.ImageSource = Temp;
                }
            if (Host) HostImg.Visibility = Visibility.Visible;
            NameBox.Text = User.Name;

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
        }
    }
}
