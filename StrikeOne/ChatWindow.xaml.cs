using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using StrikeOne.Components;
using StrikeOne.Core;
using StrikeOne.Core.Network;

namespace StrikeOne
{
    /// <summary>
    /// ChatWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ChatWindow : MetroWindow
    {
        public Room CurrentRoom { set; private get; }

        public bool AllowToClose { set; get; } = false;
        public bool CtrlEnterMode { set; get; } = false;

        public ChatWindow()
        {
            InitializeComponent();
            ChatStack.Height = 0;
            //SendMessage("System Initalizing...");
        }

        private void WindowClosing(object Sender, CancelEventArgs E)
        {
            if (AllowToClose) return;
            this.Hide();
            E.Cancel = true;
        }

        private void SendClick(object Sender, RoutedEventArgs E)
        {
            SendMessage(InputBox.Text, App.CurrentUser, CurrentRoom.Host.Id == App.CurrentUser.Id);
            InputBox.Text = null;
        }
        private void InputKeyDown(object sender, KeyEventArgs e)
        {
            if ((!CtrlEnterMode && e.KeyboardDevice.Modifiers == ModifierKeys.None && e.Key == Key.Enter) || 
                (CtrlEnterMode && e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.Enter))
            {
                //InputBox.Text = InputBox.Text.Substring(0, InputBox.Text.Length - 1);
                SendMessage(InputBox.Text, App.CurrentUser, CurrentRoom.Host.Id == App.CurrentUser.Id);
                InputBox.Text = null;
            }
        }

        public void SendMessage(string Text, Player Speaker = null, bool Host = false)
        {
            if (CurrentRoom.IsHost(App.CurrentUser))
                foreach (var Client in App.Server)
                    Client.SendAsync(Encoding.UTF8.GetBytes("ChatMessage|*|" +
                        (Speaker?.Id.ToString() ?? "System") + "|*|" + Text));
            else
                App.Client.SendAsync(Encoding.UTF8.GetBytes("ChatMessage|*|" +
                    (Speaker?.Id.ToString() ?? "System") + "|*|" + Text));

            bool ScrollToBottom = ChatScrollViewer.HorizontalOffset >= ChatStack.ActualHeight - ChatScrollViewer.ActualHeight;
            ChatterItem Item = new ChatterItem();
            Item.Init(Text, Speaker, Host);

            Item.Opacity = 0;
            ChatStack.Children.Add(Item);
            var HeightAnimation = new DoubleAnimation()
            {
                From = ChatStack.ActualHeight,
                To = ChatStack.Children.OfType<FrameworkElement>().Sum(O => O.Height),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
            };
            HeightAnimation.Completed += delegate
            {
                Item.BeginAnimation(OpacityProperty, new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.2),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                });
                if (ScrollToBottom) ChatScrollViewer.ScrollToBottom();
            };
            ChatStack.BeginAnimation(HeightProperty, HeightAnimation);
        }
        public void SendMessageAlone(string Text, Player Speaker = null, bool Host = false)
        {
            bool ScrollToBottom = ChatScrollViewer.HorizontalOffset >= ChatStack.ActualHeight - ChatScrollViewer.ActualHeight;
            ChatterItem Item = new ChatterItem();
            Item.Init(Text, Speaker, Host);

            Item.Opacity = 0;
            ChatStack.Children.Add(Item);
            var HeightAnimation = new DoubleAnimation()
            {
                From = ChatStack.ActualHeight,
                To = ChatStack.Children.OfType<FrameworkElement>().Sum(O => O.Height),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
            };
            HeightAnimation.Completed += delegate
            {
                Item.BeginAnimation(OpacityProperty, new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.2),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                });
                if (ScrollToBottom) ChatScrollViewer.ScrollToBottom();
            };
            ChatStack.BeginAnimation(HeightProperty, HeightAnimation);
        }
        public ProgressItem BeginProgress(string Description, double Length)
        {
            bool ScrollToBottom = ChatScrollViewer.HorizontalOffset >= ChatStack.ActualHeight - ChatScrollViewer.ActualHeight;
            ChatterItem Item = new ChatterItem();
            var ProgressItem = Item.SystemProgressInit(Description, Length);

            Item.Opacity = 0;
            ChatStack.Children.Add(Item);
            var HeightAnimation = new DoubleAnimation()
            {
                From = ChatStack.ActualHeight,
                To = ChatStack.Children.OfType<FrameworkElement>().Sum(O => O.Height),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
            };
            HeightAnimation.Completed += delegate
            {
                Item.BeginAnimation(OpacityProperty, new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.2),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                });
                if (ScrollToBottom) ChatScrollViewer.ScrollToBottom();
            };
            ChatStack.BeginAnimation(HeightProperty, HeightAnimation);

            return ProgressItem;
        }

        private void CtrlEnterChecked(object Sender, RoutedEventArgs E)
        {
            CtrlEnterMode = true;
        }
        private void CtrlEnterUnchecked(object Sender, RoutedEventArgs E)
        {
            CtrlEnterMode = false;
        }

        public void MessageReceived(object Sender, SocketEventArgs E)
        {
            string Data = Encoding.UTF8.GetString(E.Data);
            string[] Fragments = Data.Split(new[] {"|*|"},
                StringSplitOptions.RemoveEmptyEntries);
            if (Fragments[0] != "ChatMessage") return;

            Player Speaker = Fragments[1] == "System" ? null : CurrentRoom.Members
                .Find(O => O.Id == Guid.Parse(Fragments[1]));
            this.Dispatcher.Invoke(() =>
                SendMessageAlone(Fragments[2], Speaker, 
                Speaker != null && CurrentRoom.IsHost(Speaker)));

            if (CurrentRoom.IsHost(App.CurrentUser))
            {
                var From = E.Socket["User"] as User;
                foreach (var Client in App.Server
                    .Where(Client => !From.Equals(Client["User"])))
                    Client.SendAsync(E.Data);
            }
        }
    }
}
