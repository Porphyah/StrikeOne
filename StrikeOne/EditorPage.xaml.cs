using System;
using System.Collections.Generic;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace StrikeOne
{
    /// <summary>
    /// EditorPage.xaml 的交互逻辑
    /// </summary>
    public partial class EditorPage : UserControl
    {
        public Action LeaveAction { set; private get; }
        private List<FrameworkElement> MenuList { set; get; }

        public EditorPage()
        {
            InitializeComponent();
            MenuList = new List<FrameworkElement>()
            {
                EditSkill,
                EditAi
            };

            //this.Opacity = 0;
            MenuList.ForEach(O => O.Opacity = 0);
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
                DispatcherTimer MenuTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.25) };
                int Count = 0;
                MenuTimer.Tick += delegate
                {
                    MenuList[Count].BeginAnimation(OpacityProperty, new DoubleAnimation()
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    });
                    MenuList[Count].BeginAnimation(MarginProperty, new ThicknessAnimation()
                    {
                        From = new Thickness(
                            MenuList[Count].Margin.Left - 50,
                            MenuList[Count].Margin.Top,
                            MenuList[Count].Margin.Right,
                            MenuList[Count].Margin.Bottom),
                        To = MenuList[Count].Margin,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    });
                    Count++;
                    if (Count >= MenuList.Count)
                        MenuTimer.Stop();
                };
                MenuTimer.Start();
            };
            TitleGrid.BeginAnimation(OpacityProperty, OpacityAnimation);
            TitleGrid.BeginAnimation(MarginProperty, MarginAnimation);
        }
        public void PageLeave()
        {
            DispatcherTimer MenuTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.25) };
            int Count = 0;
            MenuTimer.Tick += delegate
            {
                MenuList[Count].BeginAnimation(OpacityProperty, new DoubleAnimation()
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                });
                var MarginAnimation = new ThicknessAnimation()
                {
                    From = MenuList[Count].Margin,
                    To = new Thickness(
                        MenuList[Count].Margin.Left + 50,
                        MenuList[Count].Margin.Top,
                        MenuList[Count].Margin.Right,
                        MenuList[Count].Margin.Bottom),
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseIn}
                };
                if (Count == MenuList.Count - 1)
                    MarginAnimation.Completed += delegate
                    {
                        DispatcherTimer FinalTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.75) };
                        FinalTimer.Tick += delegate
                        {
                            LeaveAction?.Invoke();
                            FinalTimer.Stop();
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
                        FinalTimer.Start();
                    };
                MenuList[Count].BeginAnimation(MarginProperty, MarginAnimation);
                Count++;
                if (Count >= MenuList.Count)
                    MenuTimer.Stop();
            };
            MenuTimer.Start();
        }

        private void EditSkill_MouseEnter(object Sender, MouseEventArgs E)
        {
            EditSkill_Effect.BeginAnimation(DropShadowEffect.ColorProperty, new ColorAnimation()
            {
                From = EditSkill_Effect.Color,
                To = Colors.White,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            });
            EditSkill_StartGradientStop.BeginAnimation(GradientStop.ColorProperty, new ColorAnimation()
            {
                From = EditSkill_StartGradientStop.Color,
                To = Color.FromArgb(128, 0, 0, 0),
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            });
            EditSkill_EndGradientStop.BeginAnimation(GradientStop.ColorProperty, new ColorAnimation()
            {
                From = EditSkill_EndGradientStop.Color,
                To = Color.FromArgb(128, 0, 0, 0),
                Duration = TimeSpan.FromSeconds(0.8),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            });

            EditSkill_Img.Source = Resources["Skill_white"] as BitmapImage;
            EditSkill_Title.Foreground = EditSkill_Title.Foreground.Clone();
            EditSkill_Title.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation()
            {
                From = ((SolidColorBrush)EditSkill_Title.Foreground).Color,
                To = Colors.White,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            });
            EditSkill_Eng.Foreground = EditSkill_Eng.Foreground.Clone();
            EditSkill_Eng.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation()
            {
                From = ((SolidColorBrush)EditSkill_Eng.Foreground).Color,
                To = Colors.White,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            });
        }
        private void EditSkill_MouseLeave(object Sender, MouseEventArgs E)
        {
            EditSkill_Effect.BeginAnimation(DropShadowEffect.ColorProperty, new ColorAnimation()
            {
                From = EditSkill_Effect.Color,
                To = Colors.Gray,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            });
            EditSkill_StartGradientStop.BeginAnimation(GradientStop.ColorProperty, new ColorAnimation()
            {
                From = EditSkill_StartGradientStop.Color,
                To = Colors.Transparent,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            });
            EditSkill_EndGradientStop.BeginAnimation(GradientStop.ColorProperty, new ColorAnimation()
            {
                From = EditSkill_EndGradientStop.Color,
                To = Colors.Transparent,
                Duration = TimeSpan.FromSeconds(0.8),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            });

            EditSkill_Img.Source = Resources["Skill_grey"] as BitmapImage;
            EditSkill_Title.Foreground = EditSkill_Title.Foreground.Clone();
            EditSkill_Title.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation()
            {
                From = ((SolidColorBrush)EditSkill_Title.Foreground).Color,
                To = Colors.Gray,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            });
            EditSkill_Eng.Foreground = EditSkill_Eng.Foreground.Clone();
            EditSkill_Eng.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation()
            {
                From = ((SolidColorBrush)EditSkill_Eng.Foreground).Color,
                To = Colors.Gray,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            });
        }
        private void EditSkill_Click(object Sender, MouseButtonEventArgs E)
        {
            LeaveAction = delegate { MainWindow.Instance.EditSkill(); };
            this.PageLeave();
        }

        private void EditAi_MouseEnter(object Sender, MouseEventArgs E)
        {
            EditAi_Effect.BeginAnimation(DropShadowEffect.ColorProperty, new ColorAnimation()
            {
                From = EditAi_Effect.Color,
                To = Colors.White,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            });
            EditAi_StartGradientStop.BeginAnimation(GradientStop.ColorProperty, new ColorAnimation()
            {
                From = EditAi_StartGradientStop.Color,
                To = Color.FromArgb(128, 0, 0, 0),
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            });
            EditAi_EndGradientStop.BeginAnimation(GradientStop.ColorProperty, new ColorAnimation()
            {
                From = EditAi_EndGradientStop.Color,
                To = Color.FromArgb(128, 0, 0, 0),
                Duration = TimeSpan.FromSeconds(0.8),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            });

            EditAi_Img.Source = Resources["Ai_white"] as BitmapImage;
            EditAi_Title.Foreground = EditAi_Title.Foreground.Clone();
            EditAi_Title.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation()
            {
                From = ((SolidColorBrush)EditAi_Title.Foreground).Color,
                To = Colors.White,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            });
            EditAi_Eng.Foreground = EditAi_Eng.Foreground.Clone();
            EditAi_Eng.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation()
            {
                From = ((SolidColorBrush)EditAi_Eng.Foreground).Color,
                To = Colors.White,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            });
        }
        private void EditAi_MouseLeave(object Sender, MouseEventArgs E)
        {
            EditAi_Effect.BeginAnimation(DropShadowEffect.ColorProperty, new ColorAnimation()
            {
                From = EditAi_Effect.Color,
                To = Colors.Gray,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            });
            EditAi_StartGradientStop.BeginAnimation(GradientStop.ColorProperty, new ColorAnimation()
            {
                From = EditAi_StartGradientStop.Color,
                To = Colors.Transparent,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            });
            EditAi_EndGradientStop.BeginAnimation(GradientStop.ColorProperty, new ColorAnimation()
            {
                From = EditAi_EndGradientStop.Color,
                To = Colors.Transparent,
                Duration = TimeSpan.FromSeconds(0.8),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            });

            EditAi_Img.Source = Resources["Ai_grey"] as BitmapImage;
            EditAi_Title.Foreground = EditAi_Title.Foreground.Clone();
            EditAi_Title.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation()
            {
                From = ((SolidColorBrush)EditAi_Title.Foreground).Color,
                To = Colors.Gray,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            });
            EditAi_Eng.Foreground = EditAi_Eng.Foreground.Clone();
            EditAi_Eng.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation()
            {
                From = ((SolidColorBrush)EditAi_Eng.Foreground).Color,
                To = Colors.Gray,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            });
        }
        private void EditAi_Click(object Sender, MouseButtonEventArgs E)
        {
            
        }
    }
}
