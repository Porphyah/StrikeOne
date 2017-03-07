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
        public LocalStrikePage LocalStrikePage { set; get; }
        public RoomWizardPage RoomWizardPage { set; get; }
        public RoomJoinPage RoomJoinPage { set; get; }
        public RoomPage RoomPage { set; get; }

        public EditorPage EditorPage { set; get; }
        public EditSkillPage EditSkillPage { set; get; }

        public UserControl CurrentPage { set; get; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object Sender, RoutedEventArgs E)
        {
            Instance = this;
            IO.LoadConfig();
            if (!App.EditorMode)
                Login();
            else
                EnterEditorMode();
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
                EasingFunction = new ExponentialEase() { Exponent = Math.E, EasingMode = EasingMode.EaseOut }
            });
        }

        public void HideInfoGrid()
        {
            InfoGrid.BeginAnimation(MarginProperty, new ThicknessAnimation()
            {
                From = InfoGrid.Margin,
                To = new Thickness(-300, 0, 0, 0),
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { Exponent = Math.E, EasingMode = EasingMode.EaseIn }
            });
        }

        public void EnterLocalStrike()
        {
            if (CurrentPage != null)
                MainGrid.Children.Remove(CurrentPage);

            LocalStrikePage = new LocalStrikePage()
            {
                Height = MainGrid.ActualHeight,
                Width = MainGrid.ActualWidth
            };

            MainGrid.Children.Add(LocalStrikePage);
            CurrentPage = LocalStrikePage;
            LocalStrikePage.PageEnter();
        }

        public void EnterRoomWizardPage()
        {
            if (CurrentPage != null)
                MainGrid.Children.Remove(CurrentPage);

            RoomWizardPage = new RoomWizardPage()
            {
                Height = MainGrid.ActualHeight,
                Width = MainGrid.ActualWidth
            };

            MainGrid.Children.Add(RoomWizardPage);
            CurrentPage = RoomWizardPage;
            RoomWizardPage.PageEnter();
        }

        public void EnterRoomJoinPage()
        {
            if (CurrentPage != null)
                MainGrid.Children.Remove(CurrentPage);

            RoomJoinPage = new RoomJoinPage()
            {
                Height = MainGrid.ActualHeight,
                Width = MainGrid.ActualWidth
            };

            MainGrid.Children.Add(RoomJoinPage);
            CurrentPage = RoomJoinPage;
            RoomJoinPage.PageEnter();
        }

        public void EnterRoomPage(Room Target)
        {
            if (CurrentPage != null)
                MainGrid.Children.Remove(CurrentPage);

            RoomPage = new RoomPage()
            {
                Height = MainGrid.ActualHeight,
                Width = MainGrid.ActualWidth
            };

            MainGrid.Children.Add(RoomPage);
            CurrentPage = RoomPage;
            RoomPage.PageEnter(Target);
        }

        public void EnterEditorMode()
        {
            if (CurrentPage != null)
                MainGrid.Children.Remove(CurrentPage);

            EditorPage = new EditorPage()
            {
                Height = MainGrid.ActualHeight,
                Width = MainGrid.ActualWidth
            };

            MainGrid.Children.Add(EditorPage);
            CurrentPage = EditorPage;
            EditorPage.PageEnter();
        }

        public void EditSkill()
        {
            if (CurrentPage != null)
                MainGrid.Children.Remove(CurrentPage);

            EditSkillPage = new EditSkillPage()
            {
                Height = MainGrid.ActualHeight,
                Width = MainGrid.ActualWidth
            };

            MainGrid.Children.Add(EditSkillPage);
            CurrentPage = EditSkillPage;
            EditSkillPage.PageEnter();
        }
    }
}
