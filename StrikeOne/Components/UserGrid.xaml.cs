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
    /// UserGrid.xaml 的交互逻辑
    /// </summary>
    public partial class UserGrid : UserControl
    {
        public UserGrid()
        {
            InitializeComponent();
        }

        public void Init()
        {
            if (App.CurrentUser.Avator == null)
                AvatorImage.ImageSource = Resources["Icon_empty"] as BitmapImage;
            else
            {
                using (MemoryStream Stream = new MemoryStream())
                {
                    App.CurrentUser.Avator.Save(Stream, ImageFormat.Png);
                    BitmapImage Temp = new BitmapImage();
                    Temp.BeginInit();
                    Temp.CacheOption = BitmapCacheOption.OnLoad;
                    Temp.StreamSource = Stream;
                    Temp.EndInit();
                    AvatorImage.ImageSource = Temp;
                }
            }

            NameBox.Text = App.CurrentUser.Name;
            IntroBox.Text = App.CurrentUser.Introduction;

            MatchesBox.Text = "共参与" + App.CurrentUser.Records.Count + "场对战";

            VictoryCountText.Text = " - " + App.CurrentUser.Records.Count(O => O.Win) + "/" +
                               App.CurrentUser.Records.Count;
            double VictoryRatio = App.CurrentUser.VictoryRatio;
            VictoryRatioText.Text = "%" + (VictoryRatio * 100).ToString("0.0");
            VictoryRatioProgress.Value = VictoryRatio;
            if (App.CurrentUser.Records.Count == 0)
                VictoryRatioText.Foreground = Brushes.Gray;
            else if (VictoryRatio <= 0.25)
            {
                VictoryRatioText.Foreground = Brushes.Red;
                VictoryRatioProgress.Foreground = Brushes.Red;
            }
            else if (VictoryRatio <= 0.5)
            {
                VictoryRatioText.Foreground = Brushes.DarkOrange;
                VictoryRatioProgress.Foreground = Brushes.DarkOrange;
            }
            else if (VictoryRatio <= 0.75)
            {
                VictoryRatioText.Foreground = Brushes.YellowGreen;
                VictoryRatioProgress.Foreground = Brushes.YellowGreen;
            }
            else
            {
                VictoryRatioText.Foreground = Brushes.LimeGreen;
                VictoryRatioProgress.Foreground = Brushes.LimeGreen;
            }

            var DiceRolls = App.CurrentUser.Records
                .SelectMany(O => O.Participants.Find(P => P.Id == App.CurrentUser.Id).Rolls).ToList();
            double LuckRatio = App.CurrentUser.LuckRatio;
            LuckRatioText.Text = LuckRatio.ToString("0.0") + "%";
            LuckRatioProgress.Value = LuckRatio;
            if (DiceRolls.Count == 0)
                LuckRatioText.Foreground = Brushes.Gray;
            else if (LuckRatio <= 0.25)
            {
                LuckRatioText.Foreground = Brushes.Red;
                LuckRatioProgress.Foreground = Brushes.Red;
            }
            else if (LuckRatio <= 0.5)
            {
                LuckRatioText.Foreground = Brushes.DarkOrange;
                LuckRatioProgress.Foreground = Brushes.DarkOrange;
            }
            else if (LuckRatio <= 0.75)
            {
                LuckRatioText.Foreground = Brushes.YellowGreen;
                LuckRatioProgress.Foreground = Brushes.YellowGreen;
            }
            else
            {
                LuckRatioText.Foreground = Brushes.LimeGreen;
                LuckRatioProgress.Foreground = Brushes.LimeGreen;
            }
        }
    }
}
