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
    /// StatisticsPage.xaml 的交互逻辑
    /// </summary>
    public partial class StatisticsPage : UserControl
    {
        public StatisticsPage()
        {
            InitializeComponent();
        }

        public Action LeaveAction { set; get; }
        private Battlefield Battlefield { set; get; }

        public void PageEnter(Battlefield Battlefield)
        {
            this.Battlefield = Battlefield;

            for (int i = 0; i < ContentGrid.Children.Count; i++)
            {
                var NaviControl = new NavigationControl()
                {
                    Index = i,
                    Height = 10, Width = 10,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(-5 * ContentGrid.Children.Count - 
                        2.5 * (ContentGrid.Children.Count - 1) + 15 * i, 0,
                        5 * ContentGrid.Children.Count +
                        2.5 * (ContentGrid.Children.Count - 1) - 15 * i, 0),
                    RelatedGrid = ContentGrid.Children[i] as Grid
                };
                NaviControl.SelectAction = delegate
                {
                    if (NavigationGrid.Children.OfType<NavigationControl>().Any(O => O.IsSelected))
                    {
                        var Before = NavigationGrid.Children.OfType<NavigationControl>().First(O => O.IsSelected);
                        var After = NaviControl;
                        var Timer = new DispatcherTimer() {Interval = TimeSpan.FromSeconds(1)};
                        Timer.Tick += delegate
                        {
                            Before.RelatedGrid.Visibility = Visibility.Hidden;
                            After.RelatedGrid.Visibility = Visibility.Visible;
                            After.RelatedGrid.BeginAnimation(OpacityProperty, new DoubleAnimation()
                            {
                                From = 0,
                                To = 1,
                                Duration = TimeSpan.FromSeconds(1),
                                EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut}
                            });
                            After.RelatedGrid.BeginAnimation(MarginProperty, new ThicknessAnimation()
                            {
                                From = After.Index >= Before.Index ? new Thickness(
                                    After.RelatedGrid.Margin.Left - 100,
                                    After.RelatedGrid.Margin.Top,
                                    After.RelatedGrid.Margin.Right + 100,
                                    After.RelatedGrid.Margin.Bottom) : new Thickness(
                                    After.RelatedGrid.Margin.Left + 100,
                                    After.RelatedGrid.Margin.Top,
                                    After.RelatedGrid.Margin.Right - 100,
                                    After.RelatedGrid.Margin.Bottom),
                                To = After.RelatedGrid.Margin,
                                Duration = TimeSpan.FromSeconds(1),
                                EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut},
                            });
                            Timer.Stop();
                        };

                        Before.RelatedGrid.BeginAnimation(OpacityProperty, new DoubleAnimation()
                        {
                            From = 1,
                            To = 0,
                            Duration = TimeSpan.FromSeconds(1),
                            EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseIn}
                        });
                        Before.RelatedGrid.BeginAnimation(MarginProperty, new ThicknessAnimation()
                        {
                            From = Before.RelatedGrid.Margin,
                            To = After.Index >= Before.Index ? new Thickness(
                                Before.RelatedGrid.Margin.Left + 100,
                                Before.RelatedGrid.Margin.Top,
                                Before.RelatedGrid.Margin.Right - 100,
                                Before.RelatedGrid.Margin.Bottom) : new Thickness(
                                Before.RelatedGrid.Margin.Left - 100,
                                Before.RelatedGrid.Margin.Top,
                                Before.RelatedGrid.Margin.Right + 100,
                                Before.RelatedGrid.Margin.Bottom),
                            Duration = TimeSpan.FromSeconds(1),
                            EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseIn},
                            FillBehavior = FillBehavior.Stop
                        });
                        Timer.Start();
                        Before.Unselect();
                    }
                    else
                    {
                        var After = NaviControl;
                        After.RelatedGrid.Visibility = Visibility.Visible;
                        After.RelatedGrid.BeginAnimation(OpacityProperty, new DoubleAnimation()
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromSeconds(1),
                            EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                        });
                        After.RelatedGrid.BeginAnimation(MarginProperty, new ThicknessAnimation()
                        {
                            From = new Thickness(
                                After.RelatedGrid.Margin.Left - 100,
                                After.RelatedGrid.Margin.Top,
                                After.RelatedGrid.Margin.Right + 100,
                                After.RelatedGrid.Margin.Bottom),
                            To = After.RelatedGrid.Margin,
                            Duration = TimeSpan.FromSeconds(1),
                            EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                        });
                    }
                    if (NavigationGrid.Children.OfType<NavigationControl>().All(O => O.IsViewed))
                        ContinueButton.IsEnabled = true;
                };
                NavigationGrid.Children.Add(NaviControl);
            }

            WinnersDisplayPanel.Width = 150*Battlefield.GetWinnerGroup().Participants.Length;
            Battlefield.GetWinnerGroup().Participants.ToList()
                .ForEach(O =>
                {
                    var TempControl = new ParticipantDisplayXLarge()
                    { Height = 150, Width = 150, Opacity = 0 };
                    TempControl.Init(O);
                    WinnersDisplayPanel.Children.Add(TempControl);
                });

            Battlefield.SetStatisticsRatio();
            PlayersListBox.ItemsSource = Battlefield.Room.Groups.SelectMany(O => O.Participants);
            PlayersListBox.SelectedIndex = 0;

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
                DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.75) };
                Timer.Tick += delegate
                {
                    ShowWinners();
                    Timer.Stop();
                };

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
                ((NavigationControl)NavigationGrid.Children[0]).Select();
                Timer.Start();
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

        private void ShowWinners()
        {
            var Winners = WinnersDisplayPanel.Children.OfType<ParticipantDisplayXLarge>().ToList();
            foreach (var Winner in Winners)
                Winner.Opacity = 0;
            DispatcherTimer Timer = new DispatcherTimer()
            { Interval = TimeSpan.FromSeconds(0.5) };
            int Count = 0;
            Timer.Tick += delegate
            {
                Winners[Count].BeginAnimation(OpacityProperty, new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(1),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });
                Count++;
                if (Count >= Winners.Count)
                    Timer.Stop();
            };
            Timer.Start();
        }

        private void SelectPlayer(object Sender, SelectionChangedEventArgs SelectionChangedEventArgs)
        {
            var Player = (Player)PlayersListBox.SelectedItem;
            if (Player == null) return;

            PlayerName.Text = Player.Name;
            GroupName.Text = Player.BattleData.Group.Name;
            GroupName.Foreground = new SolidColorBrush(Player.BattleData.Group.Color);
            PlayerDescription.Text = Player.Introduction;
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
            SetContributions(Player);

            AttackRatioText.Text = Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.Attack) == 0 ? "0/0 (0%)" :
                Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.Attack && O.Success) + "/" +
                Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.Attack) + " (" +
                (Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.Attack && O.Success) /
                (double)Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.Attack) * 100).ToString("0.0") + "%)";
            DefendRatioText.Text = Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.Defense) == 0 ? "0/0 (0%)" :
                Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.Defense && O.Success) + "/" +
                Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.Defense) + " (" +
                (Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.Defense && O.Success) /
                (double)Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.Defense) * 100).ToString("0.0") + "%)";
            CounterRatioText.Text = Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.Counter) == 0 ? "0/0 (0%)" :
                Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.Counter && O.Success) + "/" +
                Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.Counter) + " (" +
                (Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.Counter && O.Success) /
                (double)Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.Counter) * 100).ToString("0.0") + "%)";
            SkillRatioText.Text = Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.UseSkill) == 0 ? "0/0 (0%)" :
                Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.UseSkill && O.Success) + "/" +
                Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.UseSkill) + " (" +
                (Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.UseSkill && O.Success) /
                (double)Battlefield.Record.Participants[Player.Id].Rolls.Count(O => O.Type == DiceRoll.RollType.UseSkill) * 100).ToString("0.0") + "%)";

            double LuckRatio = Player.BattleData.Luck;
            LuckRatioText.Text = (LuckRatio*100).ToString("0.0") + "%";
            if (LuckRatio <= 0.25)
                LuckRatioText.Foreground = Brushes.Red;
            else if (LuckRatio <= 0.5)
                LuckRatioText.Foreground = Brushes.DarkOrange;
            else if (LuckRatio <= 0.75)
                LuckRatioText.Foreground = Brushes.YellowGreen;
            else
                LuckRatioText.Foreground = Brushes.LimeGreen;

            VictoryBackground.Color = Player.BattleData.Group.Color;
            VictoryText.Foreground = new SolidColorBrush(Player.BattleData.Group.Color);
            LuckBackground.Color = Player.BattleData.Group.Color;
            LuckText.Foreground = new SolidColorBrush(Player.BattleData.Group.Color);
            VictoryToPlayersBackground.Color = Player.BattleData.Group.Color;
            VictoryToPlayersText.Foreground = new SolidColorBrush(Player.BattleData.Group.Color);
            BattleLuckBackground.Color = Player.BattleData.Group.Color;
            BattleLuckText.Foreground = new SolidColorBrush(Player.BattleData.Group.Color);

            VictoryMarkerProgressBar.Init(Player.BattleData.BeforeVictoryRatio, Player.BattleData.AfterVictoryRatio);
            LuckMarkerProgressBar.Init(Player.BattleData.BeforeLuckRatio, Player.BattleData.AfterLuckRatio);
            VictoryMarkerProgressBar.Animate();
            LuckMarkerProgressBar.Animate();

            SetVictoryToPlayers(Player);
            LuckGraph.Init(Battlefield.Record.Participants[Player.Id].Rolls);
            LuckGraph.SetColor(Player.BattleData.Group.Color);
        }
        private void SetContributions(Player Player)
        {
            PlayerMarkPanel.Children.Clear();

            if (Player.BattleData.IsVictorious)
                PlayerMarkPanel.Children.Add(new WrapPanel()
                {
                    Height = 25,
                    Children =
                    {
                        new Image()
                        {
                            Height = 25,
                            Width = 25,
                            Source = Resources["Victory_flag"] as BitmapImage
                        },
                        new TextBlock()
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            FontSize = 15,
                            FontWeight = FontWeights.Light,
                            Text = "Victorious",
                            Padding = new Thickness(5, 0, 0, 0),
                            Foreground = new SolidColorBrush(Color.FromRgb(112, 173, 71))
                        }
                    },
                    ToolTip = "该玩家所在的小组获得了胜利。"
                });
            if (Player.BattleData.IsExecutioner)
                PlayerMarkPanel.Children.Add(new WrapPanel()
                {
                    Height = 25,
                    Children =
                    {
                        new Image()
                        {
                            Height = 25,
                            Width = 25,
                            Source = Resources["Executioner_flag"] as BitmapImage
                        },
                        new TextBlock()
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            FontSize = 15,
                            FontWeight = FontWeights.Light,
                            Text = "Executioner",
                            Padding = new Thickness(5, 0, 0, 0),
                            Foreground = new SolidColorBrush(Color.FromRgb(237, 125, 49))
                        }
                    },
                    ToolTip = "该玩家共击杀了" + Player.BattleData.Executed.Count + 
                              "个敌人，是全场最高。"
                });
            if (Player.BattleData.IsLuckyStar)
                PlayerMarkPanel.Children.Add(new WrapPanel()
                {
                    Height = 25,
                    Children =
                    {
                        new Image()
                        {
                            Height = 25,
                            Width = 25,
                            Source = Resources["Luck_flag"] as BitmapImage
                        },
                        new TextBlock()
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            FontSize = 15,
                            FontWeight = FontWeights.Light,
                            Text = "Lucky Star",
                            Padding = new Thickness(5, 0, 0, 0),
                            Foreground = new SolidColorBrush(Color.FromRgb(255, 192, 0))
                        }
                    },
                    ToolTip = "该玩家的本场运气达到了" + (Player.BattleData.Luck * 100).ToString("0.0") +
                              "%，是全场最高。"
                });
            if (Player.BattleData.IsContributor)
                PlayerMarkPanel.Children.Add(new WrapPanel()
                {
                    Height = 25,
                    Children =
                    {
                        new Image()
                        {
                            Height = 25,
                            Width = 25,
                            Source = Resources["Contributor_flag"] as BitmapImage
                        },
                        new TextBlock()
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            FontSize = 15,
                            FontWeight = FontWeights.Light,
                            Text = "Group Contributor",
                            Padding = new Thickness(5, 0, 0, 0),
                            Foreground = new SolidColorBrush(Color.FromRgb(91, 155, 213))
                        }
                    },
                    ToolTip = "该玩家是" + Player.BattleData.Group.Name + "的最大贡献者。"
                });
        }
        private void SetVictoryToPlayers(Player Player)
        {
            VictoryToPlayersPanel.Children.Clear();
            Battlefield.PlayerList.Where(O => O.BattleData.Group.Name != 
                Player.BattleData.Group.Name).ToList().ForEach(O =>
                {
                    var Data =
                        Player.Records.Where(
                            P =>
                                P.Id != Battlefield.Record.Id &&
                                P.Participants.ContainsKey(O.Id) &&
                                P.Participants[O.Id].Group != P.Participants[Player.Id].Group)
                        .ToList();
                    double BeforeValue = Data.Count == 0 ? 0 : Data.Count(P => P.Win)/(double) Data.Count;

                    Data =
                        Player.Records.Where(
                            P =>
                                P.Participants.ContainsKey(O.Id) &&
                                P.Participants[O.Id].Group != P.Participants[Player.Id].Group)
                        .ToList();
                    double AfterValue = Data.Count(P => P.Win) / (double)Data.Count;

                    double Delta = AfterValue - BeforeValue;

                    BitmapImage TempImageSource = null;
                    using (MemoryStream Stream = new MemoryStream())
                    {
                        O.Avator.Save(Stream, ImageFormat.Png);
                        BitmapImage Temp = new BitmapImage();
                        Temp.BeginInit();
                        Temp.CacheOption = BitmapCacheOption.OnLoad;
                        Temp.StreamSource = Stream;
                        Temp.EndInit();
                        TempImageSource = Temp;
                    }

                    VictoryToPlayersPanel.Children.Add(new Grid()
                    {
                        Height = 30,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Children =
                        {
                            new Ellipse()
                            {
                                Height = 30,
                                Width = 30,
                                Margin = new Thickness(0, 0, 0, 0),
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Center,
                                Fill = new ImageBrush(TempImageSource)
                            },
                            new TextBlock()
                            {
                                Margin = new Thickness(35, 0, 0, 0),
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Center,
                                FontSize = 15,
                                FontWeight = FontWeights.Light,
                                Text = O.Name
                            },
                            new TextBlock()
                            {
                                Margin = new Thickness(150, 0, 0, 0),
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Center,
                                Text = (AfterValue*100).ToString("0.0") + "%",
                                Foreground = AfterValue <= 0.25
                                    ? Brushes.Red
                                    : AfterValue <= 0.5
                                        ? Brushes.DarkOrange
                                        : AfterValue <= 0.75 ? Brushes.YellowGreen : Brushes.LimeGreen
                            },
                            new TextBlock()
                            {
                                Margin = new Thickness(250, 0, 0, 0),
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Center,
                                Text = (Delta > 0 ? "+" : "") + (Delta*100).ToString("0.0") + "%",
                                Foreground = Delta > 0 ? Brushes.LimeGreen : Delta < 0 ? Brushes.Red : Brushes.Gray
                            }
                        }
                    });
                });
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            LeaveAction = delegate
            {
                Battlefield.PlayerList.ForEach(O =>
                {
                    if (O is User) IO.SaveUser((User) O);
                    else if (O is AI)
                    {
                        IO.SaveAi((AI) O);
                        ((AI) O).PlayerDataCollection = null;
                    }
                    O.BattleData = null;
                });
                MainWindow.Instance.EnterUserPage(true);
            };
            PageLeave();
        }
    }
}
