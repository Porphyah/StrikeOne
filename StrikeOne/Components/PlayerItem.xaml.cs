using System;
using System.Collections.Generic;
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
using StrikeOne.Core;

namespace StrikeOne.Components
{
    /// <summary>
    /// PlayerItem.xaml 的交互逻辑
    /// </summary>
    public partial class PlayerItem : UserControl
    {
        public PlayerItem()
        {
            InitializeComponent();
        }
        public Player Player { private set; get; }

        public void Init(Player Source)
        {
            Player = Source;
            Source.BattleData.PlayerItem = this;

            GroupBrush.Color = Source.BattleData.Group.Color;
            HpProgress.Foreground = Brushes.LimeGreen;
            HpProgress.Value = Source.BattleData.CurrentHp.GetDouble();
            HpText.Text = Source.BattleData.CurrentHp.GetDouble().ToString("0");
            PlayerName.Text = Player.Name;
            GroupName.Text = Source.BattleData.Group.Name;
            GroupName.Foreground = new SolidColorBrush(Source.BattleData.Group.Color);
            SetStatus("Joined");
        }

        public void SetStatus(string Status)
        {
            StatusImg.Source = Resources[Status] as BitmapImage;
            switch (Status)
            {
                case "Joined":
                    StatusImg.ToolTip = "该角色正在等待下一回合。";
                    break;
                case "Ready":
                    StatusImg.ToolTip = "该角色正在执行当前回合。";
                    break;
                case "Breakdown":
                    StatusImg.ToolTip = "该角色正在遭受攻击。";
                    break;
                case "Nobody":
                    StatusImg.ToolTip = "该角色已阵亡。";
                    break;
            }
        }
        public void UpdateHp()
        {
            HpText.Text = Player.BattleData.CurrentHp.GetDouble() > 0 ? 
                Player.BattleData.CurrentHp.GetDouble().ToString("0") : "-";
            HpProgress.BeginAnimation(RangeBase.ValueProperty, new DoubleAnimation()
            {
                From = HpProgress.Value,
                To = Player.BattleData.CurrentHp.GetDouble(),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
            });

            double HpRatio = Player.BattleData.CurrentHp.GetDouble()/BattleData.TotalHp;
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

            HpText.Foreground = HpText.Foreground.Clone();
            ((SolidColorBrush)HpText.Foreground).BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation()
                {
                    From = ((SolidColorBrush)HpText.Foreground).Color,
                    To = Color,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                });
            HpProgress.Foreground = HpProgress.Foreground.Clone();
            ((SolidColorBrush)HpProgress.Foreground).BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation()
                {
                    From = ((SolidColorBrush)HpProgress.Foreground).Color,
                    To = Color,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                });
        }
    }
}
