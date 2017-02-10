using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using MahApps.Metro.Controls;
using StrikeOne.Core;

namespace StrikeOne
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static MainWindow Instance { set; get; }

        public LoginPage LoginPage { set; get; }
        public SignupPage SignupPage { set; get; }
        public UserPage UserPage { set; get; }

        public UserControl CurrentPage { set; get; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object Sender, RoutedEventArgs E)
        {
            Instance = this;
            IO.LoadConfig();
            Login();
        }

        public void Login()
        {
            if (CurrentPage != null)
                MainGrid.Children.Remove(CurrentPage);

            LoginPage = new LoginPage()
            {
                Height = MainGrid.ActualHeight,
                Width = MainGrid.ActualWidth
            };

            MainGrid.Children.Add(LoginPage);
            CurrentPage = LoginPage;
            LoginPage.PageEnter();
        }

        public void Signup()
        {
            if (CurrentPage != null)
                MainGrid.Children.Remove(CurrentPage);

            SignupPage = new SignupPage()
            {
                Height = MainGrid.ActualHeight,
                Width = MainGrid.ActualWidth
            };

            MainGrid.Children.Add(SignupPage);
            CurrentPage = SignupPage;
            SignupPage.PageEnter();
        }

        public void EnterUserPage(bool Login)
        {
            if (CurrentPage != null)
                MainGrid.Children.Remove(CurrentPage);

            UserPage = new UserPage()
            {
                Height = MainGrid.ActualHeight,
                Width = MainGrid.ActualWidth
            };

            MainGrid.Children.Add(UserPage);
            CurrentPage = UserPage;
            UserPage.PageEnter(Login);
        }

        public void ShowInfoGrid(bool Init = false)
        {
            if (Init) InfoGrid.Init();
            InfoGrid.BeginAnimation(MarginProperty, new ThicknessAnimation()
            {
                From = InfoGrid.Margin,
                To = new Thickness(0, 0, 0, 0),
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            });
        }

        public void HideInfoGrid()
        {
            InfoGrid.BeginAnimation(MarginProperty, new ThicknessAnimation()
            {
                From = InfoGrid.Margin,
                To = new Thickness(-300, 0, 0, 0),
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            });
        }
    }
}
