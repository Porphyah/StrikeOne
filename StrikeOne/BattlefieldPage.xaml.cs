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
using StrikeOne.Core.Lua;
using TimeSpan = System.TimeSpan;

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
        public Action LeaveAction { set; private get; }

        public void PageEnter(Battlefield Source)
        {
            Battlefield = Source;

            InitLuaUi();
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
            var GroupPanels = CardCanvas.Children.OfType<StackPanel>().ToArray();
            int Count = 0;
            switch (Battlefield.Room.BattleType)
            {
                case BattleType.OneVsOne:
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
                case BattleType.TriangleMess:
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
                                ArcAnimation(90, 225);
                                ShowGroupPanel(GroupPanels[1]);
                                break;
                            case 3:
                                ExpandGroupPanel(GroupPanels[1]);
                                break;
                            case 4:
                                ArcAnimation(225, 315);
                                ShowGroupPanel(GroupPanels[2]);
                                break;
                            case 5:
                                ExpandGroupPanel(GroupPanels[2]);
                                break;
                            case 6:
                                ArcAnimation(315, 90);
                                break;
                            case 7:
                                SetStatus("Ready", Colors.LimeGreen);
                                break;
                            case 8:
                                NextRound();
                                Timer.Stop();
                                break;
                        }
                        Count++;
                    };
                    Timer.Start();
                    break;
                case BattleType.SquareMess:
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
                                ArcAnimation(45, 135);
                                ShowGroupPanel(GroupPanels[1]);
                                break;
                            case 3:
                                ExpandGroupPanel(GroupPanels[1]);
                                break;
                            case 4:
                                ArcAnimation(135, 225);
                                ShowGroupPanel(GroupPanels[2]);
                                break;
                            case 5:
                                ExpandGroupPanel(GroupPanels[2]);
                                break;
                            case 6:
                                ArcAnimation(225, 315);
                                ShowGroupPanel(GroupPanels[3]);
                                break;
                            case 7:
                                ExpandGroupPanel(GroupPanels[3]);
                                break;
                            case 8:
                                ArcAnimation(315, 45);
                                break;
                            case 9:
                                SetStatus("Ready", Colors.LimeGreen);
                                break;
                            case 10:
                                NextRound();
                                Timer.Stop();
                                break;
                        }
                        Count++;
                    };
                    Timer.Start();
                    break;
                case BattleType.TwinningFight:
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
                                ArcAnimation(90, 270);
                                ShowGroupPanel(GroupPanels[1]);
                                break;
                            case 3:
                                ExpandGroupPanel(GroupPanels[1]);
                                break;
                            case 4:
                                ArcAnimation(270, 90);
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
        public void PageLeave()
        {
            DoubleAnimation OpacityAnimation = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(1.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            };
            OpacityAnimation.Completed += delegate
            {
                LeaveAction?.Invoke();
            };
            this.BeginAnimation(OpacityProperty, OpacityAnimation);
        }

        private void InitLuaUi()
        {
            LuaUI Instance = LuaMain.LuaState["UI"] as LuaUI;
            Instance.SetGlobalStatusAction = SetStatus;
            Instance.DiceControlAction = delegate(int Probability, Action EndAction)
            {
                DiceControl.Expected = 6 - Probability;
                DiceControl.EndAction = EndAction;
                DiceControl.Expand();
            };
            Instance.SetStatusImgAction = SetStatusImg;
            Instance.RemoveStatusImgAction = RemoveStatusImg;
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

            Player.BattleData.OnTurnAction();
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
                App.CurrentUser.BattleData.AttackDamage.GetDouble() + "点伤害。",
                App.CurrentUser.BattleData.AttackSuccessRatio.GetInt());
            AttackButton.MouseUp += Attack_Click;
            CenterGrid.Children.Add(AttackButton);

            var AbondonButton = new ActionButton() { Height = 100, Width = 85 };
            AbondonButton.Init("放弃", "Abondon", "该回合不做任何行动，并使下一回合攻击成功的概率增加1/6。");
            AbondonButton.MouseUp += AttackAbondon_Click;
            CenterGrid.Children.Add(AbondonButton);

            if (App.CurrentUser.BattleData.Skill != null &&
                App.CurrentUser.BattleData.Skill.IsActive)
            {
                var SkillButton = new ActionButton() {Height = 100, Width = 85};
                if (App.CurrentUser.BattleData.Skill.Enable)
                {
                    SkillButton.Init("使用技能", "Skill_white",
                        "使用您的技能：" + App.CurrentUser.BattleData.Skill.Name + "。",
                        App.CurrentUser.BattleData.Skill.Probability);
                    SkillButton.MouseUp += Skill_Click;
                }
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
                App.CurrentUser.BattleData.DefenceCapacity.GetDouble() + "点伤害。",
                App.CurrentUser.BattleData.DefendSuccessRatio.GetInt());
            DefendButton.MouseUp += Defend_Click;
            CenterGrid.Children.Add(DefendButton);

            var CounterButton = new ActionButton() { Height = 100, Width = 85 };
            CounterButton.Init("反击", "Counter", "是时候反击了！反击可给予对方" +
                App.CurrentUser.BattleData.CounterDamage.GetDouble() + "点伤害，但一旦失败则会给自己造成额外伤害。",
                App.CurrentUser.BattleData.CounterSuccessRatio.GetInt());
            CounterButton.MouseUp += Counter_Click;
            CenterGrid.Children.Add(CounterButton);

            var AbondonButton = new ActionButton() { Height = 100, Width = 85 };
            AbondonButton.Init("放弃", "Abondon", "默默忍受本次攻击，并使下一回合防御成功的概率增加1/3。");
            AbondonButton.MouseUp += DefendAbondon_Click;
            CenterGrid.Children.Add(AbondonButton);

            if (App.CurrentUser.BattleData.Skill != null &&
               !App.CurrentUser.BattleData.Skill.IsActive)
            {
                var SkillButton = new ActionButton() { Height = 100, Width = 85 };
                if (App.CurrentUser.BattleData.Skill.Enable)
                {
                    SkillButton.Init("使用技能", "Skill_white",
                        "使用您的技能：" + App.CurrentUser.BattleData.Skill.Name + "。",
                        App.CurrentUser.BattleData.Skill.Probability);
                    SkillButton.MouseUp += Skill_Click;
                }
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

                    DoingEventArgs AttackingEvent = new DoingEventArgs()
                    {
                        Attacker = Player,
                        Defender = Target,
                        Continue = true,
                        DoingSuccessRatio = Player.BattleData.AttackSuccessRatio.GetDouble(),
                        DoingValue = Player.BattleData.AttackDamage.GetDouble()
                    };
                    Player.BattleData.OnAttacking(AttackingEvent);
                    Player.BattleData.AttackDamage.Tag = AttackingEvent.DoingValue;
                    Player.BattleData.AttackSuccessRatio.Tag = AttackingEvent.DoingSuccessRatio;

                    var SelfCard = Player.BattleData.PlayerCard;
                    var PlayerCard = Target.BattleData.PlayerCard;
                    ActionLineAnimation(
                        new Point(Canvas.GetLeft((StackPanel)SelfCard.Parent) + SelfCard.ActualWidth / 2,
                            Canvas.GetTop((StackPanel)SelfCard.Parent) + ((StackPanel)SelfCard.Parent).Children.IndexOf(SelfCard) * 100 + SelfCard.ActualHeight / 2),
                        new Point(Canvas.GetLeft((StackPanel)PlayerCard.Parent) + PlayerCard.ActualWidth / 2,
                            Canvas.GetTop((StackPanel)PlayerCard.Parent) + ((StackPanel)PlayerCard.Parent).Children.IndexOf(PlayerCard) * 100 + PlayerCard.ActualHeight / 2),
                        Colors.Red, delegate (Line ActionLine)
                        {
                            DiceControl.Expected = 6 - (int)Math.Round(AttackingEvent.DoingSuccessRatio);
                            DiceControl.EndAction = delegate
                            { AttackAction(Player, Target, ActionLine, DiceControl.Success); };
                            DiceControl.Expand();
                        });
                    PlayerCard.Player.BattleData.SetStatus("Breakdown");
                    SelfCard.SetAction("攻击", "Attack", "选择一个目标进行常规攻击，攻击成功将能造成" +
                        AttackingEvent.DoingValue + "点伤害。", (int)Math.Round(AttackingEvent.DoingSuccessRatio));
                    SetStatus(Target.Name + "正遭受攻击！", Colors.OrangeRed);
                    break;
                case "Skill":
                    SkillAction_SelectTarget(Player.BattleData.Skill);
                    SkillAction_Prepare(Player.BattleData.Skill);
                    break;
                case "Abondon":
                    SetStatus(Player.Name + "放弃了本次攻击机会。", Colors.Gray);
                    Player.BattleData.AttackSuccessRatio.UpdateVariable(
                        Guid.Parse("00000000-0000-0000-0000-000000000000"), 1, false);
                    Player.BattleData.PlayerCard.SetAction("放弃", "Abondon",
                        Player.Name + "放弃了本次攻击机会，下回合" + Player.Name +
                        "的攻击成功率变为：" + Player.BattleData.AttackSuccessRatio.GetInt() + "/6。", 
                        null);

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
                        Player.BattleData.DefenceCapacity.GetDouble() + "点伤害。", 
                        Player.BattleData.DefendSuccessRatio.GetInt());
                    SetStatus(Player.Name + "选择防御！", Colors.OrangeRed);

                    DoingEventArgs DefendingEvent = new DoingEventArgs()
                    {
                        Attacker = Battlefield.CurrentPlayer,
                        Defender = Player,
                        Continue = true,
                        DoingValue = Player.BattleData.DefenceCapacity.GetDouble(),
                        DoingSuccessRatio = Player.BattleData.DefendSuccessRatio.GetDouble()
                    };
                    Player.BattleData.OnDefending(DefendingEvent);
                    Player.BattleData.DefenceCapacity.Tag = DefendingEvent.DoingValue;
                    Player.BattleData.DefendSuccessRatio.Tag = DefendingEvent.DoingSuccessRatio;

                    DiceControl.Expected = 6 - (int)Math.Round(DefendingEvent.DoingSuccessRatio);
                    DiceControl.EndAction = delegate
                    { DefendAction(Battlefield.CurrentPlayer, Player, DiceControl.Success); };
                    DiceControl.Expand();
                    break;
                case "Counter":
                    DoingEventArgs CounteringEvent = new DoingEventArgs()
                    {
                        Attacker = Player,
                        Defender = Battlefield.CurrentPlayer,
                        Continue = true,
                        DoingValue = Player.BattleData.CounterDamage.GetDouble(),
                        DoingSuccessRatio = Player.BattleData.CounterSuccessRatio.GetDouble()
                    };
                    Player.BattleData.OnCounterAttacking(CounteringEvent);
                    Player.BattleData.CounterDamage.Tag = CounteringEvent.DoingValue;
                    Player.BattleData.CounterSuccessRatio.Tag = CounteringEvent.DoingSuccessRatio;

                    var PlayerCard = Battlefield.CurrentPlayer.BattleData.PlayerCard;
                    ActionLineAnimation(
                        new Point(Canvas.GetLeft((StackPanel)SelfCard.Parent) + SelfCard.ActualWidth / 2,
                            Canvas.GetTop((StackPanel)SelfCard.Parent) + ((StackPanel)SelfCard.Parent).Children.IndexOf(SelfCard) * 100 + SelfCard.ActualHeight / 2),
                        new Point(Canvas.GetLeft((StackPanel)PlayerCard.Parent) + PlayerCard.ActualWidth / 2,
                            Canvas.GetTop((StackPanel)PlayerCard.Parent) + ((StackPanel)PlayerCard.Parent).Children.IndexOf(PlayerCard) * 100 + PlayerCard.ActualHeight / 2),
                        Colors.OrangeRed, delegate (Line ActionLine)
                        {
                            DiceControl.Expected = 6 - (int)Math.Round(CounteringEvent.DoingSuccessRatio);
                            DiceControl.EndAction = delegate
                            { CounterAction(Player, Battlefield.CurrentPlayer, ActionLine, DiceControl.Success); };
                            DiceControl.Expand();
                        });

                    Player.BattleData.PlayerCard.SetAction("反击", "Counter", "该角色正在进行反击，反击成功将能造成" +
                        Player.BattleData.CounterDamage.GetDouble() + "点伤害，若失败自己将遭受" +
                        Battlefield.CurrentPlayer.BattleData.CounterPunishment.GetDouble() + "点伤害。",
                        Player.BattleData.CounterSuccessRatio.GetInt());
                    SetStatus(Player.Name + "选择反击！", Colors.OrangeRed);
                    Battlefield.CurrentPlayer.BattleData.SetStatus("Breakdown");
                    break;
                case "Skill":
                    SkillAction_SelectTarget(Player.BattleData.Skill);
                    SkillAction_Prepare(Player.BattleData.Skill);
                    break;
                case "Abondon":
                    SetStatus(Player.Name + "放弃了本次防御机会。", Colors.Gray);
                    Player.BattleData.DefendSuccessRatio.UpdateVariable(
                        Guid.Parse("00000000-0000-0000-0000-000000000000"), 2, false);
                    Player.BattleData.PlayerCard.SetAction("放弃", "Abondon",
                        Player.Name + "放弃了本次防御机会，下回合" + Player.Name +
                        "的防御成功率变为：" + Player.BattleData.DefendSuccessRatio.GetInt() + "/6。", null);

                    DispatcherTimer AbondonTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                    AbondonTimer.Tick += delegate
                    {
                        Player.BattleData.CurrentHp.SetBasic(Player.BattleData.CurrentHp.GetDouble() -
                            (double)Battlefield.CurrentPlayer.BattleData.AttackDamage.Tag);
                        Player.BattleData.Capacity += (double)Battlefield.CurrentPlayer.BattleData.AttackDamage.Tag;
                        if (Player.Inclination == AttackInclination.Vindictive)
                            Player.PlayerDataCollection[Battlefield.CurrentPlayer] += 2;
                        if (Battlefield.CurrentPlayer is AI &&
                            ((AI)Battlefield.CurrentPlayer).Inclination == AttackInclination.TargetHard)
                            ((AI)Battlefield.CurrentPlayer).PlayerDataCollection[App.CurrentUser] += 2;

                        if (Player.BattleData.CurrentHp.GetInt() == 0)
                        {
                            Battlefield.CurrentPlayer.BattleData.Executed.Add(App.CurrentUser);
                            Player.BattleData.SetStatus("Nobody");
                            SetStatus(Player.Name + "已被" + Battlefield.CurrentPlayer.Name + "击杀！", Colors.Gray);
                        }
                        else
                            Player.BattleData.SetStatus("Joined");

                        if (Battlefield.IsGameOver())
                        {
                            var Group = Battlefield.GetWinnerGroup();
                            if (App.CurrentUser.BattleData == null)
                                GameOver();
                            else if (Group.Name == App.CurrentUser.BattleData.Group.Name)
                                Victory();
                            else
                                Defeated();
                        }
                        else
                            NextPlayer();

                        AbondonTimer.Stop();
                    };
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
                case BattleType.TriangleMess:
                    switch (GroupIndex)
                    {
                        case 0: SetItemPosition(Item, 90); break;
                        case 1: SetItemPosition(Item, 225); break;
                        case 2: SetItemPosition(Item, 315); break;
                    }
                    break;
                case BattleType.SquareMess:
                    switch (GroupIndex)
                    {
                        case 0: SetItemPosition(Item, 45); break;
                        case 1: SetItemPosition(Item, 135); break;
                        case 2: SetItemPosition(Item, 225); break;
                        case 3: SetItemPosition(Item, 315); break;
                    }
                    break;
                case BattleType.TwinningFight:
                    switch (GroupIndex)
                    {
                        case 0: SetItemPosition(Item, 90); break;
                        case 1: SetItemPosition(Item, 270); break;
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
                    To = PlayerCard.Player.BattleData.CurrentHp.GetDouble(),
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
        private void SetStatusImg(PlayerCard Card, Guid Id, BitmapImage BitmapImage, string Description)
        {
            Card.StatusPanel.Children.Add(new Image()
            {
                Height = 20,
                Source = BitmapImage,
                ToolTip = Description,
                Tag = Id
            });
        }
        private void RemoveStatusImg(PlayerCard Card, Guid Id)
        {
            var Image = Card.StatusPanel.Children.OfType<Image>()
                .First(O => (Guid) O.Tag == Id);
            if (Image == null) return;
            Card.StatusPanel.Children.Remove(Image);
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
                case "ShowSkillConfirmControl":
                    SkillConfirmControl.Visibility = Visibility.Visible;
                    DispatcherTimer ShowControlTimer = new DispatcherTimer()
                    { Interval = TimeSpan.FromSeconds(0.5) };
                    ShowControlTimer.Tick += delegate
                    {
                        Commands[0].Value?.Invoke();
                        if (Commands.Count > 1)
                            CenterBorderCommand(Commands.Skip(1).ToList());
                        ShowControlTimer.Stop();
                    };
                    SkillConfirmControl.BeginAnimation(OpacityProperty, new DoubleAnimation()
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    });
                    SkillConfirmControl.BeginAnimation(MarginProperty, new ThicknessAnimation()
                    {
                        From = new Thickness(50, 0, -50, 0),
                        To = new Thickness(0, 0, 0, 0),
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    });
                    ShowControlTimer.Start();
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
                case "HideSkillConfirmControl":
                    DispatcherTimer HideControlTimer = new DispatcherTimer()
                    { Interval = TimeSpan.FromSeconds(0.5) };
                    HideControlTimer.Tick += delegate
                    {
                        SkillConfirmControl.Visibility = Visibility.Hidden;
                        Commands[0].Value?.Invoke();
                        if (Commands.Count > 1)
                            CenterBorderCommand(Commands.Skip(1).ToList());
                        HideControlTimer.Stop();
                    };
                    SkillConfirmControl.BeginAnimation(OpacityProperty, new DoubleAnimation()
                    {
                        From = SkillConfirmControl.Opacity,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                    });
                    SkillConfirmControl.BeginAnimation(MarginProperty, new ThicknessAnimation()
                    {
                        From = SkillConfirmControl.Margin,
                        To = new Thickness(-50, 0, 50, 0),
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                    });
                    HideControlTimer.Start();
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
                !(PlayerCard.Player.BattleData.CurrentHp.GetDouble() > 0))
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
                !(PlayerCard.Player.BattleData.CurrentHp.GetDouble() > 0))
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

            DoingEventArgs AttackingEvent = new DoingEventArgs()
            {
                Attacker = App.CurrentUser,
                Defender = PlayerCard.Player,
                Continue = true,
                DoingSuccessRatio = App.CurrentUser.BattleData.AttackSuccessRatio.GetDouble(),
                DoingValue = App.CurrentUser.BattleData.AttackDamage.GetDouble()
            };
            App.CurrentUser.BattleData.OnAttacking(AttackingEvent);
            App.CurrentUser.BattleData.AttackDamage.Tag = AttackingEvent.DoingValue;
            App.CurrentUser.BattleData.AttackSuccessRatio.Tag = AttackingEvent.DoingSuccessRatio;

            var SelfCard = App.CurrentUser.BattleData.PlayerCard;
            ActionLineAnimation(
                new Point(Canvas.GetLeft((StackPanel)SelfCard.Parent) + SelfCard.ActualWidth / 2, 
                    Canvas.GetTop((StackPanel)SelfCard.Parent) + ((StackPanel)SelfCard.Parent).Children.IndexOf(SelfCard) * 100 + SelfCard.ActualHeight / 2),
                new Point(Canvas.GetLeft((StackPanel)PlayerCard.Parent) + PlayerCard.ActualWidth / 2, 
                    Canvas.GetTop((StackPanel)PlayerCard.Parent) + ((StackPanel)PlayerCard.Parent).Children.IndexOf(PlayerCard) * 100 + PlayerCard.ActualHeight / 2),
                Colors.Red, delegate(Line ActionLine)
                {
                    DiceControl.Expected = 6 - (int)Math.Round(AttackingEvent.DoingSuccessRatio);
                    DiceControl.EndAction = delegate
                    { AttackAction(App.CurrentUser, PlayerCard.Player, ActionLine, DiceControl.Success); };
                    DiceControl.Expand();
                });
            PlayerCard.Player.BattleData.SetStatus("Breakdown");
            App.CurrentUser.BattleData.PlayerCard.SetAction("攻击", "Attack", "选择一个目标进行常规攻击，攻击成功将能造成" +
                 AttackingEvent.DoingValue + "点伤害。", (int)Math.Round(AttackingEvent.DoingSuccessRatio));
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
                    App.CurrentUser.BattleData.AttackSuccessRatio.UpdateVariable(
                        Guid.Parse("00000000-0000-0000-0000-000000000000"), 1, false);
                    App.CurrentUser.BattleData.PlayerCard.SetAction("放弃", "Abondon",
                        App.CurrentUser.Name + "放弃了本次攻击机会，下回合" + App.CurrentUser.Name +
                        "的攻击成功率变为：" + App.CurrentUser.BattleData.AttackSuccessRatio.GetInt() + "/6。", null);
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
                        App.CurrentUser.BattleData.DefenceCapacity.GetDouble() + "点伤害。", 
                        App.CurrentUser.BattleData.DefendSuccessRatio.GetInt());
                    SetStatus(App.CurrentUser.Name + "选择防御！", Colors.OrangeRed);
                }),
                new KeyValuePair<string, Action>("HideCenterBorder", delegate
                {
                    DoingEventArgs DefendingEvent = new DoingEventArgs()
                    {
                        Attacker = Battlefield.CurrentPlayer,
                        Defender = App.CurrentUser,
                        Continue = true,
                        DoingValue = App.CurrentUser.BattleData.DefenceCapacity.GetDouble(),
                        DoingSuccessRatio = App.CurrentUser.BattleData.DefendSuccessRatio.GetDouble()
                    };
                    App.CurrentUser.BattleData.OnDefending(DefendingEvent);
                    App.CurrentUser.BattleData.DefenceCapacity.Tag = DefendingEvent.DoingValue;
                    App.CurrentUser.BattleData.DefendSuccessRatio.Tag = DefendingEvent.DoingSuccessRatio;

                    DiceControl.Expected = 6 - (int)Math.Round(DefendingEvent.DoingSuccessRatio);
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
                    App.CurrentUser.BattleData.DefendSuccessRatio.UpdateVariable(
                        Guid.Parse("00000000-0000-0000-0000-000000000000"), 2, false);
                    App.CurrentUser.BattleData.PlayerCard.SetAction("放弃", "Abondon",
                        App.CurrentUser.Name + "放弃了本次防御机会，下回合" + App.CurrentUser.Name +
                        "的防御成功率变为：" + App.CurrentUser.BattleData.DefendSuccessRatio.GetInt() + "/6。", null);
                }),
                new KeyValuePair<string, Action>("HideCenterBorder", delegate
                {
                    App.CurrentUser.BattleData.CurrentHp.SetBasic(
                        App.CurrentUser.BattleData.CurrentHp.GetDouble() -
                        (double)Battlefield.CurrentPlayer.BattleData.AttackDamage.Tag);
                    App.CurrentUser.BattleData.Capacity += (double)Battlefield.CurrentPlayer.BattleData.AttackDamage.Tag;
                    if (Battlefield.CurrentPlayer is AI && 
                        ((AI)Battlefield.CurrentPlayer).Inclination == AttackInclination.TargetHard)
                        ((AI)Battlefield.CurrentPlayer).PlayerDataCollection[App.CurrentUser] += 2;

                    if (App.CurrentUser.BattleData.CurrentHp.GetInt() == 0)
                    {
                        Battlefield.CurrentPlayer.BattleData.Executed.Add(App.CurrentUser);
                        App.CurrentUser.BattleData.SetStatus("Nobody");
                        SetStatus(App.CurrentUser.Name + "已被" + Battlefield.CurrentPlayer.Name + "击杀！", Colors.Gray);
                    }
                    else
                        App.CurrentUser.BattleData.SetStatus("Joined");

                    if (Battlefield.IsGameOver())
                    {
                        var Group = Battlefield.GetWinnerGroup();
                        if (Group.Name == App.CurrentUser.BattleData.Group.Name)
                            Victory();
                        else
                            Defeated();
                    }
                    else
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
                        App.CurrentUser.BattleData.CounterDamage.GetDouble() + "点伤害，若失败自己将遭受" +
                        Battlefield.CurrentPlayer.BattleData.CounterPunishment.GetDouble() + "点伤害。",
                        App.CurrentUser.BattleData.CounterSuccessRatio.GetInt());
                    SetStatus(App.CurrentUser.Name + "选择反击！", Colors.OrangeRed);
                }),
                new KeyValuePair<string, Action>("HideCenterBorder", delegate
                {
                    DoingEventArgs CounteringEvent = new DoingEventArgs()
                    {
                        Attacker = App.CurrentUser,
                        Defender = Battlefield.CurrentPlayer,
                        Continue = true,
                        DoingSuccessRatio = App.CurrentUser.BattleData.CounterSuccessRatio.GetDouble(),
                        DoingValue = App.CurrentUser.BattleData.CounterDamage.GetDouble()
                    };
                    App.CurrentUser.BattleData.OnCounterAttacking(CounteringEvent);
                    App.CurrentUser.BattleData.CounterDamage.Tag = CounteringEvent.DoingValue;
                    App.CurrentUser.BattleData.CounterSuccessRatio.Tag = CounteringEvent.DoingSuccessRatio;

                    var SelfCard = App.CurrentUser.BattleData.PlayerCard;
                    var TargetCard = Battlefield.CurrentPlayer.BattleData.PlayerCard;
                    ActionLineAnimation(
                        new Point(Canvas.GetLeft((StackPanel)SelfCard.Parent) + SelfCard.ActualWidth / 2,
                            Canvas.GetTop((StackPanel)SelfCard.Parent) + ((StackPanel)SelfCard.Parent).Children.IndexOf(SelfCard) * 100 + SelfCard.ActualHeight / 2),
                        new Point(Canvas.GetLeft((StackPanel)TargetCard.Parent) + TargetCard.ActualWidth / 2,
                            Canvas.GetTop((StackPanel)TargetCard.Parent) + ((StackPanel)TargetCard.Parent).Children.IndexOf(TargetCard) * 100 + TargetCard.ActualHeight / 2),
                        Colors.OrangeRed, delegate(Line ActionLine)
                        {
                            DiceControl.Expected = 6 - (int)Math.Round(CounteringEvent.DoingSuccessRatio);
                            DiceControl.EndAction = delegate
                            { CounterAction(App.CurrentUser, Battlefield.CurrentPlayer, ActionLine, DiceControl.Success); };
                            DiceControl.Expand();
                        });
                    //Battlefield.CurrentAttacker = App.CurrentUser;
                })
            });
        }
        private void Skill_Click(object sender, MouseButtonEventArgs e)
        {
            CenterBorderCommand(new List<KeyValuePair<string, Action>>()
            {
                new KeyValuePair<string, Action>("HideCenterGrid", delegate
                {
                    var Skill = App.CurrentUser.BattleData.Skill;
                    SkillConfirmControl.Init(Skill);
                    SkillConfirmControl.ConfirmAction = delegate
                    {
                        CenterBorderCommand(new List<KeyValuePair<string, Action>>()
                        {
                            new KeyValuePair<string, Action>("HideSkillConfirmControl", delegate
                            {
                                while (!SkillAction_SelectTarget(Skill))
                                    if (Skill.CurrentTarget.HasValue &&
                                        (Skill.CurrentTarget.Value.Key == SkillTarget.Enemy ||
                                         Skill.CurrentTarget.Value.Key == SkillTarget.Ally ||
                                         Skill.CurrentTarget.Value.Key == SkillTarget.AllyWithoutSelf))
                                        break;
                                if (Skill.CurrentTarget == null)
                                    SkillAction_Prepare(Skill);
                            }),
                            new KeyValuePair<string, Action>("HideCenterBorder", null)
                        });
                        SkillConfirmControl.ConfirmAction = null;
                    };
                    SkillConfirmControl.CancelAction = delegate
                    {
                        CenterBorderCommand(new List<KeyValuePair<string, Action>>()
                        {
                            new KeyValuePair<string, Action>("HideSkillConfirmControl", null),
                            new KeyValuePair<string, Action>("ShowCenterGrid", null)
                        });
                        SkillConfirmControl.CancelAction = null;
                    };
                }),
                new KeyValuePair<string, Action>("ShowSkillConfirmControl", null)
            });
        }
        private void SkillTarget_MouseEnter(object sender, MouseEventArgs e)
        {
            PlayerCard PlayerCard = (PlayerCard)sender;
            if (!((Func<Player, bool>)PlayerCard.Tag).Invoke(PlayerCard.Player))
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
                            Text = "该角色不能被作为技能使用对象。"
                        }
                    }
                };

                PlayerCard.InfoBorder.Background = PlayerCard.InfoBorder.Background.Clone();
                ((SolidColorBrush)PlayerCard.InfoBorder.Background).BeginAnimation(
                    SolidColorBrush.ColorProperty, new ColorAnimation()
                    {
                        From = ((SolidColorBrush)PlayerCard.InfoBorder.Background).Color,
                        To = Colors.LightGray,
                        Duration = TimeSpan.FromSeconds(0.3),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
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
                            Text = "该角色可被选为技能使用对象。"
                        }
                    }
                };

                PlayerCard.InfoBorder.Background = PlayerCard.InfoBorder.Background.Clone();
                ((SolidColorBrush)PlayerCard.InfoBorder.Background).BeginAnimation(
                    SolidColorBrush.ColorProperty, new ColorAnimation()
                    {
                        From = ((SolidColorBrush)PlayerCard.InfoBorder.Background).Color,
                        To = Colors.Khaki,
                        Duration = TimeSpan.FromSeconds(0.3),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    });
            }
        }
        private void SkillTarget_MouseLeave(object sender, MouseEventArgs e)
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
        private void SkillTarget_MouseClick(object sender, MouseButtonEventArgs e)
        {
            PlayerCard PlayerCard = (PlayerCard)sender;
            if (!((Func<Player, bool>)PlayerCard.Tag).Invoke(PlayerCard.Player))
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
                EachPlayerCard.MouseEnter -= SkillTarget_MouseEnter;
                EachPlayerCard.MouseLeave -= SkillTarget_MouseLeave;
                EachPlayerCard.MouseLeftButtonUp -= SkillTarget_MouseClick;
                EachPlayerCard.Tag = null;
            }
            this.MouseRightButtonUp -= SkillCancel_Click;

            var Skill = App.CurrentUser.BattleData.Skill;
            switch (Skill.CurrentTarget.Value.Key)
            {
                case SkillTarget.Enemy:
                case SkillTarget.Ally:
                case SkillTarget.AllyWithoutSelf:
                    Skill.TargetCollection.Add(PlayerCard.Player);
                    break;
            }
            while (!SkillAction_SelectTarget(Skill))
                if (Skill.CurrentTarget.HasValue &&
                    (Skill.CurrentTarget.Value.Key == SkillTarget.Enemy ||
                     Skill.CurrentTarget.Value.Key == SkillTarget.Ally ||
                     Skill.CurrentTarget.Value.Key == SkillTarget.AllyWithoutSelf))
                    break;
            if (Skill.CurrentTarget == null)
                SkillAction_Prepare(Skill);

            //var SelfCard = App.CurrentUser.BattleData.PlayerCard;
            //ActionLineAnimation(
            //    new Point(Canvas.GetLeft((StackPanel)SelfCard.Parent) + SelfCard.ActualWidth / 2,
            //        Canvas.GetTop((StackPanel)SelfCard.Parent) + ((StackPanel)SelfCard.Parent).Children.IndexOf(SelfCard) * 100 + SelfCard.ActualHeight / 2),
            //    new Point(Canvas.GetLeft((StackPanel)PlayerCard.Parent) + PlayerCard.ActualWidth / 2,
            //        Canvas.GetTop((StackPanel)PlayerCard.Parent) + ((StackPanel)PlayerCard.Parent).Children.IndexOf(PlayerCard) * 100 + PlayerCard.ActualHeight / 2),
            //    Colors.Orange, delegate (Line ActionLine)
            //    {
            //        DiceControl.Expected = 6 - App.CurrentUser.BattleData.AttackSuccessRatio;
            //        DiceControl.EndAction = delegate
            //        { AttackAction(App.CurrentUser, PlayerCard.Player, ActionLine, DiceControl.Success); };
            //        DiceControl.Expand();
            //    });
            //PlayerCard.Player.BattleData.SetStatus("Breakdown");
            //App.CurrentUser.BattleData.PlayerCard.SetAction("攻击", "Attack", "选择一个目标进行常规攻击，攻击成功将能造成" +
            //    App.CurrentUser.BattleData.AttackDamage + "点伤害。",
            //    App.CurrentUser.BattleData.AttackSuccessRatio);
            //SetStatus(App + "正遭受攻击！", Colors.OrangeRed);
        }
        private void SkillCancel_Click(object sender, MouseButtonEventArgs e)
        {
            foreach (var EachPlayerCard in Battlefield.PlayerList.Select(O => O.BattleData.PlayerCard))
            {
                EachPlayerCard.MouseEnter -= SkillTarget_MouseEnter;
                EachPlayerCard.MouseLeave -= SkillTarget_MouseLeave;
                EachPlayerCard.MouseLeftButtonUp -= SkillTarget_MouseClick;
                EachPlayerCard.Tag = null;
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
            this.MouseRightButtonUp -= SkillCancel_Click;
            App.CurrentUser.BattleData.Skill.CurrentTarget = null;
            App.CurrentUser.BattleData.Skill.TargetCollection.Clear();

            if (App.CurrentUser.BattleData.Skill.IsActive)
            {
                SetStatus("角色：" + App.CurrentUser.Name + "的回合", Colors.LimeGreen);
                OpenAttackOptions(true);
            }
            else
            {
                SetStatus(App.CurrentUser.Name + "正在做出防御选项...", Colors.OrangeRed);
                OpenDefendOptions(true);
            }
        }

        private void AttackAction(Player Attacker, Player Defender, Line ActionLine, bool Success)
        {
            if (Success)
            {
                SetStatus(Attacker.Name + "攻击成功！", Colors.LimeGreen);

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
                        Probability = new KeyValuePair<int, int>(
                            (int)Math.Round((double)Attacker.BattleData.AttackSuccessRatio.Tag), 6),
                        Type = DiceRoll.RollType.Attack,
                        Success = true,
                        Time = DateTime.Now,
                    });
                Attacker.BattleData.PlayerCard.UpdateLuck();

                DoneEventArgs AttackedEvent = new DoneEventArgs()
                {
                    Attacker = Attacker,
                    Defender = Defender,
                    DoneValue = (double)Attacker.BattleData.AttackDamage.Tag,
                    Success = true
                };
                Attacker.BattleData.OnAttacked(AttackedEvent);

                Attacker.BattleData.AttackSuccessRatio.UpdateVariable(
                    Guid.Parse("00000000-0000-0000-0000-000000000000"), 0);
                Attacker.BattleData.AttackDamage.Tag = AttackedEvent.DoneValue;
                Attacker.BattleData.Output += AttackedEvent.DoneValue;

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
                        Probability = new KeyValuePair<int, int>(
                            (int)Math.Round((double)Attacker.BattleData.AttackSuccessRatio.Tag), 6),
                        Type = DiceRoll.RollType.Attack,
                        Success = false,
                        Time = DateTime.Now,
                    }); 
                Attacker.BattleData.PlayerCard.UpdateLuck();
                Attacker.BattleData.AttackSuccessRatio.UpdateVariable(
                    Guid.Parse("00000000-0000-0000-0000-000000000000"), 0);

                DoneEventArgs AttackedEvent = new DoneEventArgs()
                {
                    Attacker = Attacker,
                    Defender = Defender,
                    DoneValue = (double)Attacker.BattleData.AttackDamage.Tag,
                    Success = false
                };
                Attacker.BattleData.OnAttacked(AttackedEvent);

                if (Attacker is AI && ((AI)Attacker).Inclination == AttackInclination.TargetHard)
                    ((AI)Attacker).PlayerDataCollection[Defender] -= 1;

                Attacker.BattleData.AttackDamage.Tag = null;
                Attacker.BattleData.AttackSuccessRatio.Tag = null;

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
                    if (Defender.BattleData.CurrentHp.GetInt() == 0)
                    {
                        Attacker.BattleData.Executed.Add(Defender);
                        Defender.BattleData.SetStatus("Nobody");
                        SetStatus(Defender.Name + "已被" + Attacker.Name + "击杀！", Colors.Gray);
                    }
                    else
                        Defender.BattleData.SetStatus("Joined");

                    if (Battlefield.IsGameOver())
                    {
                        var Group = Battlefield.GetWinnerGroup();
                        if (App.CurrentUser.BattleData == null)
                            GameOver();
                        else if (Group.Name == App.CurrentUser.BattleData.Group.Name)
                            Victory();
                        else
                            Defeated();
                    }
                    else
                        NextPlayer();

                    Timer.Stop();
                };

                Battlefield.Record.Participants[Defender.Id].Rolls.Add(
                    new DiceRoll()
                    {
                        Probability = new KeyValuePair<int, int>(
                            (int)Math.Round((double)Defender.BattleData.DefendSuccessRatio.Tag), 6),
                        Type = DiceRoll.RollType.Defense,
                        Success = true,
                        Time = DateTime.Now,
                    });
                Defender.BattleData.PlayerCard.UpdateLuck();

                DoneEventArgs DefendedEvent = new DoneEventArgs()
                {
                    Attacker = Attacker,
                    Defender = Defender,
                    DoneValue = (double)Defender.BattleData.DefenceCapacity.Tag,
                    Success = true
                };
                Attacker.BattleData.OnDefended(DefendedEvent);

                Defender.BattleData.CurrentHp.SetBasic(
                    Defender.BattleData.CurrentHp.GetDouble() -
                    (double)Attacker.BattleData.AttackDamage.Tag +
                    DefendedEvent.DoneValue);
                Defender.BattleData.Capacity += (double) Attacker.BattleData.AttackDamage.Tag;
                if (Defender is AI && ((AI) Defender).Inclination == AttackInclination.Vindictive)
                    ((AI) Defender).PlayerDataCollection[Attacker] += 1;
                if (Attacker is AI && ((AI)Attacker).Inclination == AttackInclination.TargetHard)
                    ((AI) Attacker).PlayerDataCollection[Defender] += 1;

                Defender.BattleData.DefendSuccessRatio.UpdateVariable(
                    Guid.Parse("00000000-0000-0000-0000-000000000000"), 0);
                Attacker.BattleData.AttackDamage.Tag = null;
                Attacker.BattleData.AttackSuccessRatio.Tag = null;
                Defender.BattleData.DefenceCapacity.Tag = null;
                Defender.BattleData.DefendSuccessRatio.Tag = null;

                Timer.Start();
            }
            else
            {
                SetStatus(Defender.Name + "防御失败。", Colors.Gray);

                DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                Timer.Tick += delegate
                {
                    if (Defender.BattleData.CurrentHp.GetInt() == 0)
                    {
                        Attacker.BattleData.Executed.Add(Defender);
                        Defender.BattleData.SetStatus("Nobody");
                        SetStatus(Defender.Name + "已被" + Attacker.Name + "击杀！", Colors.Gray);
                    }
                    else
                        Defender.BattleData.SetStatus("Joined");

                    if (Battlefield.IsGameOver())
                    {
                        var Group = Battlefield.GetWinnerGroup();
                        if (App.CurrentUser.BattleData == null)
                            GameOver();
                        else if (Group.Name == App.CurrentUser.BattleData.Group.Name)
                            Victory();
                        else
                            Defeated();
                    }
                    else
                        NextPlayer();

                    Timer.Stop();
                };

                Battlefield.Record.Participants[Defender.Id].Rolls.Add(
                    new DiceRoll()
                    {
                        Probability = new KeyValuePair<int, int>(Defender.BattleData.DefendSuccessRatio.GetInt(), 6),
                        Type = DiceRoll.RollType.Defense,
                        Success = false,
                        Time = DateTime.Now
                    });
                Defender.BattleData.PlayerCard.UpdateLuck();

                DoneEventArgs DefendedEvent = new DoneEventArgs()
                {
                    Attacker = Attacker,
                    Defender = Defender,
                    DoneValue = (double)Defender.BattleData.DefenceCapacity.Tag,
                    Success = false
                };
                Attacker.BattleData.OnDefended(DefendedEvent);

                Defender.BattleData.CurrentHp.SetBasic(
                    Defender.BattleData.CurrentHp.GetDouble() -
                    (double)Attacker.BattleData.AttackDamage.Tag);
                Defender.BattleData.Capacity += (double)Attacker.BattleData.AttackDamage.Tag;
                if (Defender is AI && ((AI)Defender).Inclination == AttackInclination.Vindictive)
                    ((AI)Defender).PlayerDataCollection[Attacker] += 2;
                if (Attacker is AI && ((AI)Attacker).Inclination == AttackInclination.TargetHard)
                    ((AI)Attacker).PlayerDataCollection[Defender] += 2;

                Defender.BattleData.DefendSuccessRatio.UpdateVariable(
                    Guid.Parse("00000000-0000-0000-0000-000000000000"), 0);
                Attacker.BattleData.AttackDamage.Tag = null;
                Attacker.BattleData.AttackSuccessRatio.Tag = null;
                Defender.BattleData.DefenceCapacity.Tag = null;
                Defender.BattleData.DefendSuccessRatio.Tag = null;

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

                    if (Defender.BattleData.CurrentHp.GetInt() == 0)
                    {
                        Attacker.BattleData.Executed.Add(Defender);
                        Defender.BattleData.SetStatus("Nobody");
                        SetStatus(Defender.Name + "已被" + Attacker.Name + "击杀！", Colors.Gray);
                    }
                    else
                        Defender.BattleData.SetStatus("Ready");

                    PathCanvas.Children.Remove(ActionLine);

                    if (Battlefield.IsGameOver())
                    {
                        var Group = Battlefield.GetWinnerGroup();
                        if (App.CurrentUser.BattleData == null)
                            GameOver();
                        else if (Group.Name == App.CurrentUser.BattleData.Group.Name)
                            Victory();
                        else
                            Defeated();
                    }
                    else
                        NextPlayer();

                    Timer.Stop();
                };

                Battlefield.Record.Participants[Attacker.Id].Rolls.Add(
                    new DiceRoll()
                    {
                        Probability = new KeyValuePair<int, int>
                            ((int)Math.Round((double)Attacker.BattleData.CounterSuccessRatio.Tag), 6),
                        Type = DiceRoll.RollType.Counter,
                        Success = true,
                        Time = DateTime.Now,
                    });
                Attacker.BattleData.PlayerCard.UpdateLuck();

                DoneEventArgs CounterAttackedEvent = new DoneEventArgs()
                {
                    Attacker = Attacker,
                    Defender = Defender,
                    DoneValue = (double)Attacker.BattleData.CounterDamage.Tag,
                    Success = true
                };
                Attacker.BattleData.OnCounterAttacked(CounterAttackedEvent);

                Defender.BattleData.CurrentHp.SetBasic(
                    Defender.BattleData.CurrentHp.GetDouble() -
                    CounterAttackedEvent.DoneValue);
                Attacker.BattleData.Output += CounterAttackedEvent.DoneValue;
                if (Defender is AI && ((AI) Defender).Inclination == AttackInclination.Vindictive)
                    ((AI) Defender).PlayerDataCollection[Attacker] += 2;
                if (Attacker is AI)
                {
                    if (((AI)Attacker).Inclination == AttackInclination.TargetHard)
                        ((AI) Attacker).PlayerDataCollection[Defender] += 2;
                    else if (((AI)Attacker).Inclination == AttackInclination.Vindictive)
                        ((AI) Attacker).PlayerDataCollection[Defender] += 1;
                }

                Attacker.BattleData.CounterSuccessRatio.Tag = null;
                Attacker.BattleData.CounterDamage.Tag = null;

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
                    if (Attacker.BattleData.CurrentHp.GetInt() == 0)
                    {
                        Defender.BattleData.Executed.Add(Attacker);
                        Attacker.BattleData.SetStatus("Nobody");
                        SetStatus(Attacker.Name + "已被" + Defender.Name + "击杀！", Colors.Gray);
                    }
                    else
                        Attacker.BattleData.SetStatus("Joined");

                    Defender.BattleData.SetStatus("Ready");
                    PathCanvas.Children.Remove(ActionLine);

                    if (Battlefield.IsGameOver())
                    {
                        var Group = Battlefield.GetWinnerGroup();
                        if (App.CurrentUser.BattleData == null)
                            GameOver();
                        else if (Group.Name == App.CurrentUser.BattleData.Group.Name)
                            Victory();
                        else
                            Defeated();
                    }
                    else
                        NextPlayer();

                    Timer.Stop();
                };

                Battlefield.Record.Participants[Attacker.Id].Rolls.Add(
                    new DiceRoll()
                    {
                        Probability = new KeyValuePair<int, int>
                            ((int)Math.Round((double)Attacker.BattleData.CounterSuccessRatio.Tag), 6),
                        Type = DiceRoll.RollType.Counter,
                        Success = false,
                        Time = DateTime.Now,
                    });
                Attacker.BattleData.PlayerCard.UpdateLuck();

                DoneEventArgs CounterAttackedEvent = new DoneEventArgs()
                {
                    Attacker = Attacker,
                    Defender = Defender,
                    DoneValue = Defender.BattleData.CounterPunishment.GetDouble(),
                    Success = false
                };
                Attacker.BattleData.OnCounterAttacked(CounterAttackedEvent);

                Attacker.BattleData.CurrentHp.SetBasic(
                    Attacker.BattleData.CurrentHp.GetDouble() -
                    CounterAttackedEvent.DoneValue);
                Defender.BattleData.Output += CounterAttackedEvent.DoneValue;
                if (Defender is AI && ((AI)Defender).Inclination == AttackInclination.TargetHard)
                    ((AI)Defender).PlayerDataCollection[Attacker] += 3;
                if (Attacker is AI && ((AI)Attacker).Inclination == AttackInclination.Vindictive)
                    ((AI)Attacker).PlayerDataCollection[Defender] += 3;

                Attacker.BattleData.CounterSuccessRatio.Tag = null;
                Attacker.BattleData.CounterDamage.Tag = null;

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
        private bool SkillAction_SelectTarget(Skill Skill)
        {
            if (Skill.Owner is AI)
            {
                Skill.TargetCollection =
                    ((AI) Skill.Owner).GetSkillTargets(Skill).ToList();
                return true;
            }

            Skill.NextTarget();
            if (Skill.CurrentTarget == null) return true;
            switch (Skill.CurrentTarget.Value.Key)
            {
                case SkillTarget.Self:
                    Skill.TargetCollection.Add(Skill.Owner);
                    break;
                case SkillTarget.Enemy:
                    SetStatus("选择一个目标敌人", Colors.Orange);
                    foreach (var PlayerCard in Battlefield.PlayerList.Select(O => O.BattleData.PlayerCard))
                    {
                        PlayerCard.MouseEnter += SkillTarget_MouseEnter;
                        PlayerCard.MouseLeave += SkillTarget_MouseLeave;
                        PlayerCard.MouseLeftButtonUp += SkillTarget_MouseClick;
                        PlayerCard.Tag = new Func<Player, bool>(
                            Self => Self.BattleData.Group.Name != App.CurrentUser.BattleData.Group.Name
                                    && Self.BattleData.CurrentHp.GetDouble() > 0);
                    }
                    this.MouseRightButtonUp += SkillCancel_Click;
                    break;
                case SkillTarget.Ally:
                    SetStatus("选择一个目标盟友", Colors.Orange);
                    foreach (var PlayerCard in Battlefield.PlayerList.Select(O => O.BattleData.PlayerCard))
                    {
                        PlayerCard.MouseEnter += SkillTarget_MouseEnter;
                        PlayerCard.MouseLeave += SkillTarget_MouseLeave;
                        PlayerCard.MouseLeftButtonUp += SkillTarget_MouseClick;
                        PlayerCard.Tag = new Func<Player, bool>(
                            Self => Self.BattleData.Group.Name == App.CurrentUser.BattleData.Group.Name
                                    && Self.BattleData.CurrentHp.GetDouble() > 0);
                    }
                    this.MouseRightButtonUp += SkillCancel_Click;
                    break;
                case SkillTarget.AllyWithoutSelf:
                    SetStatus("选择一个除自己外的目标盟友", Colors.Orange);
                    foreach (var PlayerCard in Battlefield.PlayerList.Select(O => O.BattleData.PlayerCard))
                    {
                        PlayerCard.MouseEnter += SkillTarget_MouseEnter;
                        PlayerCard.MouseLeave += SkillTarget_MouseLeave;
                        PlayerCard.MouseLeftButtonUp += SkillTarget_MouseClick;
                        PlayerCard.Tag = new Func<Player, bool>(
                            Self => Self.Id != App.CurrentUser.Id &&
                                    Self.BattleData.Group.Name == App.CurrentUser.BattleData.Group.Name
                                    && Self.BattleData.CurrentHp.GetDouble() > 0);
                    }
                    this.MouseRightButtonUp += SkillCancel_Click;
                    break;
                case SkillTarget.SelfGroup:
                    Skill.TargetCollection.AddRange(Skill.Owner.BattleData.Battlefield.PlayerList
                        .Where(O => O.BattleData.Group.Name == Skill.Owner.BattleData.Group.Name));
                    break;
            }
            return false;
        }
        private void SkillAction_Prepare(Skill Skill)
        {
            SetStatus(Skill.Owner.Name + "正在执行技能：" + Skill.Name + "...", Colors.OrangeRed);
            Skill.Owner.BattleData.PlayerCard.SetAction(Skill.Name, Skill.Image, Skill.Description, Skill.Probability);

            List<Line> ActionLines = new List<Line>();

            DispatcherTimer Timer = new DispatcherTimer()
            { Interval = TimeSpan.FromSeconds(1.6) };
            Timer.Tick += delegate
            {
                DiceControl.Expected = 6 - Skill.Probability;
                DiceControl.EndAction = delegate
                { SkillAction_Activate(Skill, ActionLines, DiceControl.Success); };
                DiceControl.Expand();
                Timer.Stop();
            };

            var SelfCard = Skill.Owner.BattleData.PlayerCard;
            foreach (var Target in Skill.TargetCollection)
            {
                var TargetCard = Target.BattleData.PlayerCard;
                ActionLineAnimation(
                       new Point(Canvas.GetLeft((StackPanel)SelfCard.Parent) + SelfCard.ActualWidth / 2,
                           Canvas.GetTop((StackPanel)SelfCard.Parent) + ((StackPanel)SelfCard.Parent).Children.IndexOf(SelfCard) * 100 + SelfCard.ActualHeight / 2),
                       new Point(Canvas.GetLeft((StackPanel)TargetCard.Parent) + TargetCard.ActualWidth / 2,
                           Canvas.GetTop((StackPanel)TargetCard.Parent) + ((StackPanel)TargetCard.Parent).Children.IndexOf(TargetCard) * 100 + TargetCard.ActualHeight / 2),
                       Colors.Orange, delegate(Line Line) { ActionLines.Add(Line); });
            }
            Timer.Start();
        }
        private void SkillAction_Activate(Skill Skill, List<Line> ActionLines , bool Success)
        {
            if (Success)
            {
                SetStatus(Skill.Owner.Name + "发动技能成功！", Colors.LimeGreen);

                DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.5) };
                Timer.Tick += delegate
                {
                    ActionLines.ForEach(O => PathCanvas.Children.Remove(O));
                    if (Battlefield.IsGameOver())
                    {
                        var Group = Battlefield.GetWinnerGroup();
                        if (Group.Name == App.CurrentUser.BattleData.Group.Name)
                            Victory();
                        else
                            Defeated();
                    }
                    else
                        NextPlayer();
                    Timer.Stop();
                };

                Skill.Launch();

                Battlefield.Record.Participants[Skill.Owner.Id].Rolls.Add(
                    new DiceRoll()
                    {
                        Probability = new KeyValuePair<int, int>(Skill.Probability, 6),
                        Type = DiceRoll.RollType.UseSkill,
                        SkillId = Skill.Id,
                        Success = true,
                        Time = DateTime.Now,
                    });
                Skill.Owner.BattleData.PlayerCard.UpdateLuck();

                foreach (var ActionLine in ActionLines)
                {
                    ActionLine.BeginAnimation(Line.X1Property, new DoubleAnimation()
                    {
                        From = ActionLine.X1,
                        To = ActionLine.X2,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseIn}
                    });
                    ActionLine.BeginAnimation(Line.Y1Property, new DoubleAnimation()
                    {
                        From = ActionLine.Y1,
                        To = ActionLine.Y2,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseIn}
                    });
                    ActionLine.BeginAnimation(Line.StrokeThicknessProperty, new DoubleAnimation()
                    {
                        From = 5,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.5),
                    });
                }
                Timer.Start();
            }
            else
            {
                SetStatus(Skill.Owner.Name + "发动技能失败。", Colors.Gray);

                DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                Timer.Tick += delegate
                {
                    ActionLines.ForEach(O => PathCanvas.Children.Remove(O));
                    NextPlayer();
                    Timer.Stop();
                };

                Battlefield.Record.Participants[Skill.Owner.Id].Rolls.Add(
                    new DiceRoll()
                    {
                        Probability = new KeyValuePair<int, int>(Skill.Probability, 6),
                        Type = DiceRoll.RollType.UseSkill,
                        SkillId = Skill.Id,
                        Success = false,
                        Time = DateTime.Now,
                    });
                Skill.Owner.BattleData.PlayerCard.UpdateLuck();

                foreach (var ActionLine in ActionLines)
                {
                    ActionLine.BeginAnimation(Line.X2Property, new DoubleAnimation()
                    {
                        From = ActionLine.X2,
                        To = ActionLine.X1,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseIn}
                    });
                    ActionLine.BeginAnimation(Line.Y2Property, new DoubleAnimation()
                    {
                        From = ActionLine.Y2,
                        To = ActionLine.Y1,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseIn}
                    });
                    ActionLine.BeginAnimation(Line.StrokeThicknessProperty, new DoubleAnimation()
                    {
                        From = 5,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.5),
                    });
                }
                Timer.Start();
            }
        }

        private void Victory()
        {
            App.CurrentUser.BattleData.Group.IsVictorious = true;
            CenterBorderCommand(new List<KeyValuePair<string, Action>>()
            {
                new KeyValuePair<string, Action>("ShowCenterBorder", delegate
                {
                    CenterBorder.Background = CenterBorder.Background.Clone();
                    ((SolidColorBrush)CenterBorder.Background).BeginAnimation(SolidColorBrush.ColorProperty,
                        new ColorAnimation()
                        {
                            From = ((SolidColorBrush)CenterBorder.Background).Color,
                            To = Color.FromArgb(138, 0, 100, 0),
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                        });
                }),
                new KeyValuePair<string, Action>("ShowCenterText;You are Victorious!", null),
                new KeyValuePair<string, Action>("Wait;2", null),
                new KeyValuePair<string, Action>("HideCenterText", null),
                new KeyValuePair<string, Action>("HideCenterBorder", delegate
                {
                    LeaveAction = delegate
                    {
                        MainWindow.Instance.EnterStatistics(Battlefield);
                    };
                    PageLeave();
                })
            });
        }
        private void Defeated()
        {
            Battlefield.GetWinnerGroup().IsVictorious = true;
            CenterBorderCommand(new List<KeyValuePair<string, Action>>()
            {
                new KeyValuePair<string, Action>("ShowCenterBorder", delegate
                {
                    CenterBorder.Background = CenterBorder.Background.Clone();
                    ((SolidColorBrush)CenterBorder.Background).BeginAnimation(SolidColorBrush.ColorProperty,
                        new ColorAnimation()
                        {
                            From = ((SolidColorBrush)CenterBorder.Background).Color,
                            To = Color.FromArgb(138, 100, 0, 0),
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                        });
                }),
                new KeyValuePair<string, Action>("ShowCenterText;You are Defeated", null),
                new KeyValuePair<string, Action>("Wait;2", null),
                new KeyValuePair<string, Action>("HideCenterText", null),
                new KeyValuePair<string, Action>("HideCenterBorder", delegate
                {
                    LeaveAction = delegate
                    {
                        MainWindow.Instance.EnterStatistics(Battlefield);
                    };
                    PageLeave();
                })
            });
        }
        private void GameOver()
        {
            Battlefield.GetWinnerGroup().IsVictorious = true;
            CenterBorderCommand(new List<KeyValuePair<string, Action>>()
            {
                new KeyValuePair<string, Action>("ShowCenterBorder", delegate
                {
                    CenterBorder.Background = CenterBorder.Background.Clone();
                    ((SolidColorBrush)CenterBorder.Background).BeginAnimation(SolidColorBrush.ColorProperty,
                        new ColorAnimation()
                        {
                            From = ((SolidColorBrush)CenterBorder.Background).Color,
                            To = Color.FromArgb(138, 100, 100, 0),
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                        });
                }),
                new KeyValuePair<string, Action>("ShowCenterText;Game is Over", null),
                new KeyValuePair<string, Action>("Wait;2", null),
                new KeyValuePair<string, Action>("HideCenterText", null),
                new KeyValuePair<string, Action>("HideCenterBorder", delegate
                {
                    LeaveAction = delegate
                    {
                        MainWindow.Instance.EnterStatistics(Battlefield);
                    };
                    PageLeave();
                })
            });
        }

        private void Close_Click(object Sender, RoutedEventArgs E)
        {
            
        }
    }
}
