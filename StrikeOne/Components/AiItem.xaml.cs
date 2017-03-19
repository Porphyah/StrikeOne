using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using StrikeOne.Core;

namespace StrikeOne.Components
{
    /// <summary>
    /// AiItem.xaml 的交互逻辑
    /// </summary>
    public partial class AiItem : UserControl
    {
        public AI Ai { set; get; }
        public bool IsSelected { private set; get; }
        public Action<AI> SelectSyncAction { set; private get; }

        public AiItem()
        {
            InitializeComponent();
        }

        public void Init(AI Ai)
        {
            this.Ai = Ai;
            AiName.Text = Ai.Name;
            RadicalText.Text = (Ai.RadicalRatio*100).ToString();
            AiDescription.Text = Ai.Introduction;

            using (MemoryStream Stream = new MemoryStream())
            {
                Ai.Drawing.Save(Stream, ImageFormat.Png);
                BitmapImage Temp = new BitmapImage();
                Temp.BeginInit();
                Temp.CacheOption = BitmapCacheOption.OnLoad;
                Temp.StreamSource = Stream;
                Temp.EndInit();
                Drawing.Source = Temp;
            }

            MatchesText.Text = "对战" + Ai.Records.Count + "场";

            double VictoryRatio = Ai.VictoryRatio;
            VictoryRatioText.Text = "%" + (VictoryRatio * 100).ToString("0.0");
            VictoryCountText.Text = " - " + Ai.Records.Count(O => O.Win) + "/" + Ai.Records.Count;
            VictoryRatioProgress.Value = VictoryRatio;
            if (Ai.Records.Count == 0)
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

            var DiceRolls = Ai.Records.SelectMany(O => O.Participants.Find(P => P.Id == App.CurrentUser.Id).Rolls).ToList();
            double LuckRatio = Ai.LuckRatio;
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

            foreach (var Skill in Ai.SkillPool.Keys)
            {
                BitmapSource Image;
                if (Skill.Image != null)
                    using (MemoryStream Stream = new MemoryStream())
                    {
                        Skill.Image.Save(Stream, ImageFormat.Png);
                        BitmapImage Temp = new BitmapImage();
                        Temp.BeginInit();
                        Temp.CacheOption = BitmapCacheOption.OnLoad;
                        Temp.StreamSource = Stream;
                        Temp.EndInit();
                        Image = Temp;
                    }
                else
                    Image = null;
                var WrapPanel = new WrapPanel()
                {
                    Children =
                    {
                        new Image
                        {
                            Height = 25,
                            Width = 25,
                            Source = Image
                        },
                        new TextBlock()
                        {
                            Text = Skill.Name,
                            Foreground = Brushes.White,
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                };
                SkillPool.Children.Add(WrapPanel);
            }
        }

        private void OnMouseEnter(object Sender, MouseEventArgs E)
        {
            if (IsSelected) return;
            FirstGradientStop.Color = Color.FromArgb(255, 15, 72, 128);
            SecondGradientStop.Color = Color.FromArgb(21, 15, 72, 128);
        }
        private void OnMouseLeave(object Sender, MouseEventArgs E)
        {
            if (IsSelected) return;
            FirstGradientStop.Color = Colors.Gray;
            SecondGradientStop.Color = Color.FromArgb(21, 0, 0, 0);
        }

        private void OnMouseDown(object Sender, MouseButtonEventArgs E)
        {
            Select(true);
        }

        public void Select(bool Flag)
        {
            if (Flag)
            {
                if (IsSelected) return;
                FirstGradientStop.Color = Color.FromArgb(255, 30, 144, 255);
                SecondGradientStop.Color = Color.FromArgb(21, 30, 144, 255);
                IsSelected = true;
                SelectSyncAction?.Invoke(this.Ai);
            }
            else
            {
                if (!IsSelected) return;
                FirstGradientStop.Color = Colors.Gray;
                SecondGradientStop.Color = Color.FromArgb(21, 0, 0, 0);
                IsSelected = false;
            }
        }
    }
}
