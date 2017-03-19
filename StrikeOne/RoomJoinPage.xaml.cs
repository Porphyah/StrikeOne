using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
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
using StrikeOne.Core.Network;

namespace StrikeOne
{
    /// <summary>
    /// RoomJoinPage.xaml 的交互逻辑
    /// </summary>
    public partial class RoomJoinPage : UserControl
    {
        private static readonly Dictionary<BattleType, string> BattleTypeDictionary = new Dictionary<BattleType, string>()
        {
            { BattleType.OneVsOne, "One vs One" }
        };

        private Room CurrentRoom { set; get; }
        private string RoomName { set; get; }
        private string RoomHost { set; get; }
        public Action LeaveAction { set; private get; }
        private DispatcherTimer StatusTimer { set; get; }
        private bool Linking { set; get; } = false;

        public RoomJoinPage()
        {
            InitializeComponent();

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

        private void Link_Click(object Sender, RoutedEventArgs E)
        {
            if (Linking) return;
            Linking = true;
            if (string.IsNullOrWhiteSpace(InvitationBox.Text))
            {
                SetStatus("邀请码不可为空。", Colors.OrangeRed);
                Linking = false;
                return;
            }

            if (RoomInfoBox.Visibility == Visibility.Hidden)
            {
                RoomInfoBox.Visibility = Visibility.Visible;
                RoomInfoBox.BeginAnimation(OpacityProperty, new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.75),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });
                RoomInfoBox.BeginAnimation(MarginProperty, new ThicknessAnimation()
                {
                    From = new Thickness(
                        RoomInfoBox.Margin.Left - 50,
                        RoomInfoBox.Margin.Top,
                        RoomInfoBox.Margin.Right + 50,
                        RoomInfoBox.Margin.Bottom),
                    To = RoomInfoBox.Margin,
                    Duration = TimeSpan.FromSeconds(0.75),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });
            }

