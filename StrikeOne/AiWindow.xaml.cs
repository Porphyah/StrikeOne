using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
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
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using StrikeOne.Components;
using StrikeOne.Core;

namespace StrikeOne
{
    /// <summary>
    /// AiWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AiWindow : MetroWindow
    {
        public bool Canceled { private set; get; } = true;
        public bool SelfChoosable { set; private get; } = true;
        public List<AI> ChosenAi { set; private get; } = new List<AI>();  
        public bool IsSelectingAi { private set; get; } = false;
        public AI SelectedAi { private set; get; }

        public AiWindow()
        {
            InitializeComponent();
            UserGrid.Tag = false;
            AiGrid.Tag = false;

            UserName.Text = App.CurrentUser.Name;
            if (App.CurrentUser.Avator != null)
                using (MemoryStream Stream = new MemoryStream())
                {
                    App.CurrentUser.Avator.Save(Stream, ImageFormat.Png);
                    BitmapImage Temp = new BitmapImage();
                    Temp.BeginInit();
                    Temp.CacheOption = BitmapCacheOption.OnLoad;
                    Temp.StreamSource = Stream;
                    Temp.EndInit();
                    AvatorImage.ImageSource = Temp;
                }
        }

        private void MouseEnterGrid(object Sender, MouseEventArgs E)
        {
            var Target = Sender as Grid;
            if ((bool) Target.Tag) return;
            Target.Background = new SolidColorBrush(Color.FromArgb(136, 15, 72, 128));
        }
        private void MouseLeaveGrid(object Sender, MouseEventArgs E)
        {
            var Target = Sender as Grid;
            if ((bool)Target.Tag) return;
            Target.Background = new SolidColorBrush(Color.FromArgb(136, 0, 0, 0));
        }
        private void UserGrid_Click(object Sender, MouseButtonEventArgs E)
        {
            UserGrid.Tag = true;
            UserGrid.Background = new SolidColorBrush(Color.FromArgb(136, 30, 144, 255));
            if ((bool) AiGrid.Tag)
            {
                AiGrid.Background = new SolidColorBrush(Color.FromArgb(136, 0, 0, 0));
                AiGrid.Tag = false;
            }
            AcceptButton.Visibility = Visibility.Visible;
        }
        private void AiGrid_Click(object Sender, MouseButtonEventArgs E)
        {
            AiGrid.Tag = true;
            AiGrid.Background = new SolidColorBrush(Color.FromArgb(136, 30, 144, 255));
            if ((bool)UserGrid.Tag)
            {
                UserGrid.Background = new SolidColorBrush(Color.FromArgb(136, 0, 0, 0));
                UserGrid.Tag = false;
            }
            AcceptButton.Visibility = Visibility.Visible;
        }

        private void Confirm_Click(object Sender, RoutedEventArgs E)
        {
            if (!IsSelectingAi)
            {
                if ((bool) AiGrid.Tag)
                {
                    var MarginAnimation = new ThicknessAnimation()
                    {
                        From = FirstGrid.Margin,
                        To = new Thickness(
                                FirstGrid.Margin.Left - 50,
                                FirstGrid.Margin.Top,
                                FirstGrid.Margin.Right + 50,
                                FirstGrid.Margin.Bottom
                            ),
                        Duration = TimeSpan.FromSeconds(0.25),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    };
                    MarginAnimation.Completed += delegate
                    {
                        AiStack.Children.Clear();
                        foreach (var Ai in App.AiList)
                        {
                            if (ChosenAi.Contains(Ai)) continue;
                            var AiItem = new AiItem()
                            { Height = 350, Width = 250 };
                            AiItem.Init(Ai);
                            AiItem.SelectSyncAction = delegate(AI Target)
                            {
                                SelectedAi = Target;
                                AiStack.Children.OfType<AiItem>()
                                    .Where(O => !ReferenceEquals(O.Ai, Target))
                                    .ToList().ForEach(O => O.Select(false));
                            };
                            AiStack.Children.Add(AiItem);
                        }

                        FirstGrid.Visibility = Visibility.Hidden;
                        SecondGrid.Visibility = Visibility.Visible;
                        SecondGrid.BeginAnimation(OpacityProperty, new DoubleAnimation()
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromSeconds(0.25),
                            EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                        });
                        SecondGrid.BeginAnimation(MarginProperty, new ThicknessAnimation()
                        {
                            From = new Thickness(
                                SecondGrid.Margin.Left + 50,
                                SecondGrid.Margin.Top,
                                SecondGrid.Margin.Right - 50,
                                SecondGrid.Margin.Bottom
                            ),
                            To = SecondGrid.Margin,
                            Duration = TimeSpan.FromSeconds(0.25),
                            EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                        });
                    };
                    FirstGrid.BeginAnimation(OpacityProperty, new DoubleAnimation()
                    {
                        From = 1,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.25),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    });
                    FirstGrid.BeginAnimation(MarginProperty, MarginAnimation);
                    IsSelectingAi = true;
                }
                else
                {
                    if (!SelfChoosable)
                    {
                        MessageBox.Show("您已经将自己加入某个角色槽了。若要重新选择角色槽，请先从原来的角色槽退出。",
                            "选择加入者", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    IsSelectingAi = false;
                    Canceled = false;
                    this.Close();
                }
            }
            else
            {
                if (SelectedAi == null)
                {
                    MessageBox.Show("请先选择一个AI角色。", "选择加入者",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                IsSelectingAi = true;
                Canceled = false;
                this.Close();
            }
        }
        private void Cancel_Click(object Sender, RoutedEventArgs E)
        {
            Canceled = true;
            this.Close();
        }
    }
}
