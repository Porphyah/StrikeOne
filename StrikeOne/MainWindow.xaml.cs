using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        public DebugWindow DebugWindow { private set; get; }

        public LoginPage LoginPage { set; get; }
        public SignupPage SignupPage { set; get; }
        public UserPage UserPage { set; get; }
        public LocalStrikePage LocalStrikePage { set; get; }
        public RoomWizardPage RoomWizardPage { set; get; }
        public RoomJoinPage RoomJoinPage { set; get; }
        public RoomPage RoomPage { set; get; }
        public PrepareStrikePage PrepareStrikePage { set; get; }
        public BattlefieldPage BattlefieldPage { set; get; }

        public EditorPage EditorPage { set; get; }
        public EditSkillPage EditSkillPage { set; get; }
        public EditAiPage EditAiPage { set; get; }

        public UserControl CurrentPage { set; get; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object Sender, RoutedEventArgs E)
        {
            Instance = this;

            IO.LoadConfig();
            IO.LoadSkills();
            IO.LoadUsers();
            IO.LoadAis();

            if (!App.EditorMode)
            {
                if (App.DebugMode)
                {
                    DebugWindow = new DebugWindow();
                    DebugWindow.Show();
                }
                Login();
            }
            else
                EnterEditorMode();
        }

        public void SetLog(string Text, Color Color, bool Bold = false, bool Italic = false)
        {
            DebugWindow?.SetLog(Text, Color, Bold, Italic);
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

        public void PrepareStrike(Room Target)
        {
            if (CurrentPage != null)
                MainGrid.Children.Remove(CurrentPage);

            PrepareStrikePage = new PrepareStrikePage()
            {
                Height = MainGrid.ActualHeight,
                Width = MainGrid.ActualWidth
            };

            MainGrid.Children.Add(PrepareStrikePage);
            CurrentPage = PrepareStrikePage;
            PrepareStrikePage.PageEnter(Target);
        }

        public void EnterBattlefield(Battlefield Battlefield)
        {
            if (CurrentPage != null)
                MainGrid.Children.Remove(CurrentPage);

            BattlefieldPage = new BattlefieldPage()
            {
                Height = MainGrid.ActualHeight,
                Width = MainGrid.ActualWidth
            };

            MainGrid.Children.Add(BattlefieldPage);
            CurrentPage = BattlefieldPage;
            BattlefieldPage.PageEnter(Battlefield);
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
        public void EditAi()
        {
            if (CurrentPage != null)
                MainGrid.Children.Remove(CurrentPage);

            EditAiPage = new EditAiPage()
            {
                Height = MainGrid.ActualHeight,
                Width = MainGrid.ActualWidth
            };

            MainGrid.Children.Add(EditAiPage);
            CurrentPage = EditAiPage;
            EditAiPage.PageEnter();
        }
    }
}