            SetRoomInfoBox("链接房间中...", Colors.Orange);
            ResetInProgress();
            if (RoomInfoGrid.Visibility == Visibility.Visible)
            {
                DoubleAnimation RoomInfoGridOpacityAnimation = new DoubleAnimation()
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                };
                RoomInfoGridOpacityAnimation.Completed += delegate
                {
                    RoomInfoGrid.Visibility = Visibility.Hidden;
                    InProgressGrid.Visibility = Visibility.Visible;
                    InProgressGrid.BeginAnimation(OpacityProperty, new DoubleAnimation()
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    });
                };
                RoomInfoGrid.BeginAnimation(OpacityProperty, RoomInfoGridOpacityAnimation);
            }

            Thread ConnectionThread = new Thread(() =>
            {
                string Text = "";
                this.Dispatcher.Invoke(() =>
                {
                    Text = InvitationBox.Text; 
                    SetStatus("正在解析邀请码...", Colors.Orange, false);
                });
                try
                {
                    if (Text.Length == 1)
                        throw new FormatException("邀请码长度有误。");
                    Text = Encryption.Decrypt(Text);
                }
                catch (Exception Ex1)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        SetRoomInfoBox("房间链接失败！", Colors.Red);
                        SetStatus("邀请码解码失败。", Colors.Red);
                        SetFailedText("无法解析的邀请码格式。\n" + Ex1.Message);
                    });
                    Linking = false;
                    return;
                }
                
                string[] Fragments = Text.Split(new[] {';'},
                    StringSplitOptions.RemoveEmptyEntries);
                if (Fragments.Length != 4)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        SetRoomInfoBox("房间链接失败！", Colors.Red);
                        SetStatus("邀请码解码失败。", Colors.Red);
                        SetFailedText("邀请码长度有误，信息缺失。");
                    });
                    Linking = false;
                    return;
                }

                IPAddress Address;
                try
                {
                    Address = IPAddress.Parse(Fragments[0]);
                }
                catch (Exception Ex2)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        SetRoomInfoBox("房间链接失败！", Colors.Red);
                        SetStatus("IP地址解析失败。", Colors.Red);
                        SetFailedText(Ex2.Message);
                    });
                    Linking = false;
                    return;
                }
                RoomName = Fragments[2];
                RoomHost = Fragments[3];

                this.Dispatcher.Invoke(() =>
                {
                    SetStatus("已确定IP地址，正在链接" + RoomHost + "的房间："
                        + RoomName + "...", Colors.Orange, false);
                });
                App.Client = new TcpClient();
                App.Client.ConnectFailed += ServerConnectFailed;
                App.Client.ConnectCompleted += ServerConnectCompleted;
                App.Client.ConnectAsync(new IPEndPoint(Address, App.Port));

            }) { Name = "房间链接线程" };
            ConnectionThread.Start();
        }
        private void Return_Click(object Sender, RoutedEventArgs E)
        {
            LeaveAction = delegate
            {
                MainWindow.Instance.EnterUserPage(false);
            };
            this.PageLeave();
        }
        private void Join_Click(object Sender, RoutedEventArgs E)
        {
            if (CurrentRoom == null)
            {
                SetStatus("您当前并未链接到任何房间。", Colors.OrangeRed);
                return;
            }

            ReturnButton.IsEnabled = false;
            JoinButton.IsEnabled = false;
            LinkButton.IsEnabled = false;
            InvitationBox.IsEnabled = false;
            JoinButton.Foreground = JoinButton.Foreground.Clone();
            ((SolidColorBrush)JoinButton.Foreground).BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation()
            {
                From = ((SolidColorBrush)JoinButton.Foreground).Color,
                To = Color.FromArgb(255, 173, 173, 173),
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            });

            //Thread JoinThread = new Thread(() =>
            //{
            this.Dispatcher.Invoke(() =>
            {
                SetStatus("正在等待" + CurrentRoom.Host + "房间：" + CurrentRoom.Name
                    + "的响应...", Colors.Orange);
            });
            App.Client.ReceiveCompleted += JoinConfirmed;
            App.Client.Send(Encoding.UTF8.GetBytes("LetMeIn|*|" + App.CurrentUser.Name));
                
            //}) { Name = "加入房间线程" };
            //JoinThread.Start();
        }
        private void JoinConfirmed(object Sender, SocketEventArgs E)
        {
            App.Client.Send(App.CurrentUser);
            App.CurrentRoom = CurrentRoom;
            App.Client.ReceiveCompleted -= JoinConfirmed;
            App.Client["ReadyToReceive"] = false;
            App.Client.ReceiveCompleted += ReadyToUpdataRoom;
            this.Dispatcher.Invoke(() =>
            {
                SetStatus("身份已确认，正在加入...", Colors.LimeGreen);
                LeaveAction = delegate { MainWindow.Instance.EnterRoomPage(CurrentRoom); };
                PageLeave();
            });
        }
        private void ReadyToUpdataRoom(object Sender, SocketEventArgs E)
        {
            App.Client["ReadyToReceive"] = true;
            //App.Client.Send(Encoding.UTF8.GetBytes("ReadyToReceive"));
            App.Client.ReceiveCompleted -= ReadyToUpdataRoom;
        }

        private void ServerConnectFailed(object Sender, SocketEventArgs E)
        {
            this.Dispatcher.Invoke(() =>
            {
                SetRoomInfoBox("房间链接失败！", Colors.Red);
                SetStatus("与" + RoomHost + "的房间：" + RoomName + "链接失败。", Colors.Red);
                SetFailedText(E.Message);
            });
            Linking = false;
        }
        private void ServerConnectCompleted(object Sender, SocketEventArgs E)
        {
            this.Dispatcher.Invoke(() =>
            {
                SetStatus("正在抓取" + RoomHost + "的房间："
                    + RoomName + "的信息...", Colors.Orange, false);
            });
            App.Client.ReceiveCompleted += BeginGetRoomData;
        }
        private void BeginGetRoomData(object Sender, SocketEventArgs E)
        {
            int DataLength = int.Parse(Encoding.UTF8.GetString(E.Data).Split(new[] {"|*|"},
                StringSplitOptions.RemoveEmptyEntries)[1]);
            E.Socket.BeginReceive(DataLength);
            this.Dispatcher.Invoke(() => { ReceiveProgressBar.Maximum = DataLength; });

            App.Client.ReceiveCompleted -= BeginGetRoomData;
            App.Client.DataReceiver.ReceiveOnce += ReceivingRoomData;
            App.Client.ReceiveCompleted += EndGetRoomData;
        }
        private void ReceivingRoomData(object Sender, SocketDataReceiver.ReceiveOnceEventArgs E)
        {
            double Progress = E.CumulativeLength/(double) E.TotalLength*100;
            this.Dispatcher.Invoke(() =>
            {
                SetStatus("正在抓取" + RoomHost + "的房间："
                    + RoomName + "的信息...（" + Progress.ToString("0.0") + "%）", Colors.Orange, false);
                SetReceiveProgress(E.CumulativeLength, Colors.Orange);
            });
        }
        private void EndGetRoomData(object Sender, SocketEventArgs E)
        {
            CurrentRoom = App.Client.EndReceive<Room>();
            App.Client.DataReceiver.ReceiveOnce -= ReceivingRoomData;
            App.Client.ReceiveCompleted -= EndGetRoomData;
            this.Dispatcher.Invoke(() =>
            {
                SetRoomInfoBox("已链接房间", Colors.LimeGreen);
                SetStatus("链接房间成功！", Colors.LimeGreen);
                SwitchRoomInfoGrid(true);
                JoinButton.IsEnabled = true;
            });
        }

        private void SetStatus(string Text, Color Color, bool Temp = true)
        {
            if (StatusTimer != null && StatusTimer.IsEnabled)
            {
                StatusTimer.Stop();

                StatusText.Text = Text;
                StatusBar.Background = new SolidColorBrush(Color);

                if (Temp)
                {
                    StatusTimer = new DispatcherTimer() {Interval = TimeSpan.FromSeconds(5)};
                    StatusTimer.Tick += delegate
                    {
                        StatusBar.BeginAnimation(MarginProperty, new ThicknessAnimation()
                        {
                            From = StatusBar.Margin,
                            To = new Thickness(0, 0, 0, -30),
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseIn}
                        });
                        StatusTimer.Stop();
                    };
                    StatusTimer.Start();
                }
            }
            else
            {
                StatusText.Text = Text;
                StatusBar.Background = new SolidColorBrush(Color);

                if (Temp)
                {
                    StatusTimer = new DispatcherTimer() {Interval = TimeSpan.FromSeconds(5)};
                    StatusTimer.Tick += delegate
                    {
                        StatusBar.BeginAnimation(MarginProperty, new ThicknessAnimation()
                        {
                            From = StatusBar.Margin,
                            To = new Thickness(0, 0, 0, -30),
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseIn}
                        });
                        StatusTimer.Stop();
                    };
                }

                if (StatusBar.Margin.Bottom > -30) return;
                var Animation = new ThicknessAnimation()
                {
                    From = StatusBar.Margin,
                    To = new Thickness(0, 0, 0, 0),
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut}
                };
                if (Temp) Animation.Completed += delegate { StatusTimer.Start(); };
                StatusBar.BeginAnimation(MarginProperty, Animation);
            }

        }
        private void SetRoomInfoBox(string Header, Color Color)
        {
            RoomInfoBox.Background = RoomInfoBox.Background.Clone();
            RoomInfoBox.BorderBrush = RoomInfoBox.BorderBrush.Clone();

            RoomInfoBox.Header = Header;
            ((SolidColorBrush)RoomInfoBox.Background).BeginAnimation(SolidColorBrush.ColorProperty, 
                new ColorAnimation()
                {
                    From = ((SolidColorBrush)RoomInfoBox.Background).Color,
                    To = Color,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });
            ((SolidColorBrush)RoomInfoBox.BorderBrush).BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation()
                {
                    From = ((SolidColorBrush)RoomInfoBox.BorderBrush).Color,
                    To = Color,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });
        }
        private void SetFailedText(string Text)
        {
            ProgressRing.Visibility = Visibility.Hidden;
            //ProgressText.HorizontalAlignment = HorizontalAlignment.Center;
            ProgressText.Text = Text;
            double ProgressTextWidth = new FormattedText(Text, System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, new Typeface(ProgressText.FontFamily.ToString()),
                ProgressText.FontSize, ProgressText.Foreground).WidthIncludingTrailingWhitespace;
            ProgressText.Width = ProgressTextWidth > 400 ? 400 : ProgressTextWidth;
            ProgressText.Margin = new Thickness((InProgressGrid.ActualWidth - ProgressText.Width) / 2 + 40, 0, 0, 0);

            ErrorImg.Visibility = Visibility.Visible;
            ErrorImg.Margin = new Thickness((InProgressGrid.ActualWidth - ProgressText.Width - 10) / 2, 0, 0, 0);
            SetReceiveProgress(0, Colors.Red);
        }
        private void ResetInProgress()
        {
            ErrorImg.Visibility = Visibility.Hidden;
            ProgressRing.Visibility = Visibility.Visible;
            ProgressText.Text = "正在尝试链接房间...";
            ReceiveProgressBar.Value = 0;
        }
        private void SetReceiveProgress(int Length, Color Color)
        {
            ReceiveProgressBar.Foreground = ReceiveProgressBar.Foreground.Clone() as SolidColorBrush;
            ReceiveProgressBar.BeginAnimation(RangeBase.ValueProperty, new DoubleAnimation()
            {
                From = ReceiveProgressBar.Value,
                To = Length,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
            });
            ((SolidColorBrush)ReceiveProgressBar.Foreground).BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation()
                {
                    From = ((SolidColorBrush)ReceiveProgressBar.Foreground).Color,
                    To = Color,
                    Duration = TimeSpan.FromSeconds(0.2),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                });
        }
        private void SwitchRoomInfoGrid(bool Enter)
        {
            if (Enter)
            {
                var FadeOutAnimation = new DoubleAnimation()
                {
                    From = InProgressGrid.Opacity,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn }
                };
                FadeOutAnimation.Completed += delegate
                {
                    InProgressGrid.Visibility = Visibility.Hidden;
                    RoomInfoGrid.Visibility = Visibility.Visible;

                    RoomNameLabel.Content = CurrentRoom.Name;
                    BattleTypeLabel.Content = BattleTypeDictionary[CurrentRoom.BattleType];
                    if (CurrentRoom.Host.Avator != null)
                        using (MemoryStream Stream = new MemoryStream())
                        {
                            CurrentRoom.Host.Avator.Save(Stream, ImageFormat.Png);
                            BitmapImage Temp = new BitmapImage();
                            Temp.BeginInit();
                            Temp.CacheOption = BitmapCacheOption.OnLoad;
                            Temp.StreamSource = Stream;
                            Temp.EndInit();
                            HostImg.ImageSource = Temp;
                        }
                    HostNameBlock.Text = CurrentRoom.Host.Name;
                    HostIntroBlock.Text = CurrentRoom.Host.Introduction;
                    RoomDescriptionBlock.Text = CurrentRoom.Description;

                    ParticipantStack.Children.Clear();
                    foreach (var Participant in CurrentRoom.Members)
                    {
                        var Item = new ParticipantDisplaySmall()
                        {
                            Height = 35,
                            Width = 325
                        };
                        if (Participant.Avator != null)
                            using (MemoryStream Stream = new MemoryStream())
                            {
                                Participant.Avator.Save(Stream, ImageFormat.Png);
                                BitmapImage Temp = new BitmapImage();
                                Temp.BeginInit();
                                Temp.CacheOption = BitmapCacheOption.OnLoad;
                                Temp.StreamSource = Stream;
                                Temp.EndInit();
                                Item.AvatorImage.ImageSource = Temp;
                            }
                        Item.UserName.Text = Participant.Name;
                        ParticipantStack.Children.Add(Item);
                    }

                    RoomInfoGrid.BeginAnimation(OpacityProperty, new DoubleAnimation()
                    {
                        From = RoomInfoGrid.Opacity,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.3),
                        EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
                    });
                };
                InProgressGrid.BeginAnimation(OpacityProperty, FadeOutAnimation);
            }
            else
            {
                var FadeOutAnimation = new DoubleAnimation()
                {
                    From = RoomInfoGrid.Opacity,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn }
                };
                FadeOutAnimation.Completed += delegate
                {
                    RoomInfoGrid.Visibility = Visibility.Hidden;
                    InProgressGrid.Visibility = Visibility.Visible;

                    SetRoomInfoBox("链接房间中...", Colors.Orange);
                    ResetInProgress();

                    InProgressGrid.BeginAnimation(OpacityProperty, new DoubleAnimation()
                    {
                        From = InProgressGrid.Opacity,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.3),
                        EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
                    });
                };
                RoomInfoGrid.BeginAnimation(OpacityProperty, FadeOutAnimation);
            }
        }
    }
}
