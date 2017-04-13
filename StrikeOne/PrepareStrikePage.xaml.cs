using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using StrikeOne.Components;
using StrikeOne.Core;

namespace StrikeOne
{
    /// <summary>
    /// PrepareStrikePage.xaml 的交互逻辑
    /// </summary>
    public partial class PrepareStrikePage : UserControl
    {
        private static readonly Dictionary<BattleType, string> BattleTypeDictionary = new Dictionary<BattleType, string>()
        {
            { BattleType.OneVsOne, "One vs One" },
            { BattleType.TriangleMess, "Triangle Mess" },
            { BattleType.SquareMess, "Square Mess" },
            { BattleType.TwinningFight, "Twinning Fight" }
        };
        public PrepareStrikePage()
        {
            InitializeComponent();
        }
        public Action LeaveAction { set; private get; }
        public Room CurrentRoom { private set; get; }

        public void PageEnter(Room Room)
        {
            CurrentRoom = Room;

            TypeName.Text = BattleTypeDictionary[Room.BattleType];
            for (int i = 0; i < Room.Groups.Count; i++)
            {
                var Panel = new WrapPanel()
                { Height = 130, Width = Room.Groups[i].Capacity * 150 };
                foreach (var Participant in Room.Groups[i].Participants)
                {
                    var DisplayItem = new ParticipantDisplayLarge()
                    { Height = 130, Width = 150 };
                    if (Participant.Avator != null)
                        using (MemoryStream Stream = new MemoryStream())
                        {
                            Participant.Avator.Save(Stream, ImageFormat.Png);
                            BitmapImage Temp = new BitmapImage();
                            Temp.BeginInit();
                            Temp.CacheOption = BitmapCacheOption.OnLoad;
                            Temp.StreamSource = Stream;
                            Temp.EndInit();
                            DisplayItem.AvatorImage.ImageSource = Temp;
                        }
                    DisplayItem.PlayerName.Text = Participant.Name;
                    DisplayItem.ShadowEffect.Color = Room.Groups[i].Color;
                    Panel.Children.Add(DisplayItem);
                }
                DisplayGrid.Children.Add(Panel);
                SetItemPosition(Panel, Room.Groups.Count, i);
            }

            var OpacityAnimation = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            };
            OpacityAnimation.Completed += delegate
            {
                DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.25) };
                int TickCount = 0;
                Timer.Tick += delegate
                {
                    switch (TickCount)
                    {
                        case 0:
                            Ring1.Visibility = Visibility.Visible;
                            Ring1.BeginAnimation(OpacityProperty, new DoubleAnimation()
                            {
                                From = 0,
                                To = 1,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                            });
                            Ring1.BeginAnimation(Canvas.LeftProperty, new DoubleAnimation()
                            {
                                From = 0,
                                To = 235,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                            });
                            Ring1.BeginAnimation(Canvas.TopProperty, new DoubleAnimation()
                            {
                                From = -200,
                                To = 35,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                            });

                            Ring2.Visibility = Visibility.Visible;
                            Ring2.BeginAnimation(OpacityProperty, new DoubleAnimation()
                            {
                                From = 0,
                                To = 1,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                            });
                            Ring2.BeginAnimation(Canvas.RightProperty, new DoubleAnimation()
                            {
                                From = 0,
                                To = 235,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                            });
                            Ring2.BeginAnimation(Canvas.BottomProperty, new DoubleAnimation()
                            {
                                From = -200,
                                To = 35,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                            });
                            break;
                        case 1:
                            Arc1.Visibility = Visibility.Visible;
                            Arc1.BeginAnimation(OpacityProperty, new DoubleAnimation()
                            {
                                From = 0,
                                To = 1,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                            });
                            Arc1.BeginAnimation(Canvas.RightProperty, new DoubleAnimation()
                            {
                                From = 0,
                                To = 235,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                            });
                            Arc1.BeginAnimation(Canvas.TopProperty, new DoubleAnimation()
                            {
                                From = -200,
                                To = 35,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                            });

                            Arc2.Visibility = Visibility.Visible;
                            Arc2.BeginAnimation(OpacityProperty, new DoubleAnimation()
                            {
                                From = 0,
                                To = 1,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                            });
                            Arc2.BeginAnimation(Canvas.LeftProperty, new DoubleAnimation()
                            {
                                From = 0,
                                To = 235,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                            });
                            Arc2.BeginAnimation(Canvas.BottomProperty, new DoubleAnimation()
                            {
                                From = -200,
                                To = 35,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                            });
                            break;
                        case 2:
                            RotateTransform.BeginAnimation(RotateTransform.AngleProperty, GenerateRotateAnimation());
                            InitalizeBattlefield();
                            Timer.Stop();
                            break;
                    }
                    TickCount++;
                };
                Timer.Start();
            };
            this.BeginAnimation(OpacityProperty, OpacityAnimation);
        }

        public void PageLeave()
        {
            DoubleAnimation OpacityAnimation = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            };
            OpacityAnimation.Completed += delegate { LeaveAction?.Invoke(); };
            this.BeginAnimation(OpacityProperty, OpacityAnimation);
        }

        private void SetItemPosition(FrameworkElement Item, int GroupCount, int GroupIndex)
        {
            switch (GroupCount)
            {
                case 2:
                    switch (GroupIndex)
                    {
                        case 0: SetItemPosition(Item, 90); break;
                        case 1: SetItemPosition(Item, 270); break;
                    }
                    break;
                case 3:
                    switch (GroupIndex)
                    {
                        case 0: SetItemPosition(Item, 60); break;
                        case 1: SetItemPosition(Item, 180); break;
                        case 2: SetItemPosition(Item, 300); break;
                    }
                    break;
                case 4:
                    switch (GroupIndex)
                    {
                        case 0: SetItemPosition(Item, 60); break;
                        case 1: SetItemPosition(Item, 120); break;
                        case 2: SetItemPosition(Item, 240); break;
                        case 3: SetItemPosition(Item, 300); break;
                    }
                    break;
            }
        }
        private void SetItemPosition(FrameworkElement Item, double Angle)
        {
            Item.HorizontalAlignment = HorizontalAlignment.Center;
            Item.VerticalAlignment = VerticalAlignment.Center;
            if (Angle >= 0 && Angle < 90)
                Item.Margin = new Thickness(
                        -500 * Math.Sin(Angle / 180 * Math.PI),
                        -500 * Math.Cos(Angle / 180 * Math.PI),
                        0, 0
                    );
            else if (Angle >= 90 && Angle < 180)
                Item.Margin = new Thickness(
                        -500 * Math.Sin(Angle / 180 * Math.PI), 0,
                        0, 500 * Math.Cos(Angle / 180 * Math.PI)
                    );
            else if (Angle >= 180 && Angle < 270)
                Item.Margin = new Thickness(0, 0, 
                        500 * Math.Sin(Angle / 180 * Math.PI),
                        500 * Math.Cos(Angle / 180 * Math.PI)
                    );
            else if (Angle >= 270 && Angle < 360)
                Item.Margin = new Thickness(
                        0, -500 * Math.Cos(Angle / 180 * Math.PI),
                        500 * Math.Sin(Angle / 180 * Math.PI), 0
                    );
        }

        private DoubleAnimation GenerateRotateAnimation()
        {
            DoubleAnimation Animation = new DoubleAnimation()
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(30)
            };
            Animation.Completed += delegate
            {
                RotateTransform.BeginAnimation(RotateTransform.AngleProperty, GenerateRotateAnimation());
            };
            return Animation;
        }


        private void InitalizeBattlefield()
        {
            Thread InitThread = new Thread(() =>
            {
                CurrentRoom.Battlefield = new Battlefield();
                CurrentRoom.Battlefield.Init(CurrentRoom);
                this.Dispatcher.Invoke(() =>
                {
                    var ProgressAnimation = new DoubleAnimation()
                    {
                        From = 0,
                        To = 100,
                        Duration = TimeSpan.FromSeconds(2),
                        EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseInOut}
                    };
                    ProgressAnimation.Completed += delegate
                    {
                        LeaveAction = delegate { MainWindow.Instance.EnterBattlefield(CurrentRoom.Battlefield); };
                        this.PageLeave();
                    };
                    Progress.BeginAnimation(RangeBase.ValueProperty, ProgressAnimation);
                });
            }) { Name = "初始化战场进程" };
            InitThread.Start();
        }
    }
}
