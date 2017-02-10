using System;
using System.Collections.Generic;
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
using StrikeOne.Core;

namespace StrikeOne
{
    /// <summary>
    /// LoginPage.xaml 的交互逻辑
    /// </summary>
    public partial class LoginPage : UserControl
    {
        private List<FrameworkElement> ElementList { set; get; }
        public Action LeaveAction { set; private get; }

        public LoginPage()
        {
            InitializeComponent();
        }

        public void PageEnter()
        {
            IO.LoadUsers();

            if (App.UserList.Any())
            {
                NonUserText.Visibility = Visibility.Hidden;
                NonUserDescription.Visibility = Visibility.Hidden;
                SignupButton.Visibility = Visibility.Hidden;

                UsersBox.ItemsSource = App.UserList;
                UsersBox.SelectedIndex = 0;

                ElementList = new List<FrameworkElement>()
                {
                    Avator,
                    UsersBox,
                    ButtonGrid
                };
                foreach (var Element in ElementList)
                    Element.Opacity = 0;
            }
            else
            {
                Avator.Visibility = Visibility.Hidden;
                UsersBox.Visibility = Visibility.Hidden;
                ButtonGrid.Visibility = Visibility.Hidden;

                ElementList = new List<FrameworkElement>()
                {
                    NonUserText,
                    NonUserDescription,
                    SignupButton
                };
                foreach (var Element in ElementList)
                    Element.Opacity = 0;
            }

            DispatcherTimer Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromSeconds(0.25);
            int Count = 0;
            Timer.Tick += delegate
            {
                ElementList[Count].BeginAnimation(OpacityProperty, new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut },
                });
                ElementList[Count].BeginAnimation(MarginProperty, new ThicknessAnimation()
                {
                    From = new Thickness
                        (
                            ElementList[Count].Margin.Left + 50,
                            ElementList[Count].Margin.Top,
                            ElementList[Count].Margin.Right - 50,
                            ElementList[Count].Margin.Bottom
                        ),
                    To = ElementList[Count].Margin,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut },
                });
                Count++;
                if (Count >= ElementList.Count)
                    Timer.Stop();
            };
            Timer.Start();
        }

        public void PageLeave()
        {
            DispatcherTimer Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromSeconds(0.25);
            int Count = 0;
            Timer.Tick += delegate
            {
                ElementList[Count].BeginAnimation(OpacityProperty, new DoubleAnimation()
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn },
                    FillBehavior = FillBehavior.Stop
                });
                var MarginAnimation = new ThicknessAnimation()
                {
                    From = ElementList[Count].Margin,
                    To = new Thickness
                    (
                        ElementList[Count].Margin.Left - 50,
                        ElementList[Count].Margin.Top,
                        ElementList[Count].Margin.Right + 50,
                        ElementList[Count].Margin.Bottom
                    ),
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn },
                    FillBehavior = FillBehavior.Stop
                };
                if (Count == ElementList.Count - 1)
                    MarginAnimation.Completed += delegate { LeaveAction?.Invoke(); };
                ElementList[Count].BeginAnimation(MarginProperty, MarginAnimation);
                Count++;
                if (Count >= ElementList.Count)
                    Timer.Stop();
            };
            Timer.Start();
        }

        private void LoginClick(object Sender, RoutedEventArgs E)
        {
            if ((User) UsersBox.SelectedItem == null)
            {
                MessageBox.Show("您尚未选中任何一个角色账号。请先选择一个再进行登录", "角色登录",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            LeaveAction += delegate
            {
                App.CurrentUser = (User) UsersBox.SelectedItem;
                MainWindow.Instance.EnterUserPage(true);
            };
            PageLeave();
        }
        private void SignupClick(object Sender, RoutedEventArgs E)
        {
            LeaveAction += delegate { MainWindow.Instance.Signup(); };
            PageLeave();
        }

        private void SelectUser(object Sender, SelectionChangedEventArgs E)
        {
            var CurrentUser = (User) UsersBox.SelectedItem;
            if (CurrentUser == null) return;
            if (CurrentUser.Avator == null)
            {
                AvatorImage.ImageSource = Resources["Icon_empty"] as BitmapImage;
            }
            else
            {
                using (MemoryStream Stream = new MemoryStream())
                {
                    CurrentUser.Avator.Save(Stream, CurrentUser.AvatorFormat);
                    BitmapImage Temp = new BitmapImage();
                    Temp.BeginInit();
                    Temp.CacheOption = BitmapCacheOption.OnLoad;
                    Temp.StreamSource = Stream;
                    Temp.EndInit();
                    AvatorImage.ImageSource = Temp;
                }
            }
        }

        
    }
}
