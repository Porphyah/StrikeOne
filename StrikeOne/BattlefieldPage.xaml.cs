using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using StrikeOne.Components;
using StrikeOne.Core;

namespace StrikeOne
{
    /// <summary>
    /// BattlefieldPage.xaml 的交互逻辑
    /// </summary>
    public partial class BattlefieldPage : UserControl
    {
        public BattlefieldPage()
        {
            InitializeComponent();
        }
        public Battlefield Battlefield { private set; get; }

        public void PageEnter(Battlefield Source)
        {
            Battlefield = Source;

            DiceControl.Height = 0;
            DiceControl.Width = 0;
            DiceControl.ContentGrid.Visibility = Visibility.Hidden;
            DiceControl.ContentGrid.Opacity = 0;

            StatusText.Text = "Initalizing...";
            for (int i = 0; i < Battlefield.Room.Groups.Count; i++)
            {
                var Panel = new StackPanel()
                { Width = 100, Height = Battlefield.Room.Groups[i].Capacity * 100 };
                foreach (var Participant in Battlefield.Room.Groups[i].Participants)
                {
                    var PlayerCard = new PlayerCard()
                    { Height = 100, Width = 300 };
                    PlayerCard.Init(Participant);
                    PlayerCard.Collapse();
                    PlayerCard.Hp.Value = 0;
                    Panel.Children.Add(PlayerCard);
                }
                Panel.Opacity = 0;
                CardCanvas.Children.Add(Panel);
                SetItemPosition(Panel, Battlefield.Room.BattleType, i);
            }
            foreach (var Player in Battlefield.PlayerList)
            {
                var PlayerItem = new PlayerItem();
                PlayerItem.Init(Player);
                PlayerItemStack.Children.Add(PlayerItem);
                Player.BattleData.SetupUiConnection();
            }

            TitleGrid.BeginAnimation(MarginProperty, new ThicknessAnimation()
            {
                From = TitleGrid.Margin,
                To = new Thickness(0, 0, 0, 0),
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            });

            DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.5) };
            switch (Battlefield.Room.BattleType)
            {
                case BattleType.OneVsOne:
                    var GroupPanels = CardCanvas.Children.OfType<StackPanel>().ToArray();
                    int Count = 0;
                    Timer.Tick += delegate
                    {
                        switch (Count)
                        {
                            case 0:
                                ShowGroupPanel(GroupPanels[0]);
                                break;
                            case 1:
                                ExpandGroupPanel(GroupPanels[0]);
                                break;
                            case 2:
                                ArcAnimation(45, 225);
                                ShowGroupPanel(GroupPanels[1]);
                                break;
                            case 3:
                                ExpandGroupPanel(GroupPanels[1]);
                                break;
                            case 4:
                                ArcAnimation(225, 45);
                                break;
                            case 5:
                                SetStatus("Ready", Colors.LimeGreen);
                                break;
                            case 6:
                                NextRound();
                                Timer.Stop();
                                break;
                        }
                        Count++;
                    };
                    Timer.Start();
                    break;
            }
        }

        private void NextRound()
        {
            Battlefield.NextRound();
            RoundText.Text = "Round " + Battlefield.Round;

            CenterBorderCommand(new List<KeyValuePair<string, Action>>()
            {
                new KeyValuePair<string, Action>("ShowCenterBorder", null),
                new KeyValuePair<string, Action>("ShowCenterText;Round " + Battlefield.Round, null),
                new KeyValuePair<string, Action>("Wait;1", null),
                new KeyValuePair<string, Action>("HideCenterText", null),
                new KeyValuePair<string, Action>("HideCenterBorder", SetCurrentPlayer)
            });
        }

        private void NextPlayer()
        {
            Battlefield.CurrentPlayer.BattleData.SetStatus("Joined");
            if (!Battlefield.NextPlayer())
                NextRound();
            else
                SetCurrentPlayer();
        }

        private void SetCurrentPlayer()
        {
            var Player = Battlefield.CurrentPlayer;
            Player.BattleData.SetStatus("Ready");
            SetStatus("角色：" + Player.Name + "的回合", Player.Id == App.CurrentUser.Id
                ? Colors.LimeGreen : Colors.DodgerBlue);
            if (Player is AI)
                SetAiAttackOptions((AI)Player);
            else if (Player.Id == App.CurrentUser.Id)
                OpenAttackOptions();
            //else
            //{ }
        }

        
    
        private void OpenAttackOptions(bool Back = false)
        {
            CenterGrid.Children.Clear();

            var AttackButton = new ActionButton() { Height = 100, Width = 85 };
            AttackButton.Init("攻击", "Attack", "选择一个目标进行常规攻击，攻击成功将能造成" +
                App.CurrentUser.BattleData.AttackDamage + "点伤害。",
                App.CurrentUser.BattleData.AttackSuccessRatio);
            AttackButton.MouseUp += Attack_Click;
            CenterGrid.Children.Add(AttackButton);

            var AbondonButton = new ActionButton() { Height = 100, Width = 85 };
            AbondonButton.Init("放弃", "Abondon", "该回合不做任何行动，并使下一回合攻击成功的概率增加1/6。");
            AbondonButton.MouseUp += AttackAbondon_Click;
            CenterGrid.Children.Add(AbondonButton);

            if (App.CurrentUser.BattleData.Skill != null &&
                App.CurrentUser.BattleData.Skill.Occasion == SkillOccasion.BeforeAttacking)
            {
                var SkillButton = new ActionButton() {Height = 100, Width = 85};
                if (App.CurrentUser.BattleData.Skill.Enable)
                    SkillButton.Init("使用技能", "Skill_white", 
                        "使用您的技能：" + App.CurrentUser.BattleData.Skill.Name + "。",
                        App.CurrentUser.BattleData.Skill.Probability);
                else
                {
                    if (!App.CurrentUser.BattleData.Skill.IsRemainingCount())
                    {
                        SkillButton.ButtonName.Foreground = Brushes.DarkGray;
                        SkillButton.Init("次数已用尽", "Skill_grey",
                            "当前由于技能次数用尽而无法使用技能：" + App.CurrentUser.BattleData.Skill.Name + "。");
                        SkillButton.IsEnabled = false;
                    }
                    else if (App.CurrentUser.BattleData.Skill.IsAffecting())
                    {
                        SkillButton.ButtonName.Foreground = Brushes.DarkGray;
                        SkillButton.Init("技能起效中", "Skill_grey",
                            "当前由于技能正在生效中而无法使用技能：" + App.CurrentUser.BattleData.Skill.Name + "。");
                        SkillButton.IsEnabled = false;
                    }
                    else if (App.CurrentUser.BattleData.Skill.IsCoolingDown())
                    {
                        SkillButton.ButtonName.Foreground = Brushes.DarkGray;
                        SkillButton.Init("技能冷却中", "Skill_grey",
                            "当前由于技能正在冷却中而无法使用技能：" + App.CurrentUser.BattleData.Skill.Name + "。");
                        SkillButton.IsEnabled = false;
                    }
                }
                CenterGrid.Children.Add(SkillButton);
                SetActionButtonPosition(AttackButton, 3, 0);
                SetActionButtonPosition(SkillButton, 3, 1);
                SetActionButtonPosition(AbondonButton, 3, 2);
            }
            else
            {
                SetActionButtonPosition(AttackButton, 2, 0);
                SetActionButtonPosition(AbondonButton, 2, 1);
            }

            if (Back)
                CenterBorderCommand(new List<KeyValuePair<string, Action>>()
                {
                    new KeyValuePair<string, Action>("ShowCenterBorder", null),
                    new KeyValuePair<string, Action>("ShowCenterGrid", null)
                });
            else
                CenterBorderCommand(new List<KeyValuePair<string, Action>>()
                {
                    new KeyValuePair<string, Action>("ShowCenterBorder", null),
                    new KeyValuePair<string, Action>("ShowCenterText;Your Turn", null),
                    new KeyValuePair<string, Action>("Wait;0.5", null),
                    new KeyValuePair<string, Action>("HideCenterText", null),
                    new KeyValuePair<string, Action>("ShowCenterGrid", null)
                });
        }
        private void OpenDefendOptions(bool Back = false)
        {
            CenterGrid.Children.Clear();

            var DefendButton = new ActionButton() { Height = 100, Width = 85 };
            DefendButton.Init("防御", "Defend", "使用防御来削减来自对方的伤害，当前可降低" +
                App.CurrentUser.BattleData.DefenceCapacity + "点伤害。",
                App.CurrentUser.BattleData.DefendSuccessRatio);
            DefendButton.MouseUp += Defend_Click;
            CenterGrid.Children.Add(DefendButton);

            var CounterButton = new ActionButton() { Height = 100, Width = 85 };
            CounterButton.Init("反击", "Counter", "是时候反击了！反击可使本次攻击无效并给予对方" +
                App.CurrentUser.BattleData.CounterDamage + "点伤害，但一旦失败则会给自己造成额外伤害。",
                App.CurrentUser.BattleData.CounterSuccessRatio);
            CounterButton.MouseUp += Counter_Click;
            CenterGrid.Children.Add(CounterButton);

            var AbondonButton = new ActionButton() { Height = 100, Width = 85 };
            AbondonButton.Init("放弃", "Abondon", "默默忍受本次攻击，并使下一回合防御成功的概率增加1/3。");
            AbondonButton.MouseUp += DefendAbondon_Click;
            CenterGrid.Children.Add(AbondonButton);

            if (App.CurrentUser.BattleData.Skill != null &&
               App.CurrentUser.BattleData.Skill.Occasion == SkillOccasion.UnderAttack)
            {
                var SkillButton = new ActionButton() { Height = 100, Width = 85 };
                if (App.CurrentUser.BattleData.Skill.Enable)
                    SkillButton.Init("使用技能", "Skill_white",
                        "使用您的技能：" + App.CurrentUser.BattleData.Skill.Name + "。",
                        App.CurrentUser.BattleData.Skill.Probability);
                else
                {
                    if (!App.CurrentUser.BattleData.Skill.IsRemainingCount())
                    {
                        SkillButton.ButtonName.Foreground = Brushes.DarkGray;
                        SkillButton.Init("次数已用尽", "Skill_grey",
                            "当前由于技能次数用尽而无法使用技能：" + App.CurrentUser.BattleData.Skill.Name + "。");
                        SkillButton.IsEnabled = false;
                    }
                    else if (App.CurrentUser.BattleData.Skill.IsAffecting())
                    {
                        SkillButton.ButtonName.Foreground = Brushes.DarkGray;
                        SkillButton.Init("技能起效中", "Skill_grey",
                            "当前由于技能正在生效中而无法使用技能：" + App.CurrentUser.BattleData.Skill.Name + "。");
                        SkillButton.IsEnabled = false;
                    }
                    else if (App.CurrentUser.BattleData.Skill.IsCoolingDown())
                    {
                        SkillButton.ButtonName.Foreground = Brushes.DarkGray;
                        SkillButton.Init("技能冷却中", "Skill_grey",
                            "当前由于技能正在冷却中而无法使用技能：" + App.CurrentUser.BattleData.Skill.Name + "。");
                        SkillButton.IsEnabled = false;
                    }
                }
                CenterGrid.Children.Add(SkillButton);
                SetActionButtonPosition(DefendButton, 4, 0);
                SetActionButtonPosition(CounterButton, 4, 1);
                SetActionButtonPosition(SkillButton, 4, 2);
                SetActionButtonPosition(AbondonButton, 4, 3);
            }
            else
            {
                SetActionButtonPosition(DefendButton, 3, 0);
                SetActionButtonPosition(CounterButton, 3, 1);
                SetActionButtonPosition(AbondonButton, 3, 2);
            }

            if (Back)
                CenterBorderCommand(new List<KeyValuePair<string, Action>>()
                {
                    new KeyValuePair<string, Action>("ShowCenterBorder", null),
                    new KeyValuePair<string, Action>("ShowCenterGrid", null)
                });
            else
                CenterBorderCommand(new List<KeyValuePair<string, Action>>()
                {
                    new KeyValuePair<string, Action>("ShowCenterBorder", null),
                    new KeyValuePair<string, Action>("ShowCenterText;Under Attack!", null),
                    new KeyValuePair<string, Action>("Wait;0.5", null),
                    new KeyValuePair<string, Action>("HideCenterText", null),
                    new KeyValuePair<string, Action>("ShowCenterGrid", null)
                });
        }

        private void SetAiAttackOptions(AI Player)
        {
            string Choice = Player.ChooseAttackChoice();
            switch (Choice)
            {
                case "Attack":
                    Player Target = Player.GetAttackTarget();

                    var SelfCard = Player.BattleData.PlayerCard;
                    var PlayerCard = Target.BattleData.PlayerCard;
                    ActionLineAnimation(
                        new Point(Canvas.GetLeft((StackPanel)SelfCard.Parent) + SelfCard.ActualWidth / 2,
                            Canvas.GetTop((StackPanel)SelfCard.Parent) + ((StackPanel)SelfCard.Parent).Children.IndexOf(SelfCard) * 100 + SelfCard.ActualHeight / 2),
                        new Point(Canvas.GetLeft((StackPanel)PlayerCard.Parent) + PlayerCard.ActualWidth / 2,
                            Canvas.GetTop((StackPanel)PlayerCard.Parent) + ((StackPanel)PlayerCard.Parent).Children.IndexOf(PlayerCard) * 100 + PlayerCard.ActualHeight / 2),
                        Colors.Red, delegate (Line ActionLine)
                        {
                            DiceControl.Expected = 6 - Player.BattleData.AttackSuccessRatio;
                            DiceControl.EndAction = delegate
                            { AttackAction(Player, Target, ActionLine, DiceControl.Success); };
                            DiceControl.Expand();
                        });
                    PlayerCard.Player.BattleData.SetStatus("Breakdown");
                    SelfCard.SetAction("攻击", "Attack", "选择一个目标进行常规攻击，攻击成功将能造成" +
                        Player.BattleData.AttackDamage + "点伤害。", Player.BattleData.AttackSuccessRatio);
                    SetStatus(Target.Name + "正遭受攻击！", Colors.OrangeRed);
                    break;
                case "Abondon":
                    SetStatus(Player.Name + "放弃了本次攻击机会。", Colors.Gray);
                    Player.BattleData.AttackSuccessRatio += 1;
                    if (Player.BattleData.AttackSuccessRatio > 6)
                        Player.BattleData.AttackSuccessRatio = 6;
                    Player.BattleData.PlayerCard.SetAction("放弃", "Abondon",
                        Player.Name + "放弃了本次攻击机会，下回合" + Player.Name +
                        "的攻击成功率变为：" + Player.BattleData.AttackSuccessRatio + "/6。", null);

                    DispatcherTimer AbondonTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                    AbondonTimer.Tick += delegate
                    { NextPlayer(); AbondonTimer.Stop(); };
                    AbondonTimer.Start();
                    break;
            }
        }
        private void SetAiDefendOptions(AI Player)
        {
            string Choice = Player.ChooseDefendChoice();
            var SelfCard = Player.BattleData.PlayerCard;
            switch (Choice)
            {
                case "Defend":
                    SelfCard.SetAction("防御", "Defend", "使用防御来削减来自对方的伤害，当前可降低" +
                        Player.BattleData.DefenceCapacity + "点伤害。", Player.BattleData.DefendSuccessRatio);
                    SetStatus(Player.Name + "选择防御！", Colors.OrangeRed);

                    DiceControl.Expected = 6 - App.CurrentUser.BattleData.DefendSuccessRatio;
                    DiceControl.EndAction = delegate
                    { DefendAction(Battlefield.CurrentPlayer, Player, DiceControl.Success); };
                    DiceControl.Expand();
                    break;
                case "Counter":
                    var PlayerCard = Battlefield.CurrentPlayer.BattleData.PlayerCard;
                    ActionLineAnimation(
                        new Point(Canvas.GetLeft((StackPanel)SelfCard.Parent) + SelfCard.ActualWidth / 2,
                            Canvas.GetTop((StackPanel)SelfCard.Parent) + ((StackPanel)SelfCard.Parent).Children.IndexOf(SelfCard) * 100 + SelfCard.ActualHeight / 2),
                        new Point(Canvas.GetLeft((StackPanel)PlayerCard.Parent) + PlayerCard.ActualWidth / 2,
                            Canvas.GetTop((StackPanel)PlayerCard.Parent) + ((StackPanel)PlayerCard.Parent).Children.IndexOf(PlayerCard) * 100 + PlayerCard.ActualHeight / 2),
                        Colors.Red, delegate (Line ActionLine)
                        {
                            DiceControl.Expected = 6 - App.CurrentUser.BattleData.CounterSuccessRatio;
                            DiceControl.EndAction = delegate
                            { CounterAction(Player, Battlefield.CurrentPlayer, ActionLine, DiceControl.Success); };
                            DiceControl.Expand();
                        });

                    Player.BattleData.PlayerCard.SetAction("反击", "Counter", "该角色正在进行反击，反击成功将能造成" +
                        Player.BattleData.CounterDamage + "点伤害，若失败自己将遭受" +
                        Battlefield.CurrentPlayer.BattleData.CounterPunishment + "点伤害。",
                        Player.BattleData.CounterSuccessRatio);
                    SetStatus(Player.Name + "选择反击！", Colors.OrangeRed);
                    Battlefield.CurrentPlayer.BattleData.SetStatus("Breakdown");
                    break;
                case "Abondon":
                    SetStatus(Player.Name + "放弃了本次防御机会。", Colors.Gray);
                    Player.BattleData.DefendSuccessRatio += 2;
                    if (Player.BattleData.DefendSuccessRatio > 6)
                        Player.BattleData.DefendSuccessRatio = 6;
                    Player.BattleData.PlayerCard.SetAction("放弃", "Abondon",
                        Player.Name + "放弃了本次防御机会，下回合" + Player.Name +
                        "的防御成功率变为：" + Player.BattleData.DefendSuccessRatio + "/6。", null);

                    DispatcherTimer AbondonTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                    AbondonTimer.Tick += delegate
                    { NextPlayer(); AbondonTimer.Stop(); };
                    AbondonTimer.Start();
                    break;
            }
        }

        private void SetItemPosition(FrameworkElement Item, BattleType BattleType, int GroupIndex)
        {
            switch (BattleType)
            {
                case BattleType.OneVsOne:
                    switch (GroupIndex)
                    {
                        case 0: SetItemPosition(Item, 45); break;
                        case 1: SetItemPosition(Item, 225); break;
                    }
                    break;
            }
        }
        private void SetItemPosition(FrameworkElement Item, double Angle)
        {
            Item.HorizontalAlignment = HorizontalAlignment.Center;
            Item.VerticalAlignment = VerticalAlignment.Center;
            Canvas.SetLeft(Item, -250 * Math.Sin(Angle / 180 * Math.PI) 
                + CardCanvas.ActualWidth / 2 - Item.Width / 2);
            Canvas.SetTop(Item, -250 * Math.Cos(Angle / 180 * Math.PI)
                + CardCanvas.ActualHeight / 2 - Item.Height / 2);
        }

        private void ShowGroupPanel(StackPanel GroupPanel)
        {
            GroupPanel.BeginAnimation(OpacityProperty, new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
            });
            foreach (var PlayerCard in GroupPanel.Children.OfType<PlayerCard>())
                PlayerCard.Hp.BeginAnimation(RangeBase.ValueProperty, new DoubleAnimation()
                {
                    From = 0,
                    To = PlayerCard.Player.BattleData.CurrentHp,
                    Duration = TimeSpan.FromSeconds(1),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                });
        }
        private void ArcAnimation(double FromAngle, double ToAngle)
        {
            Point StartPoint = new Point(
                -250*Math.Sin(FromAngle/180*Math.PI) + PathCanvas.ActualWidth/2,
                -250*Math.Cos(FromAngle/180*Math.PI) + PathCanvas.ActualHeight/2);
            Point EndPoint = new Point(
                -250 * Math.Sin(ToAngle / 180 * Math.PI) + PathCanvas.ActualWidth / 2,
                -250 * Math.Cos(ToAngle / 180 * Math.PI) + PathCanvas.ActualHeight / 2);
            var ArcSegment = new ArcSegment()
            {
                IsLargeArc = ToAngle - FromAngle > 180,
                Size = new Size(250, 250),
                Point = StartPoint
            };
            var PathTrace = new PathGeometry()
            {
                Figures =
                {
                    new PathFigure()
                    {
                        StartPoint = StartPoint,
                        IsClosed = false,
                        Segments =
                        {
                            new ArcSegment()
                            {
                                IsLargeArc = ToAngle - FromAngle > 180,
                                Size = new Size(250, 250),
                                Point = EndPoint
                            }
                        }
                    }
                }
            };
            var Arc = new System.Windows.Shapes.Path()
            {
                Height = PathCanvas.ActualHeight,
                Width = PathCanvas.ActualWidth,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 220, 220)),
                StrokeThickness = 3.5,
                Effect = new DropShadowEffect()
                {
                    BlurRadius = 3,
                    Color = Colors.Gray,
                    ShadowDepth = 0
                },
                Data = new PathGeometry()
                {
                    Figures = new PathFigureCollection()
                    {
                        new PathFigure()
                        {
                            StartPoint = StartPoint,
                            IsClosed = false,
                            Segments = { ArcSegment }
                        }
                    }
                }
            };
            PathCanvas.Children.Add(Arc);

            PointAnimationUsingPath ArcAnimation = new PointAnimationUsingPath()
            {
                PathGeometry = PathTrace,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            //ArcAnimation.Completed += delegate { Completed?.Invoke(); };
            ArcSegment.BeginAnimation(ArcSegment.PointProperty, ArcAnimation);
        }
        private void ExpandGroupPanel(StackPanel GroupPanel)
        {
            GroupPanel.BeginAnimation(Canvas.LeftProperty, new DoubleAnimation()
            {
                From = Canvas.GetLeft(GroupPanel),
                To = Canvas.GetLeft(GroupPanel) - 100,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
            });
            GroupPanel.BeginAnimation(WidthProperty, new DoubleAnimation()
            {
                From = 100,
                To = 300,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
            });
            foreach (var PlayerCard in GroupPanel.Children.OfType<PlayerCard>())
                PlayerCard.Expand();
        }
        private void ActionLineAnimation(Point From, Point To, Color Color, Action<Line> Action)
        {
            Line ActionLine = new Line()
            {
                Height = PathCanvas.ActualHeight,
                Width = PathCanvas.ActualWidth,
                X1 = From.X, Y1 = From.Y,
                X2 = From.X, Y2 = From.Y,
                Stroke = new SolidColorBrush(Color),
                StrokeEndLineCap = PenLineCap.Triangle,
                StrokeThickness = 0,
                Effect = new DropShadowEffect()
                {
                    ShadowDepth = 2,
                    Color = Colors.Gray
                }
            };
            Canvas.SetLeft(ActionLine, 0);
            Canvas.SetTop(ActionLine, 0);
            PathCanvas.Children.Add(ActionLine);
            DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1.5) };
            Timer.Tick += delegate
            {
                Action?.Invoke(ActionLine);
                Timer.Stop();
            };

            ActionLine.BeginAnimation(Line.X2Property, new DoubleAnimation()
            {
                From = ActionLine.X1,
                To = To.X,
                Duration = TimeSpan.FromSeconds(1.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
            });
            ActionLine.BeginAnimation(Line.Y2Property, new DoubleAnimation()
            {
                From = ActionLine.Y1,
                To = To.Y,
                Duration = TimeSpan.FromSeconds(1.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
            });
            ActionLine.BeginAnimation(Line.StrokeThicknessProperty, new DoubleAnimation()
            {
                From = 0,
                To = 5,
                Duration = TimeSpan.FromSeconds(1.5),
                //EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            });
            Timer.Start();
        }

        private void SetStatus(string Text, Color Color)
        {
            StatusText.Text = Text;
            StatusBorder.Background = StatusBorder.Background.Clone();
            ((SolidColorBrush)StatusBorder.Background).BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation()
                {
                    From = ((SolidColorBrush)StatusBorder.Background).Color,
                    To = Color,
                    Duration = TimeSpan.FromSeconds(0.25),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                });
        }

        private void CenterBorderCommand(List<KeyValuePair<string, Action>> Commands)
        {
            string[] Fragments = Commands[0].Key.Split(';');
            switch (Fragments[0])
            {
                case "ShowCenterBorder":
                    DoubleAnimation CenterShowAnimation = new DoubleAnimation()
                    {
                        From = 0,
                        To = 120,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                    };
                    CenterShowAnimation.Completed += delegate
                    {
                        Commands[0].Value?.Invoke();
                        if (Commands.Count > 1)
                            CenterBorderCommand(Commands.Skip(1).ToList());
                    };
                    CenterBorder.BeginAnimation(HeightProperty, CenterShowAnimation);
                    break;
                case "ShowCenterText":
                    CenterText.Text = Fragments[1];
                    CenterText.Visibility = Visibility.Visible;
                    DoubleAnimation TextShowAnimation = new DoubleAnimation()
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                    };
                    TextShowAnimation.Completed += delegate
                    {
                        Commands[0].Value?.Invoke();
                        if (Commands.Count > 1)
                            CenterBorderCommand(Commands.Skip(1).ToList());
                    };
                    CenterText.BeginAnimation(OpacityProperty, TextShowAnimation);
                    break;
                case "ShowCenterGrid":
                    CenterGrid.Visibility = Visibility.Visible;
                    DispatcherTimer ShowGridTimer = new DispatcherTimer()
                    { Interval = TimeSpan.FromSeconds(0.5) };
                    ShowGridTimer.Tick += delegate
                    {
                        Commands[0].Value?.Invoke();
                        if (Commands.Count > 1)
                            CenterBorderCommand(Commands.Skip(1).ToList());
                        ShowGridTimer.Stop();
                    };
                    CenterGrid.BeginAnimation(OpacityProperty, new DoubleAnimation()
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    });
                    CenterGrid.BeginAnimation(MarginProperty, new ThicknessAnimation()
                    {
                        From = new Thickness(50, 0, -50, 0),
                        To = new Thickness(0, 0, 0, 0),
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    });
                    ShowGridTimer.Start();
                    break;
                case "Wait":
                    DispatcherTimer WaitTimer = new DispatcherTimer()
                    { Interval = TimeSpan.FromSeconds(double.Parse(Fragments[1])) };
                    WaitTimer.Tick += delegate
                    {
                        Commands[0].Value?.Invoke();
                        if (Commands.Count > 1)
                            CenterBorderCommand(Commands.Skip(1).ToList());
                        WaitTimer.Stop();
                    };
                    WaitTimer.Start();
                    break;
                case "HideCenterText":
                    DoubleAnimation TextHideAnimation = new DoubleAnimation()
                    {
                        From = 1,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                    };
                    TextHideAnimation.Completed += delegate
                    {
                        CenterText.Visibility = Visibility.Hidden;
                        Commands[0].Value?.Invoke();
                        if (Commands.Count > 1)
                            CenterBorderCommand(Commands.Skip(1).ToList());
                    };
                    CenterText.BeginAnimation(OpacityProperty, TextHideAnimation);
                    break;
                case "HideCenterGrid":
                    DispatcherTimer HideGridTimer = new DispatcherTimer()
                    { Interval = TimeSpan.FromSeconds(0.5) };
                    HideGridTimer.Tick += delegate
                    {
                        CenterGrid.Visibility = Visibility.Hidden;
                        Commands[0].Value?.Invoke();
                        if (Commands.Count > 1)
                            CenterBorderCommand(Commands.Skip(1).ToList());
                        HideGridTimer.Stop();
                    };
                    CenterGrid.BeginAnimation(OpacityProperty, new DoubleAnimation()
                    {
                        From = CenterGrid.Opacity,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                    });
                    CenterGrid.BeginAnimation(MarginProperty, new ThicknessAnimation()
                    {
                        From = CenterGrid.Margin,
                        To = new Thickness(-50, 0, 50, 0),
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                    });
                    HideGridTimer.Start();
                    break;
                case "HideCenterBorder":
                    DoubleAnimation CenterHideAnimation = new DoubleAnimation()
                    {
                        From = 120,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                    };
                    CenterHideAnimation.Completed += delegate
                    {
                        Commands[0].Value?.Invoke();
                        if (Commands.Count > 1)
                            CenterBorderCommand(Commands.Skip(1).ToList());
                    };
                    CenterBorder.BeginAnimation(HeightProperty, CenterHideAnimation);
                    break;
            }
        }
        private void SetActionButtonPosition(ActionButton Button, int Count, int Index)
        {
            switch (Count)
            {
                case 1:
                    Button.Margin = new Thickness(0, 0, 0, 0);
                    break;
                case 2:
                    switch (Index)
                    {
                        case 0:
                            Button.Margin = new Thickness(-100, 0, 100, 0);
                            break;
                        case 1:
                            Button.Margin = new Thickness(100, 0, -100, 0);
                            break;
                    }
                    break;
                case 3:
                    switch (Index)
                    {
                        case 0:
                            Button.Margin = new Thickness(-150, 0, 150, 0);
                            break;
                        case 1:
                            Button.Margin = new Thickness(0, 0, 0, 0);
                            break;
                        case 2:
                            Button.Margin = new Thickness(150, 0, -150, 0);
                            break;
                    }
                    break;
                case 4:
                    switch (Index)
                    {
                        case 0:
                            Button.Margin = new Thickness(-50, 0, 50, 0);
                            break;
                        case 1:
                            Button.Margin = new Thickness(-150, 0, 150, 0);
                            break;
                        case 2:
                            Button.Margin = new Thickness(50, 0, -50, 0);
                            break;
                        case 3:
                            Button.Margin = new Thickness(150, 0, -150, 0);
                            break;
                    }
                    break;
            }
        }

        private void Attack_Click(object sender, MouseButtonEventArgs e)
        {
            CenterBorderCommand(new List<KeyValuePair<string, Action>>()
            {
                new KeyValuePair<string, Action>("HideCenterGrid", delegate
                {
                    SetStatus("选择一个攻击目标", Colors.Orange);
                    foreach (var PlayerCard in Battlefield.PlayerList.Select(O => O.BattleData.PlayerCard))
                    {
                        PlayerCard.MouseEnter += AttackTarget_MouseEnter;
                        PlayerCard.MouseLeave += AttackTarget_MouseLeave;
                        PlayerCard.MouseLeftButtonUp += AttackTarget_MouseClick;
                    }
                    this.MouseRightButtonUp += AttackCancel_Click;
                }),
                new KeyValuePair<string, Action>("HideCenterBorder", null)
            });
        }
        private void AttackTarget_MouseEnter(object sender, MouseEventArgs e)
        {
            PlayerCard PlayerCard = (PlayerCard) sender;
            if (PlayerCard.Player.BattleData.Group.Name ==
                App.CurrentUser.BattleData.Group.Name ||
                !(PlayerCard.Player.BattleData.CurrentHp > 0))
            {
                PlayerCard.ToolTip = new WrapPanel()
                {
                    Children =
                    {
                        new Image()
                        {
                            Height = 25,
                            Width = 25,
                            Source = Resources["Error"] as BitmapImage
                        },
                        new TextBlock()
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            Padding = new Thickness(5, 0, 0, 0),
                            Text = "该角色不能被作为攻击目标。"
                        }
                    }
                };

                PlayerCard.InfoBorder.Background = PlayerCard.InfoBorder.Background.Clone();
                ((SolidColorBrush) PlayerCard.InfoBorder.Background).BeginAnimation(
                    SolidColorBrush.ColorProperty, new ColorAnimation()
                    {
                        From = ((SolidColorBrush) PlayerCard.InfoBorder.Background).Color,
                        To = Colors.LightGray,
                        Duration = TimeSpan.FromSeconds(0.3),
                        EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut}
                    });
            }
            else
            {
                PlayerCard.ToolTip = new WrapPanel()
                {
                    Children =
                    {
                        new Image()
                        {
                            Height = 25,
                            Width = 25,
                            Source = Resources["Ok"] as BitmapImage
                        },
                        new TextBlock()
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            Padding = new Thickness(5, 0, 0, 0),
                            Text = "该角色可被选为攻击目标。"
                        }
                    }
                };

                PlayerCard.InfoBorder.Background = PlayerCard.InfoBorder.Background.Clone();
                ((SolidColorBrush)PlayerCard.InfoBorder.Background).BeginAnimation(
                    SolidColorBrush.ColorProperty, new ColorAnimation()
                    {
                        From = ((SolidColorBrush)PlayerCard.InfoBorder.Background).Color,
                        To = Colors.Pink,
                        Duration = TimeSpan.FromSeconds(0.3),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    });
            }
        }
        private void AttackTarget_MouseLeave(object sender, MouseEventArgs e)
        {
            PlayerCard PlayerCard = (PlayerCard)sender;
            PlayerCard.ToolTip = null;
            PlayerCard.InfoBorder.Background = PlayerCard.InfoBorder.Background.Clone();
            ((SolidColorBrush)PlayerCard.InfoBorder.Background).BeginAnimation(
                SolidColorBrush.ColorProperty, new ColorAnimation()
                {
                    From = ((SolidColorBrush)PlayerCard.InfoBorder.Background).Color,
                    To = Colors.White,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                });
        }
        private void AttackTarget_MouseClick(object sender, MouseButtonEventArgs e)
        {
            PlayerCard PlayerCard = (PlayerCard)sender;
            if (PlayerCard.Player.BattleData.Group.Name ==
                App.CurrentUser.BattleData.Group.Name ||
                !(PlayerCard.Player.BattleData.CurrentHp > 0))
                return;
            PlayerCard.ToolTip = null;
            PlayerCard.InfoBorder.Background = PlayerCard.InfoBorder.Background.Clone();
            ((SolidColorBrush)PlayerCard.InfoBorder.Background).BeginAnimation(
                SolidColorBrush.ColorProperty, new ColorAnimation()
                {
                    From = ((SolidColorBrush)PlayerCard.InfoBorder.Background).Color,
                    To = Colors.White,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                });
            foreach (var EachPlayerCard in Battlefield.PlayerList.Select(O => O.BattleData.PlayerCard))
            {
                EachPlayerCard.MouseEnter -= AttackTarget_MouseEnter;
                EachPlayerCard.MouseLeave -= AttackTarget_MouseLeave;
                EachPlayerCard.MouseLeftButtonUp -= AttackTarget_MouseClick;
            }
            this.MouseRightButtonUp -= AttackCancel_Click;

            var SelfCard = App.CurrentUser.BattleData.PlayerCard;
            ActionLineAnimation(
                new Point(Canvas.GetLeft((StackPanel)SelfCard.Parent) + SelfCard.ActualWidth / 2, 
                    Canvas.GetTop((StackPanel)SelfCard.Parent) + ((StackPanel)SelfCard.Parent).Children.IndexOf(SelfCard) * 100 + SelfCard.ActualHeight / 2),
                new Point(Canvas.GetLeft((StackPanel)PlayerCard.Parent) + PlayerCard.ActualWidth / 2, 
                    Canvas.GetTop((StackPanel)PlayerCard.Parent) + ((StackPanel)PlayerCard.Parent).Children.IndexOf(PlayerCard) * 100 + PlayerCard.ActualHeight / 2),
                Colors.Red, delegate(Line ActionLine)
                {
                    DiceControl.Expected = 6 - App.CurrentUser.BattleData.AttackSuccessRatio;
                    DiceControl.EndAction = delegate
                    { AttackAction(App.CurrentUser, PlayerCard.Player, ActionLine, DiceControl.Success); };
                    DiceControl.Expand();
                });
            PlayerCard.Player.BattleData.SetStatus("Breakdown");
            App.CurrentUser.BattleData.PlayerCard.SetAction("攻击", "Attack", "选择一个目标进行常规攻击，攻击成功将能造成" +
                App.CurrentUser.BattleData.AttackDamage + "点伤害。",
                App.CurrentUser.BattleData.AttackSuccessRatio);
            SetStatus(PlayerCard.Player.Name + "正遭受攻击！", Colors.OrangeRed);
        }
        private void AttackCancel_Click(object sender, MouseButtonEventArgs e)
        {
            foreach (var EachPlayerCard in Battlefield.PlayerList.Select(O => O.BattleData.PlayerCard))
            {
                EachPlayerCard.MouseEnter -= AttackTarget_MouseEnter;
                EachPlayerCard.MouseLeave -= AttackTarget_MouseLeave;
                EachPlayerCard.MouseLeftButtonUp -= AttackTarget_MouseClick;
                if (EachPlayerCard.IsMouseOver)
                {
                    EachPlayerCard.ToolTip = null;
                    EachPlayerCard.InfoBorder.Background = EachPlayerCard.InfoBorder.Background.Clone();
                    ((SolidColorBrush)EachPlayerCard.InfoBorder.Background).BeginAnimation(
                        SolidColorBrush.ColorProperty, new ColorAnimation()
                        {
                            From = ((SolidColorBrush)EachPlayerCard.InfoBorder.Background).Color,
                            To = Colors.White,
                            Duration = TimeSpan.FromSeconds(0.3),
                            EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                        });
                }
            }
            this.MouseRightButtonUp -= AttackCancel_Click;

            SetStatus("角色：" + App.CurrentUser.Name + "的回合", Colors.LimeGreen);
            OpenAttackOptions(true);
        }
        private void AttackAbondon_Click(object sender, MouseButtonEventArgs e)
        {
            CenterBorderCommand(new List<KeyValuePair<string, Action>>()
            {
                new KeyValuePair<string, Action>("HideCenterGrid", delegate
                {
                    SetStatus(App.CurrentUser.Name + "放弃了本次攻击机会。", Colors.Gray);
                    App.CurrentUser.BattleData.AttackSuccessRatio += 1;
                    if (App.CurrentUser.BattleData.AttackSuccessRatio > 6)
                        App.CurrentUser.BattleData.AttackSuccessRatio = 6;
                    App.CurrentUser.BattleData.PlayerCard.SetAction("放弃", "Abondon",
                        App.CurrentUser.Name + "放弃了本次攻击机会，下回合" + App.CurrentUser.Name +
                        "的攻击成功率变为：" + App.CurrentUser.BattleData.AttackSuccessRatio + "/6。", null);
                }),
                new KeyValuePair<string, Action>("HideCenterBorder", NextPlayer)
            });
        }
        private void Defend_Click(object sender, MouseButtonEventArgs e)
        {
            CenterBorderCommand(new List<KeyValuePair<string, Action>>()
            {
                new KeyValuePair<string, Action>("HideCenterGrid", delegate
                {
                    var SelfCard = App.CurrentUser.BattleData.PlayerCard;
                    SelfCard.SetAction("防御", "Defend", "使用防御来削减来自对方的伤害，当前可降低" +
                        App.CurrentUser.BattleData.DefenceCapacity + "点伤害。", App.CurrentUser.BattleData.DefendSuccessRatio);
                    SetStatus(App.CurrentUser.Name + "选择防御！", Colors.OrangeRed);
                }),
                new KeyValuePair<string, Action>("HideCenterBorder", delegate
                {
                    DiceControl.Expected = 6 - App.CurrentUser.BattleData.DefendSuccessRatio;
                    DiceControl.EndAction = delegate
                    { DefendAction(Battlefield.CurrentPlayer, App.CurrentUser, DiceControl.Success); };
                    DiceControl.Expand();
                })
            });
        }
        private void DefendAbondon_Click(object sender, MouseButtonEventArgs e)
        {
            CenterBorderCommand(new List<KeyValuePair<string, Action>>()
            {
                new KeyValuePair<string, Action>("HideCenterGrid", delegate
                {
                    SetStatus(App.CurrentUser.Name + "放弃了本次防御机会。", Colors.Gray);
                    App.CurrentUser.BattleData.DefendSuccessRatio += 2;
                    if (App.CurrentUser.BattleData.DefendSuccessRatio > 6)
                        App.CurrentUser.BattleData.DefendSuccessRatio = 6;
                    App.CurrentUser.BattleData.PlayerCard.SetAction("放弃", "Abondon",
                        App.CurrentUser.Name + "放弃了本次防御机会，下回合" + App.CurrentUser.Name +
                        "的防御成功率变为：" + App.CurrentUser.BattleData.DefendSuccessRatio + "/6。", null);
                }),
                new KeyValuePair<string, Action>("HideCenterBorder", delegate
                {
                    App.CurrentUser.BattleData.CurrentHp -=
                        Battlefield.CurrentPlayer.BattleData.AttackDamage;
                    NextPlayer();
                })
            });
        }
        private void Counter_Click(object sender, MouseButtonEventArgs e)
        {
            CenterBorderCommand(new List<KeyValuePair<string, Action>>()
            {
                new KeyValuePair<string, Action>("HideCenterGrid", delegate
                {
                    Battlefield.CurrentPlayer.BattleData.SetStatus("Breakdown");
                    
                    App.CurrentUser.BattleData.PlayerCard.SetAction("反击", "Counter", "该角色正在进行反击，反击成功将能造成" +
                        App.CurrentUser.BattleData.CounterDamage + "点伤害，若失败自己将遭受" +
                        Battlefield.CurrentPlayer.BattleData.CounterPunishment + "点伤害。",
                        App.CurrentUser.BattleData.CounterSuccessRatio);
                    SetStatus(App.CurrentUser.Name + "选择反击！", Colors.OrangeRed);
                }),
                new KeyValuePair<string, Action>("HideCenterBorder", delegate
                {
                    var SelfCard = App.CurrentUser.BattleData.PlayerCard;
                    var TargetCard = Battlefield.CurrentPlayer.BattleData.PlayerCard;
                    ActionLineAnimation(
                        new Point(Canvas.GetLeft((StackPanel)SelfCard.Parent) + SelfCard.ActualWidth / 2,
                            Canvas.GetTop((StackPanel)SelfCard.Parent) + ((StackPanel)SelfCard.Parent).Children.IndexOf(SelfCard) * 100 + SelfCard.ActualHeight / 2),
                        new Point(Canvas.GetLeft((StackPanel)TargetCard.Parent) + TargetCard.ActualWidth / 2,
                            Canvas.GetTop((StackPanel)TargetCard.Parent) + ((StackPanel)TargetCard.Parent).Children.IndexOf(TargetCard) * 100 + TargetCard.ActualHeight / 2),
                        Colors.OrangeRed, delegate(Line ActionLine)
                        {
                            DiceControl.Expected = 6 - App.CurrentUser.BattleData.CounterSuccessRatio;
                            DiceControl.EndAction = delegate
                            { CounterAction(App.CurrentUser, Battlefield.CurrentPlayer, ActionLine, DiceControl.Success); };
                            DiceControl.Expand();
                        });
                    //Battlefield.CurrentAttacker = App.CurrentUser;
                })
            });
        }

        private void AttackAction(Player Attacker, Player Defender, Line ActionLine, bool Success)
        {
            if (Success)
            {
                DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.5) };
                Timer.Tick += delegate
                {
                    SetStatus(Defender.Name + "正在做出防御选项...", Colors.OrangeRed);
                    PathCanvas.Children.Remove(ActionLine);

                    if (Defender is AI)
                        SetAiDefendOptions((AI)Defender);
                    else if (Defender.Id == App.CurrentUser.Id)
                        OpenDefendOptions();
                    else
                    { }

                    Timer.Stop();
                };

                Battlefield.Record.Participants[Attacker.Id].Rolls.Add(
                    new DiceRoll()
                    {
                        Probability = new KeyValuePair<int, int>(Attacker.BattleData.AttackSuccessRatio, 6),
                        Type = DiceRoll.RollType.Attack,
                        Success = true,
                        Time = DateTime.Now,
                    });
                Attacker.BattleData.PlayerCard.UpdateLuck();
                Attacker.BattleData.AttackSuccessRatio = 2;

                ActionLine.BeginAnimation(Line.X1Property, new DoubleAnimation()
                {
                    From = ActionLine.X1,
                    To = ActionLine.X2,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                });
                ActionLine.BeginAnimation(Line.Y1Property, new DoubleAnimation()
                {
                    From = ActionLine.Y1,
                    To = ActionLine.Y2,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                });
                ActionLine.BeginAnimation(Line.StrokeThicknessProperty, new DoubleAnimation()
                {
                    From = 5,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.5),
                });
                Timer.Start();
            }
            else
            {
                SetStatus(Attacker.Name + "攻击失败。", Colors.Gray);

                DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                Timer.Tick += delegate
                {
                    PathCanvas.Children.Remove(ActionLine);
                    NextPlayer();
                    Timer.Stop();
                };

                Battlefield.Record.Participants[Attacker.Id].Rolls.Add(
                    new DiceRoll()
                    {
                        Probability = new KeyValuePair<int, int>(Attacker.BattleData.AttackSuccessRatio, 6),
                        Type = DiceRoll.RollType.Attack,
                        Success = false,
                        Time = DateTime.Now,
                    }); 
                Attacker.BattleData.PlayerCard.UpdateLuck();
                Attacker.BattleData.AttackSuccessRatio = 2;

                Defender.BattleData.SetStatus("Joined");

                ActionLine.BeginAnimation(Line.X2Property, new DoubleAnimation()
                {
                    From = ActionLine.X2,
                    To = ActionLine.X1,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                });
                ActionLine.BeginAnimation(Line.Y2Property, new DoubleAnimation()
                {
                    From = ActionLine.Y2,
                    To = ActionLine.Y1,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                });
                ActionLine.BeginAnimation(Line.StrokeThicknessProperty, new DoubleAnimation()
                {
                    From = 5,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.5),
                });
                Timer.Start();
            }
        }
        private void DefendAction(Player Attacker, Player Defender, bool Success)
        {
            if (Success)
            {
                SetStatus(Defender.Name + "防御成功！", Colors.LimeGreen);

                DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.5) };
                Timer.Tick += delegate
                {
                    Defender.BattleData.SetStatus("Joined");
                    NextPlayer();

                    Timer.Stop();
                };

                Battlefield.Record.Participants[Defender.Id].Rolls.Add(
                    new DiceRoll()
                    {
                        Probability = new KeyValuePair<int, int>(Defender.BattleData.DefendSuccessRatio, 6),
                        Type = DiceRoll.RollType.Defense,
                        Success = true,
                        Time = DateTime.Now,
                    });
                Defender.BattleData.PlayerCard.UpdateLuck();
                Defender.BattleData.CurrentHp -= 
                    Attacker.BattleData.AttackDamage - Defender.BattleData.DefenceCapacity;
                Defender.BattleData.DefendSuccessRatio = 2;

                Timer.Start();
            }
            else
            {
                SetStatus(Defender.Name + "防御失败。", Colors.Gray);

                DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                Timer.Tick += delegate
                {
                    Defender.BattleData.SetStatus("Joined");
                    NextPlayer();

                    Timer.Stop();
                };

                Battlefield.Record.Participants[Defender.Id].Rolls.Add(
                    new DiceRoll()
                    {
                        Probability = new KeyValuePair<int, int>(Defender.BattleData.DefendSuccessRatio, 6),
                        Type = DiceRoll.RollType.Defense,
                        Success = false,
                        Time = DateTime.Now
                    });
                Defender.BattleData.PlayerCard.UpdateLuck();
                Defender.BattleData.CurrentHp -= Attacker.BattleData.AttackDamage;
                Defender.BattleData.DefendSuccessRatio = 2;

                Timer.Start();
            }
        }
        private void CounterAction(Player Attacker, Player Defender, Line ActionLine, bool Success)
        {
            if (Success)
            {
                SetStatus(Attacker.Name + "反击成功！", Colors.LimeGreen);

                DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.5) };
                Timer.Tick += delegate
                {
                    Attacker.BattleData.SetStatus("Joined");
                    Defender.BattleData.SetStatus("Ready");
                    PathCanvas.Children.Remove(ActionLine);

                    NextPlayer();

                    Timer.Stop();
                };

                Battlefield.Record.Participants[Attacker.Id].Rolls.Add(
                    new DiceRoll()
                    {
                        Probability = new KeyValuePair<int, int>(Attacker.BattleData.CounterSuccessRatio, 6),
                        Type = DiceRoll.RollType.Counter,
                        Success = true,
                        Time = DateTime.Now,
                    });
                Attacker.BattleData.PlayerCard.UpdateLuck();
                Defender.BattleData.CurrentHp -= Attacker.BattleData.CounterDamage;

                ActionLine.BeginAnimation(Line.X1Property, new DoubleAnimation()
                {
                    From = ActionLine.X1,
                    To = ActionLine.X2,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                });
                ActionLine.BeginAnimation(Line.Y1Property, new DoubleAnimation()
                {
                    From = ActionLine.Y1,
                    To = ActionLine.Y2,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                });
                ActionLine.BeginAnimation(Line.StrokeThicknessProperty, new DoubleAnimation()
                {
                    From = 5,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.5),
                });
                Timer.Start();
            }
            else
            {
                SetStatus(Attacker.Name + "反击失败。", Colors.Gray);

                DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                Timer.Tick += delegate
                {
                    Attacker.BattleData.SetStatus("Joined");
                    Defender.BattleData.SetStatus("Ready");
                    PathCanvas.Children.Remove(ActionLine);

                    NextPlayer();
                    Timer.Stop();
                };

                Battlefield.Record.Participants[Attacker.Id].Rolls.Add(
                    new DiceRoll()
                    {
                        Probability = new KeyValuePair<int, int>(Attacker.BattleData.CounterSuccessRatio, 6),
                        Type = DiceRoll.RollType.Counter,
                        Success = false,
                        Time = DateTime.Now,
                    });
                Attacker.BattleData.PlayerCard.UpdateLuck();
                Attacker.BattleData.CurrentHp -= Defender.BattleData.CounterPunishment;

                ActionLine.BeginAnimation(Line.X2Property, new DoubleAnimation()
                {
                    From = ActionLine.X2,
                    To = ActionLine.X1,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                });
                ActionLine.BeginAnimation(Line.Y2Property, new DoubleAnimation()
                {
                    From = ActionLine.Y2,
                    To = ActionLine.Y1,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                });
                ActionLine.BeginAnimation(Line.StrokeThicknessProperty, new DoubleAnimation()
                {
                    From = 5,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.5),
                });
                Timer.Start();
            }
        }

        private void Close_Click(object Sender, RoutedEventArgs E)
        {
            
        }
    }
}
