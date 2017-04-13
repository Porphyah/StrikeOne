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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace StrikeOne.Components
{
    /// <summary>
    /// MarkerProgressBar.xaml 的交互逻辑
    /// </summary>
    public partial class MarkerProgressBar : UserControl
    {
        public MarkerProgressBar()
        {
            InitializeComponent();
        }

        public double BeforeValue { set; get; }
        public double AfterValue { set; get; }

        private DispatcherTimer Timer { set; get; }

        public void Init(double Before, double After)
        {
            BeforeValue = Before;
            AfterValue = After;

            ResetAnimation();

            BeforeText.Opacity = 0;
            AfterText.Opacity = 0;
            BeforeLine.Opacity = 0;
            AfterLine.Opacity = 0;
            ChangeText.Opacity = 0;

            BeforeLine.X1 = 0;
            BeforeLine.X2 = 0;
            AfterLine.X1 = 0;
            AfterLine.X2 = 0;
            AfterLine.Y1 = 15;

            TopBar.Width = 0;
            BottomBar.Width = 0;
            BottomBar.Margin = new Thickness(0, 20, 0, 0);

            BeforeText.Text = (Before*100).ToString("0.0") + "%";
            AfterText.Text = (After*100).ToString("0.0") + "%";
            ChangeText.Text = (After - Before >= 0 ? "+" : "") + 
                ((After - Before)*100).ToString("0.0") + "%";
        }

        public void Animate()
        {
            if (BeforeValue <= AfterValue)
            {
                TopBar.Fill = new SolidColorBrush(GetColor(BeforeValue));
                var BeforeValueAnimation = new DoubleAnimation()
                {
                    From = 0,
                    To = 300 * BeforeValue,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                };
                BeforeValueAnimation.Completed += delegate
                {
                    BeforeText.Margin = new Thickness(300*BeforeValue - BeforeText.ActualWidth/2, 0, 0, 0);
                    BeforeText.BeginAnimation(OpacityProperty, new DoubleAnimation()
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut}
                    });

                    Timer = new DispatcherTimer() {Interval = TimeSpan.FromSeconds(0.5)};
                    Timer.Tick += delegate
                    {
                        StartChangeColor.Color = Color.FromArgb(0, 50, 205, 50);
                        EndChangeColor.Color = Color.FromArgb(150, 50, 205, 50);
                        BottomBar.Margin = new Thickness(300*BeforeValue, 20, 0, 0);
                        var AfterValueAnimation = new DoubleAnimation()
                        {
                            From = 0,
                            To = 300 * (AfterValue - BeforeValue),
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                        };
                        AfterValueAnimation.Completed += delegate
                        {
                            AfterText.Foreground = new SolidColorBrush(GetColor(AfterValue));
                            AfterText.BeginAnimation(OpacityProperty, new DoubleAnimation()
                            {
                                From = 0,
                                To = 1,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                            });

                            ChangeText.Foreground = Brushes.LimeGreen;
                            ChangeText.BeginAnimation(OpacityProperty, new DoubleAnimation()
                            {
                                From = 0,
                                To = 1,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut}
                            });
                        };
                        BottomBar.BeginAnimation(WidthProperty, AfterValueAnimation);

                        //TopBar.Fill = TopBar.Fill.Clone();
                        //((SolidColorBrush)TopBar.Fill).BeginAnimation(SolidColorBrush.ColorProperty,
                        //    new ColorAnimation()
                        //    {
                        //        From = ((SolidColorBrush)TopBar.Fill).Color,
                        //        To = GetColor(AfterValue),
                        //        Duration = TimeSpan.FromSeconds(0.5),
                        //        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                        //    });

                        AfterLine.BeginAnimation(OpacityProperty, new DoubleAnimation()
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                        });
                        AfterLine.BeginAnimation(Line.X1Property, new DoubleAnimation()
                        {
                            From = 300 * BeforeValue,
                            To = 300 * AfterValue,
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                        });
                        AfterLine.BeginAnimation(Line.X2Property, new DoubleAnimation()
                        {
                            From = 300 * BeforeValue,
                            To = 300 * AfterValue,
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                        });
                        if (BeforeText.Margin.Left + BeforeText.ActualWidth + 5 >= 
                            300 * AfterValue - AfterText.ActualWidth / 2)
                        {
                            AfterText.Margin = new Thickness(300 * AfterValue -
                                AfterText.ActualWidth/2, -10, 0, 0);
                            AfterLine.BeginAnimation(Line.Y1Property, new DoubleAnimation()
                            {
                                From = 15,
                                To = 5,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                            });
                        }
                        else
                            AfterText.Margin = new Thickness(300 * AfterValue -
                                AfterText.ActualWidth / 2, 0, 0, 0);

                        Timer.Stop();
                    };
                    Timer.Start();
                };
                TopBar.BeginAnimation(WidthProperty, BeforeValueAnimation);
                BeforeLine.BeginAnimation(OpacityProperty, new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });
                BeforeLine.BeginAnimation(Line.X1Property, new DoubleAnimation()
                {
                    From = 0,
                    To = 300 * BeforeValue,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });
                BeforeLine.BeginAnimation(Line.X2Property, new DoubleAnimation()
                {
                    From = 0,
                    To = 300 * BeforeValue,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });
            }
            else
            {
                TopBar.Fill = new SolidColorBrush(GetColor(BeforeValue));
                var BeforeValueAnimation = new DoubleAnimation()
                {
                    From = 0,
                    To = 300 * BeforeValue,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                };
                BeforeValueAnimation.Completed += delegate
                {
                    BeforeText.Margin = new Thickness(300 * BeforeValue - BeforeText.ActualWidth / 2, 0, 0, 0);
                    BeforeText.BeginAnimation(OpacityProperty, new DoubleAnimation()
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    });

                    Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.5) };
                    Timer.Tick += delegate
                    {
                        StartChangeColor.Color = Color.FromArgb(150, 255, 0, 0);
                        EndChangeColor.Color = Color.FromArgb(0, 0, 0, 0);
                        var AfterValueAnimation = new ThicknessAnimation()
                        {
                            From = new Thickness(300 * BeforeValue, 20, 0, 0),
                            To = new Thickness(300 * AfterValue, 20, 0, 0),
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                        };
                        AfterValueAnimation.Completed += delegate
                        {
                            AfterText.Foreground = new SolidColorBrush(GetColor(AfterValue));
                            AfterText.BeginAnimation(OpacityProperty, new DoubleAnimation()
                            {
                                From = 0,
                                To = 1,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                            });

                            ChangeText.Foreground = Brushes.Red;
                            ChangeText.BeginAnimation(OpacityProperty, new DoubleAnimation()
                            {
                                From = 0,
                                To = 1,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                            });
                        };
                        BottomBar.BeginAnimation(WidthProperty, new DoubleAnimation()
                        {
                            From = 0,
                            To = 300 * (BeforeValue - AfterValue),
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseInOut}
                        });
                        BottomBar.BeginAnimation(MarginProperty, AfterValueAnimation);

                        TopBar.Fill = TopBar.Fill.Clone();
                        ((SolidColorBrush) TopBar.Fill).BeginAnimation(SolidColorBrush.ColorProperty,
                            new ColorAnimation()
                            {
                                From = ((SolidColorBrush)TopBar.Fill).Color,
                                To = GetColor(AfterValue),
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                            });
                        TopBar.BeginAnimation(WidthProperty, new DoubleAnimation()
                        {
                            From = 300 * BeforeValue,
                            To = 300 * AfterValue,
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                        });

                        AfterLine.BeginAnimation(OpacityProperty, new DoubleAnimation()
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                        });
                        AfterLine.BeginAnimation(Line.X1Property, new DoubleAnimation()
                        {
                            From = 300 * BeforeValue,
                            To = 300 * AfterValue,
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                        });
                        AfterLine.BeginAnimation(Line.X2Property, new DoubleAnimation()
                        {
                            From = 300 * BeforeValue,
                            To = 300 * AfterValue,
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                        });
                        if (BeforeText.Margin.Left - 5 <=
                            300 * AfterValue + AfterText.ActualWidth / 2)
                        {
                            AfterText.Margin = new Thickness(300 * AfterValue -
                                AfterText.ActualWidth / 2, -10, 0, 0);
                            AfterLine.BeginAnimation(Line.Y1Property, new DoubleAnimation()
                            {
                                From = 15,
                                To = 5,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                            });
                        }
                        else
                            AfterText.Margin = new Thickness(300 * AfterValue -
                                AfterText.ActualWidth / 2, 0, 0, 0);

                        Timer.Stop();
                    };
                    Timer.Start();
                };
                TopBar.BeginAnimation(WidthProperty, BeforeValueAnimation);
                BeforeLine.BeginAnimation(OpacityProperty, new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });
                BeforeLine.BeginAnimation(Line.X1Property, new DoubleAnimation()
                {
                    From = 0,
                    To = 300 * BeforeValue,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });
                BeforeLine.BeginAnimation(Line.X2Property, new DoubleAnimation()
                {
                    From = 0,
                    To = 300 * BeforeValue,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });
            }
        }

        private void ResetAnimation()
        {
            Timer?.Stop();
            Timer = null;

            TopBar.BeginAnimation(WidthProperty, null);
            BottomBar.BeginAnimation(WidthProperty, null);
            BottomBar.BeginAnimation(MarginProperty, null);

            BeforeText.BeginAnimation(OpacityProperty, null);
            AfterText.BeginAnimation(OpacityProperty, null);
            ChangeText.BeginAnimation(OpacityProperty, null);

            BeforeLine.BeginAnimation(OpacityProperty, null);
            BeforeLine.BeginAnimation(Line.X1Property, null);
            BeforeLine.BeginAnimation(Line.X2Property, null);
            AfterLine.BeginAnimation(OpacityProperty, null);
            AfterLine.BeginAnimation(Line.X1Property, null);
            AfterLine.BeginAnimation(Line.X2Property, null);
            AfterLine.BeginAnimation(Line.Y1Property, null);
        }

        private static Color GetColor(double Value)
        {
            if (Value > 0.75)
                return Colors.LimeGreen;
            if (Value > 0.5)
                return Colors.YellowGreen;
            if (Value > 0.25)
                return Colors.Orange;
            if (Value > 0)
                return Colors.Red;
            return Colors.Gray;
        }
    }
}
