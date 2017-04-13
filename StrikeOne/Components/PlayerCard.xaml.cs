using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using StrikeOne.Core;

namespace StrikeOne.Components
{
    /// <summary>
    /// PlayerCard.xaml 的交互逻辑
    /// </summary>
    public partial class PlayerCard : UserControl
    {
        public PlayerCard()
        {
            InitializeComponent();
        }
        public Player Player { private set; get; }

        public void Init(Player Source)
        {
            Player = Source;
            Source.BattleData.PlayerCard = this;

            BackgroundEffect.Color = Player.BattleData.Group.Color;
            PlayerName.Text = Player.Name;
            if (Player.Avator != null)
                using (MemoryStream Stream = new MemoryStream())
                {
                    Player.Avator.Save(Stream, ImageFormat.Png);
                    BitmapImage Temp = new BitmapImage();
                    Temp.BeginInit();
                    Temp.CacheOption = BitmapCacheOption.OnLoad;
                    Temp.StreamSource = Stream;
                    Temp.EndInit();
                    AvatorImage.ImageSource = Temp;
                }
            HealthText.Text = Player.BattleData.CurrentHp.ToString();
            HealthText.Foreground = Brushes.LimeGreen;
            LuckText.Text = (Player.BattleData.Luck*100).ToString("0.0") + "%";
            LuckText.Foreground = Brushes.Gray;
            Luck.Value = 0.0;
            Luck.Foreground = new SolidColorBrush(Player.BattleData.Group.Color);
            if (Player.BattleData.Skill != null)
            {
                Skill Skill = Player.BattleData.Skill;
                if (Skill.Image != null)
                    using (MemoryStream Stream = new MemoryStream())
                    {
                        Skill.Image.Save(Stream, ImageFormat.Png);
                        BitmapImage Temp = new BitmapImage();
                        Temp.BeginInit();
                        Temp.CacheOption = BitmapCacheOption.OnLoad;
                        Temp.StreamSource = Stream;
                        Temp.EndInit();
                        SkillImage.Source = Temp;
                    }
                else
                    SkillImage.Source = null;
                SkillName.Text = Skill.Name;
            }
            else
            {
                SkillImage.Source = null;
                SkillName.Text = "No Skill";
            }
            SetStatus("Joined");
        }
        public void Collapse()
        {
            InfoGrid.Visibility = Visibility.Hidden;
            InfoGrid.Opacity = 0;
            this.Width = 100;
        }
        public void Expand()
        {
            DoubleAnimation WidthAnimation = new DoubleAnimation()
            {
                From = this.Width,
                To = 300,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
            };
            WidthAnimation.Completed += delegate
            {
                InfoGrid.Visibility = Visibility.Visible;
                InfoGrid.BeginAnimation(OpacityProperty, new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseInOut}
                });
            };
            this.BeginAnimation(WidthProperty, WidthAnimation);
        }

        public void SetStatus(string Status)
        {
            StatusImage.Source = Resources[Status] as BitmapImage;
            switch (Status)
            {
                case "Joined":
                    StatusImage.ToolTip = "该角色正在等待下一回合。";
                    break;
                case "Ready":
                    StatusImage.ToolTip = "该角色正在执行当前回合。";
                    break;
                case "Breakdown":
                    StatusImage.ToolTip = "该角色正在遭受攻击。";
                    break;
                case "Nobody":
                    StatusImage.ToolTip = "该角色已阵亡。";
                    break;
            }
        }
        public void SetAction(string Name, string Image, string Description, int? Probability)
        {
            CurrentAction.Init(Name, Image, Description, Probability);
            ThicknessAnimation ShowActionBorderAnimation = new ThicknessAnimation()
            {
                From = ActionBorder.Margin,
                To = new Thickness(0, 0, 0, 0),
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            };
            ShowActionBorderAnimation.Completed += delegate
            {
                DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(3) };
                Timer.Tick += delegate
                {
                    DoubleAnimation OpacityAnimation = new DoubleAnimation()
                    {
                        From = CurrentAction.Opacity,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.3),
                        EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseIn}
                    };
                    OpacityAnimation.Completed += delegate
                    {
                        CurrentAction.Visibility = Visibility.Hidden;
                        ActionBorder.BeginAnimation(MarginProperty, new ThicknessAnimation()
                        {
                            From = ActionBorder.Margin,
                            To = new Thickness(0, 0, 200, 0),
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                        });
                    };
                    CurrentAction.BeginAnimation(OpacityProperty, OpacityAnimation);
                    Timer.Stop();
                };
                CurrentAction.Visibility = Visibility.Visible;
                CurrentAction.BeginAnimation(OpacityProperty, new DoubleAnimation()
                {
                    From = CurrentAction.Opacity,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });
                Timer.Start();
            };
            ActionBorder.BeginAnimation(MarginProperty, ShowActionBorderAnimation);
        }
        public void SetAction(string Name, System.Drawing.Image Image, string Description, int? Probability)
        {
            CurrentAction.Init(Name, Image, Description, Probability);
            ThicknessAnimation ShowActionBorderAnimation = new ThicknessAnimation()
            {
                From = ActionBorder.Margin,
                To = new Thickness(0, 0, 0, 0),
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            };
            ShowActionBorderAnimation.Completed += delegate
            {
                DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(3) };
                Timer.Tick += delegate
                {
                    DoubleAnimation OpacityAnimation = new DoubleAnimation()
                    {
                        From = CurrentAction.Opacity,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.3),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                    };
                    OpacityAnimation.Completed += delegate
                    {
                        CurrentAction.Visibility = Visibility.Hidden;
                        ActionBorder.BeginAnimation(MarginProperty, new ThicknessAnimation()
                        {
                            From = ActionBorder.Margin,
                            To = new Thickness(0, 0, 200, 0),
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                        });
                    };
                    CurrentAction.BeginAnimation(OpacityProperty, OpacityAnimation);
                    Timer.Stop();
                };
                CurrentAction.Visibility = Visibility.Visible;
                CurrentAction.BeginAnimation(OpacityProperty, new DoubleAnimation()
                {
                    From = CurrentAction.Opacity,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });
                Timer.Start();
            };
            ActionBorder.BeginAnimation(MarginProperty, ShowActionBorderAnimation);
        }
        public void UpdateHp()
        {
            HealthText.Text = Player.BattleData.CurrentHp.GetDouble() > 0 ? 
                Player.BattleData.CurrentHp.GetDouble().ToString("0") : "-" ;
            Hp.BeginAnimation(RangeBase.ValueProperty, new DoubleAnimation()
            {
                From = Hp.Value,
                To = Player.BattleData.CurrentHp.GetDouble(),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
            });

            double HpRatio = Player.BattleData.CurrentHp.GetDouble() / BattleData.TotalHp;
            Color Color;
            if (Player.BattleData.CurrentHp.GetInt() == 0)
                Color = Colors.Gray;
            else if (HpRatio <= 0.25)
                Color = Colors.Red;
            else if (HpRatio <= 0.5)
                Color = Colors.DarkOrange;
            else if (HpRatio <= 0.75)
                Color = Colors.YellowGreen;
            else
                Color = Colors.LimeGreen;

            HealthText.Foreground = HealthText.Foreground.Clone();
            ((SolidColorBrush)HealthText.Foreground).BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation()
                {
                    From = ((SolidColorBrush)HealthText.Foreground).Color,
                    To = Color,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                });
            Hp.Foreground = Hp.Foreground.Clone();
            ((SolidColorBrush)Hp.Foreground).BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation()
                {
                    From = ((SolidColorBrush)Hp.Foreground).Color,
                    To = Color,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                });
        }
        public void UpdateLuck()
        {
            LuckText.Text = (Player.BattleData.Luck*100).ToString("0.0") + "%";
            if (Player.BattleData.Luck <= 0.25)
                LuckText.Foreground = Brushes.Red;
            else if (Player.BattleData.Luck <= 0.5)
                LuckText.Foreground = Brushes.DarkOrange;
            else if (Player.BattleData.Luck <= 0.75)
                LuckText.Foreground = Brushes.YellowGreen;
            else
                LuckText.Foreground = Brushes.LimeGreen;
            Luck.BeginAnimation(RangeBase.ValueProperty, new DoubleAnimation()
            {
                From = Luck.Value,
                To = Player.BattleData.Luck,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
            });
        }
    }
}
