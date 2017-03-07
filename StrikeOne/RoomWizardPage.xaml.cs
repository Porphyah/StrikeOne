using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using StrikeOne.Core;
using StrikeOne.Core.Network;

namespace StrikeOne
{
    /// <summary>
    /// RoomWizardPage.xaml 的交互逻辑
    /// </summary>
    public partial class RoomWizardPage : UserControl
    {
        private static readonly Dictionary<string, BattleType> BattleTypeDictionary = new Dictionary<string, BattleType>()
        {
            { "One vs One", BattleType.OneVsOne }
        }; 

        public Action LeaveAction { set; private get; }
        private DispatcherTimer StatusTimer { set; get; }

        public RoomWizardPage()
        {
            InitializeComponent();
            BattleTypeBox.ItemsSource = BattleTypeDictionary.Keys;

            TitleGrid.Opacity = 0;
            ContentGrid.Opacity = 0;
        }

        public void PageEnter()
        {
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

        private void RoomGenerate_Click(object Sender, RoutedEventArgs E)
        {
            if (string.IsNullOrWhiteSpace(RoomNameBox.Text))
            {
                SetStatus("房间名称不能为空。", Colors.OrangeRed);
                return;
            }
            if ((string)BattleTypeBox.SelectedItem == null)
            {
                SetStatus("必须要选择一种对战类型。", Colors.OrangeRed);
                return;
            }

            Thread RoomGenerateThread = new Thread(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    SetStatus("生成房间信息...", Colors.Orange);
                });
                Room NewRoom = new Room();
                this.Dispatcher.Invoke(() =>
                {
                    NewRoom = new Room()
                    {
                        Id = Guid.NewGuid(),
                        Name = RoomNameBox.Text,
                        Host = App.CurrentUser,
                        BattleType = BattleTypeDictionary[(string)BattleTypeBox.SelectedItem],
                        Description = DescriptionBox.Text,
                        LAN = NetworkCheckBox.IsChecked.Value
                    };
                    NewRoom.Members.Add(App.CurrentUser);
                    switch (NewRoom.BattleType)
                    {
                        case BattleType.OneVsOne:
                            NewRoom.Groups.Add(new Group()
                            {
                                Name = "Group A",
                                Color = Colors.DodgerBlue,
                                Capacity = 1,
                                Room = NewRoom
                            });
                            NewRoom.Groups.Add(new Group()
                            {
                                Name = "Group B",
                                Color = Colors.Red,
                                Capacity = 1,
                                Room = NewRoom
                            });
                            break;
                    }
                });

                App.CurrentRoom = NewRoom;

                this.Dispatcher.Invoke(() =>
                {
                    SetStatus("建立房间服务器...", Colors.Orange);
                });
                App.Server = new TcpListener()
                {
                    Lan = NewRoom.LAN,
                    Port = App.Port
                };
                try { App.Server.Start(); }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke(() =>
                      SetStatus("服务器建立失败：" + ex.Message, 
                      Colors.Red));
                    return;
                }

                this.Dispatcher.Invoke(() =>
                    SetStatus("生成成功！", Colors.LimeGreen));
                this.Dispatcher.Invoke(() =>
                {
                    LeaveAction = delegate { MainWindow.Instance.EnterRoomPage(App.CurrentRoom); };
                    PageLeave();
                });
            })
            { Name = "生成房间线程" };
            RoomGenerateThread.Start();
        }
        private void Return_Click(object Sender, RoutedEventArgs E)
        {
            LeaveAction = delegate
            {
                MainWindow.Instance.EnterUserPage(false);
            };
            this.PageLeave();
        }

        private void SetStatus(string Text, Color Color)
        {
            if (StatusTimer != null && StatusTimer.IsEnabled)
            {
                StatusTimer.Stop();

                StatusText.Text = Text;
                StatusBar.Background = new SolidColorBrush(Color);

                StatusTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(5) };
                StatusTimer.Tick += delegate
                {
                    StatusBar.BeginAnimation(MarginProperty, new ThicknessAnimation()
                    {
                        From = StatusBar.Margin,
                        To = new Thickness(0, 0, 0, -30),
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                    });
                    StatusTimer.Stop();
                };
                StatusTimer.Start();
            }
            else
            {
                StatusText.Text = Text;
                StatusBar.Background = new SolidColorBrush(Color);

                StatusTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(5) };
                StatusTimer.Tick += delegate
                {
                    StatusBar.BeginAnimation(MarginProperty, new ThicknessAnimation()
                    {
                        From = StatusBar.Margin,
                        To = new Thickness(0, 0, 0, -30),
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                    });
                    StatusTimer.Stop();
                };

                var Animation = new ThicknessAnimation()
                {
                    From = StatusBar.Margin,
                    To = new Thickness(0, 0, 0, 0),
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                };
                Animation.Completed += delegate { StatusTimer.Start(); };
                StatusBar.BeginAnimation(MarginProperty, Animation);
            }

        }
    }
}
