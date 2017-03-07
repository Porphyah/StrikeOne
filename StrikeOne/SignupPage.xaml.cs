using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
using Microsoft.Win32;
using StrikeOne.Core;

namespace StrikeOne
{
    /// <summary>
    /// SignupPage.xaml 的交互逻辑
    /// </summary>
    public partial class SignupPage : UserControl
    {
        public Action LeaveAction { set; private get; }
        public System.Drawing.Image Avator { set; get; }
        public ImageFormat AvatorFormat { set; get; }
        private DispatcherTimer StatusTimer { set; get; }

        public SignupPage()
        {
            InitializeComponent();
        }

        public void PageEnter()
        {
            DispatcherTimer Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromSeconds(0.25);
            int Count = 0;
            Timer.Tick += delegate
            {
                switch (Count)
                {
                    case 0:
                        AvatorEllipse.BeginAnimation(OpacityProperty, new DoubleAnimation()
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut},
                        });
                        break;
                    case 1:
                        NameLabel.BeginAnimation(OpacityProperty, new DoubleAnimation()
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut},
                        });
                        NameBox.BeginAnimation(OpacityProperty, new DoubleAnimation()
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut},
                        });
                        break;
                    case 2:
                        DescriptionLabel.BeginAnimation(OpacityProperty, new DoubleAnimation()
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut},
                        });
                        DescriptionBox.BeginAnimation(OpacityProperty, new DoubleAnimation()
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut},
                        });
                        break;
                    case 3:
                        BackButton.BeginAnimation(OpacityProperty, new DoubleAnimation()
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut},
                        });
                        SignupButton.BeginAnimation(OpacityProperty, new DoubleAnimation()
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut},
                        });
                        break;
                }
                Count++;
                if (Count >= 4)
                    Timer.Stop();
            };
            Timer.Start();
        }

        public void PageLeave()
        {
            DispatcherTimer Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromSeconds(0.5);
            Timer.Tick += delegate
            {
                LeaveAction?.Invoke();
                Timer.Stop();
            };

            AvatorEllipse.BeginAnimation(OpacityProperty, new DoubleAnimation()
            {
                From = AvatorEllipse.Opacity,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn },
            });
            MaskEllipse.BeginAnimation(OpacityProperty, new DoubleAnimation()
            {
                From = MaskEllipse.Opacity,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn },
            });
            NameLabel.BeginAnimation(OpacityProperty, new DoubleAnimation()
            {
                From = NameLabel.Opacity,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn },
            });
            NameBox.BeginAnimation(OpacityProperty, new DoubleAnimation()
            {
                From = NameBox.Opacity,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn },
            });
            DescriptionLabel.BeginAnimation(OpacityProperty, new DoubleAnimation()
            {
                From = DescriptionLabel.Opacity,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn },
            });
            DescriptionBox.BeginAnimation(OpacityProperty, new DoubleAnimation()
            {
                From = DescriptionBox.Opacity,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn },
            });
            BackButton.BeginAnimation(OpacityProperty, new DoubleAnimation()
            {
                From = BackButton.Opacity,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn },
            });
            SignupButton.BeginAnimation(OpacityProperty, new DoubleAnimation()
            {
                From = SignupButton.Opacity,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn },
            });
            StatusBar.BeginAnimation(OpacityProperty, new DoubleAnimation()
            {
                From = StatusBar.Opacity,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn },
            });
            Timer.Start();
        }

        private void EnterAvatorMask(object Sender, MouseEventArgs E)
        {
            MaskEllipse.BeginAnimation(OpacityProperty, new DoubleAnimation()
            {
                From = MaskEllipse.Opacity,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut },
            });
        }
        private void LeaveAvatorMask(object Sender, MouseEventArgs E)
        {
            MaskEllipse.BeginAnimation(OpacityProperty, new DoubleAnimation()
            {
                From = MaskEllipse.Opacity,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut },
            });
        }

        private void BackClick(object Sender, RoutedEventArgs E)
        {
            LeaveAction = delegate { MainWindow.Instance.Login(); };
            PageLeave();
        }
        private void SignupClick(object Sender, RoutedEventArgs E)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                SetStatus("角色名不能为空。", Colors.OrangeRed);
                return;
            }

            Thread SignupThread = new Thread(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    SetStatus("生成角色信息...", Colors.Orange);
                });
                User NewUser = new User();
                this.Dispatcher.Invoke(() =>
                {
                    NewUser = new User()
                    {
                        Id = Guid.NewGuid(),
                        Name = NameBox.Text,
                        Avator = Avator,
                        AvatorFormat = AvatorFormat,
                        Introduction = DescriptionBox.Text,
                        LanIpAddress = App.LanIpAddress,
                        WanIpAddress = App.WanIpAddress
                    };
                    App.UserList.Add(NewUser);
                });

                this.Dispatcher.Invoke(() =>
                {
                    SetStatus("保存角色信息...", Colors.Orange);
                });
                IO.SaveUser(NewUser);

                this.Dispatcher.Invoke(() =>
                {
                    SetStatus("完成注册！", Colors.LimeGreen);
                });

                this.Dispatcher.Invoke(() =>
                {
                    LeaveAction = delegate { MainWindow.Instance.Login(); };
                    PageLeave();
                });
            }) { Name = "注册线程" };
            SignupThread.Start();
        }

        private void UploadAvator(object Sender, MouseButtonEventArgs E)
        {
            OpenFileDialog FileDialog = new OpenFileDialog()
            {
                Filter = "图像文件|*.bmp;*.jpg;*.png;*.tif",
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };
            bool? Result = FileDialog.ShowDialog();
            if (!Result.HasValue || !Result.Value) return;
            string ImagePath = FileDialog.FileName;

            AvatorWindow Dialog = new AvatorWindow();
            Dialog.Init(ImagePath);
            Dialog.ShowDialog();
            if (Dialog.Canceled) return;

            Avator = Dialog.Target;
            AvatorFormat = Dialog.ImageFormat;

            using (MemoryStream Stream = new MemoryStream())
            {
                Avator.Save(Stream, Dialog.ImageFormat);
                BitmapImage Temp = new BitmapImage();
                Temp.BeginInit();
                Temp.CacheOption = BitmapCacheOption.OnLoad;
                Temp.StreamSource = Stream;
                Temp.EndInit();
                AvatorImage.ImageSource = Temp;
            }
        }

        private void SetStatus(string Text, System.Windows.Media.Color Color)
        {
            if (StatusTimer != null && StatusTimer.IsEnabled)
            {
                StatusTimer.Stop();

                StatusText.Text = Text;
                StatusBar.Background = new SolidColorBrush(Color);

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
