using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;

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

        public UserControl CurrentPage { set; get; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object Sender, RoutedEventArgs E)
        {
            Instance = this;
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
    }
}
