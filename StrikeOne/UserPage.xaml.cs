using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace StrikeOne
{
    /// <summary>
    /// UserPage.xaml 的交互逻辑
    /// </summary>
    public partial class UserPage : UserControl
    {
        public Action LeaveAction { set; private get; }
        private List<FrameworkElement> MenuList { set; get; }

        public UserPage()
        {
            InitializeComponent();
            MenuList = new List<FrameworkElement>()
            {
                LocalStrike,
                CreateStrikeRoom,
                JoinStrikeRoom,
                QuitStrike
            };

            foreach (var MenuButton in MenuList)
            {
                MenuButton.MouseEnter += (Sender, E) =>
                {
                    ((DropShadowEffect)MenuButton.Effect).BeginAnimation(OpacityProperty, new DoubleAnimation()
                    {
                        From = ((DropShadowEffect)MenuButton.Effect).Opacity,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.25),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    });
                    SliderRect.BeginAnimation(OpacityProperty, new DoubleAnimation()
                    {
                        From = SliderRect.Opacity,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.25),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    });
                    SliderRect.BeginAnimation(MarginProperty, new ThicknessAnimation()
                    {
                        From = SliderRect.Margin,
                        To = MenuButton.Margin,
                        Duration = TimeSpan.FromSeconds(0.25),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    });
                };
                MenuButton.MouseLeave += (Sender, E) =>
                {
                    ((DropShadowEffect)MenuButton.Effect).BeginAnimation(OpacityProperty, new DoubleAnimation()
                    {
                        From = ((DropShadowEffect)MenuButton.Effect).Opacity,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.25),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                    });
                    SliderRect.BeginAnimation(OpacityProperty, new DoubleAnimation()
                    {
                        From = SliderRect.Opacity,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.25),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                    });
                };
            }

            this.Opacity = 0;
            MenuList.ForEach(O => O.Opacity = 0);
        }

        public void PageEnter(bool Login = false)
        {
            BgiRotateTransform.BeginAnimation(RotateTransform.AngleProperty, GenerateBgiRotateAnimation());
            if (Login)
            {
                MainWindow.Instance.ShowInfoGrid(true);
                DispatcherTimer DelayTimer = new DispatcherTimer() {Interval = TimeSpan.FromSeconds(0.5)};
                DelayTimer.Tick += delegate
                {
                    DoubleAnimation OpacityAnimation = new DoubleAnimation()
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(1),
                        EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut}
                    };
                    OpacityAnimation.Completed += delegate
                    {
                        var RingAnimation = new ThicknessAnimation()
                        {
                            From = OrnamentRing.Margin,
                            To = new Thickness(275, 0, 0, 0),
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut}
                        };
                        RingAnimation.Completed += delegate
                        {
                            DispatcherTimer MenuTimer = new DispatcherTimer() {Interval = TimeSpan.FromSeconds(0.25)};
                            int Count = 0;
                            MenuTimer.Tick += delegate
                            {
                                MenuList[Count].BeginAnimation(OpacityProperty, new DoubleAnimation()
                                {
                                    From = 0,
                                    To = 1,
                                    Duration = TimeSpan.FromSeconds(0.5),
                                    EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut}
                                });
                                MenuList[Count].BeginAnimation(MarginProperty, new ThicknessAnimation()
                                {
                                    From = new Thickness(
                                        MenuList[Count].Margin.Left - 50,
                                        MenuList[Count].Margin.Top,
                                        MenuList[Count].Margin.Right,
                                        MenuList[Count].Margin.Bottom),
                                    To = MenuList[Count].Margin,
                                    Duration = TimeSpan.FromSeconds(0.5),
                                    EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut}
                                });
                                Count++;
                                if (Count >= MenuList.Count)
                                    MenuTimer.Stop();
                            };
                            MenuTimer.Start();
                        };
                        OrnamentRing.BeginAnimation(MarginProperty, RingAnimation);
                    };
                    this.BeginAnimation(OpacityProperty, OpacityAnimation);
                    DelayTimer.Stop();
                };
                DelayTimer.Start();
            }
            else
            {
                MainWindow.Instance.InfoGrid.Init();

                DoubleAnimation OpacityAnimation = new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                };
                OpacityAnimation.Completed += delegate
                {
                    var RingAnimation = new ThicknessAnimation()
                    {
                        From = OrnamentRing.Margin,
                        To = new Thickness(275, 0, 0, 0),
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    };
                    RingAnimation.Completed += delegate
                    {
                        DispatcherTimer MenuTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.25) };
                        int Count = 0;
                        MenuTimer.Tick += delegate
                        {
                            MenuList[Count].BeginAnimation(OpacityProperty, new DoubleAnimation()
                            {
                                From = 0,
                                To = 1,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                            });
                            MenuList[Count].BeginAnimation(MarginProperty, new ThicknessAnimation()
                            {
                                From = new Thickness(
                                    MenuList[Count].Margin.Left - 50,
                                    MenuList[Count].Margin.Top,
                                    MenuList[Count].Margin.Right,
                                    MenuList[Count].Margin.Bottom),
                                To = MenuList[Count].Margin,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                            });
                            Count++;
                            if (Count >= MenuList.Count)
                                MenuTimer.Stop();
                        };
                        MenuTimer.Start();
                    };
                    OrnamentRing.BeginAnimation(MarginProperty, RingAnimation);
                };
                this.BeginAnimation(OpacityProperty, OpacityAnimation);
            }

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
            OpacityAnimation.Completed += delegate
            {
                LeaveAction?.Invoke();
            };
            this.BeginAnimation(OpacityProperty, OpacityAnimation);
        }

        public DoubleAnimation GenerateBgiRotateAnimation()
        {
            DoubleAnimation Animation = new DoubleAnimation()
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(30)
            };
            Animation.Completed += delegate
            {
                BgiRotateTransform.BeginAnimation(RotateTransform.AngleProperty, GenerateBgiRotateAnimation());
            };
            return Animation;
        }

        private void Quit_Click(object Sender, MouseButtonEventArgs E)
        {
            if (MessageBox.Show("确实要退出当前角色账号么？", "退出账号",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
            LeaveAction = delegate
            {
                MainWindow.Instance.HideInfoGrid();
                MainWindow.Instance.Login();
            };
           
            PageLeave();
        }
    }
}
