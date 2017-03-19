using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// DiceControl.xaml 的交互逻辑
    /// </summary>
    public partial class DiceControl : UserControl
    {
        public DiceControl()
        {
            InitializeComponent();
        }
        public int Result { private set; get; }
        public int Expected { set; get; }
        public bool Success { private set; get; }
        public Action EndAction { set; private get; }
        private DispatcherTimer DiceTimer { set; get; }
        private const double RollSeconds = 3;

        public void Collapse()
        {
            Height = 0;
            Width = 0;
            ContentGrid.Visibility = Visibility.Hidden;
            ContentGrid.Opacity = 0;
        }
        public void Expand()
        {
            DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.3) };
            Timer.Tick += delegate
            {
                ContentGrid.Visibility = Visibility.Visible;
                DoubleAnimation OpacityAnimation = new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseOut}
                };
                OpacityAnimation.Completed += delegate { Begin(); };
                ContentGrid.BeginAnimation(OpacityProperty, OpacityAnimation);
                Timer.Stop();
            };
            this.BeginAnimation(HeightProperty, new DoubleAnimation()
            {
                From = 0,
                To = 200,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            });
            this.BeginAnimation(WidthProperty, new DoubleAnimation()
            {
                From = 0,
                To = 200,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            });
            Timer.Start();
        }
        private void Begin()
        {
            Random Rand = new Random((int)(System.DateTime.Now.Ticks & 0xffffffffL) | (int)(System.DateTime.Now.Ticks >> 32));
            DiceTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.25) };
            double Seconds = 0.0;
            DiceTimer.Tick += delegate
            {
                Seconds += 0.25;
                if (Seconds >= RollSeconds)
                {
                    Image FinalDiceImage = new Image()
                    {
                        Height = 100,
                        Width = 100,
                        RenderTransform = new ScaleTransform()
                        {
                            CenterX = 50,
                            CenterY = 50,
                            ScaleX = 0,
                            ScaleY = 0
                        },
                    };
                    Result = Rand.Next(6);
                    switch (Result)
                    {
                        case 0:
                            FinalDiceImage.Source = Resources["One"] as BitmapImage;
                            break;
                        case 1:
                            FinalDiceImage.Source = Resources["Two"] as BitmapImage;
                            break;
                        case 2:
                            FinalDiceImage.Source = Resources["Three"] as BitmapImage;
                            break;
                        case 3:
                            FinalDiceImage.Source = Resources["Four"] as BitmapImage;
                            break;
                        case 4:
                            FinalDiceImage.Source = Resources["Five"] as BitmapImage;
                            break;
                        case 5:
                            FinalDiceImage.Source = Resources["Six"] as BitmapImage;
                            break;
                    }

                    Storyboard FinalStoryboard = new Storyboard();
                    DoubleAnimation FinalTopAnimation = new DoubleAnimation()
                    {
                        From = -25,
                        To = 50,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    };
                    Storyboard.SetTarget(FinalTopAnimation, FinalDiceImage);
                    Storyboard.SetTargetProperty(FinalTopAnimation, new PropertyPath("(Canvas.Top)"));
                    FinalStoryboard.Children.Add(FinalTopAnimation);
                    DoubleAnimationUsingKeyFrames FinalOpacityAnimation = new DoubleAnimationUsingKeyFrames()
                    {
                        KeyFrames =
                        {
                            new EasingDoubleKeyFrame(0, TimeSpan.FromSeconds(0)),
                            new EasingDoubleKeyFrame(1, TimeSpan.FromSeconds(0.5), new ExponentialEase() { EasingMode = EasingMode.EaseOut }),
                        }
                    };
                    Storyboard.SetTarget(FinalOpacityAnimation, FinalDiceImage);
                    Storyboard.SetTargetProperty(FinalOpacityAnimation, new PropertyPath("Opacity"));
                    FinalStoryboard.Children.Add(FinalOpacityAnimation);
                    DoubleAnimationUsingKeyFrames FinalScaleXAnimation = new DoubleAnimationUsingKeyFrames()
                    {
                        KeyFrames =
                        {
                            new EasingDoubleKeyFrame(0, TimeSpan.FromSeconds(0)),
                            new EasingDoubleKeyFrame(1, TimeSpan.FromSeconds(0.5), new ExponentialEase() { EasingMode = EasingMode.EaseOut }),
                        }
                    };
                    Storyboard.SetTarget(FinalScaleXAnimation, FinalDiceImage);
                    Storyboard.SetTargetProperty(FinalScaleXAnimation, new PropertyPath("RenderTransform.(ScaleTransform.ScaleX)"));
                    FinalStoryboard.Children.Add(FinalScaleXAnimation);
                    DoubleAnimationUsingKeyFrames FinalScaleYAnimation = new DoubleAnimationUsingKeyFrames()
                    {
                        KeyFrames =
                        {
                            new EasingDoubleKeyFrame(0, TimeSpan.FromSeconds(0)),
                            new EasingDoubleKeyFrame(1, TimeSpan.FromSeconds(0.5), new ExponentialEase() { EasingMode = EasingMode.EaseOut }),
                        }
                    };
                    Storyboard.SetTarget(FinalScaleYAnimation, FinalDiceImage);
                    Storyboard.SetTargetProperty(FinalScaleYAnimation, new PropertyPath("RenderTransform.(ScaleTransform.ScaleY)"));
                    FinalStoryboard.Children.Add(FinalScaleYAnimation);

                    FinalStoryboard.Completed += delegate
                    {
                        DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                        Timer.Tick += delegate { End(FinalDiceImage); Timer.Stop(); };
                        Timer.Start();
                    };

                    Canvas.SetLeft(FinalDiceImage, 50);
                    Canvas.SetTop(FinalDiceImage, -25);
                    DiceCanvas.Children.Add(FinalDiceImage);
                    FinalStoryboard.Begin();

                    DiceTimer.Stop();
                    return;
                }

                ScaleTransform ScaleTransform = new ScaleTransform()
                {
                    CenterX = 50,
                    CenterY = 50,
                    ScaleX = 0,
                    ScaleY = 0
                };
                Image DiceImage = new Image()
                {
                    Height = 100, Width = 100,
                    RenderTransform = ScaleTransform,
                };
                switch (Rand.Next(6))
                {
                    case 0:
                        DiceImage.Source = Resources["One"] as BitmapImage;
                        break;
                    case 1:
                        DiceImage.Source = Resources["Two"] as BitmapImage;
                        break;
                    case 2:
                        DiceImage.Source = Resources["Three"] as BitmapImage;
                        break;
                    case 3:
                        DiceImage.Source = Resources["Four"] as BitmapImage;
                        break;
                    case 4:
                        DiceImage.Source = Resources["Five"] as BitmapImage;
                        break;
                    case 5:
                        DiceImage.Source = Resources["Six"] as BitmapImage;
                        break;
                }

                Storyboard FishEyeStoryboard = new Storyboard();
                DoubleAnimation TopAnimation = new DoubleAnimation()
                {
                    From = -25,
                    To = 125,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                };
                Storyboard.SetTarget(TopAnimation, DiceImage);
                Storyboard.SetTargetProperty(TopAnimation, new PropertyPath("(Canvas.Top)"));
                FishEyeStoryboard.Children.Add(TopAnimation);
                DoubleAnimationUsingKeyFrames OpacityAnimation = new DoubleAnimationUsingKeyFrames()
                {
                    KeyFrames =
                    {
                        new EasingDoubleKeyFrame(0, TimeSpan.FromSeconds(0)),
                        new EasingDoubleKeyFrame(1, TimeSpan.FromSeconds(0.25), new ExponentialEase() { EasingMode = EasingMode.EaseOut }),
                        new EasingDoubleKeyFrame(0, TimeSpan.FromSeconds(0.5), new ExponentialEase() { EasingMode = EasingMode.EaseIn })
                    }
                };
                Storyboard.SetTarget(OpacityAnimation, DiceImage);
                Storyboard.SetTargetProperty(OpacityAnimation, new PropertyPath("Opacity"));
                FishEyeStoryboard.Children.Add(OpacityAnimation);
                DoubleAnimationUsingKeyFrames ScaleXAnimation = new DoubleAnimationUsingKeyFrames()
                {
                    KeyFrames =
                    {
                        new EasingDoubleKeyFrame(0, TimeSpan.FromSeconds(0)),
                        new EasingDoubleKeyFrame(1, TimeSpan.FromSeconds(0.25), new ExponentialEase() { EasingMode = EasingMode.EaseOut }),
                        new EasingDoubleKeyFrame(0, TimeSpan.FromSeconds(0.5), new ExponentialEase() { EasingMode = EasingMode.EaseIn })
                    }
                };
                Storyboard.SetTarget(ScaleXAnimation, DiceImage);
                Storyboard.SetTargetProperty(ScaleXAnimation, new PropertyPath("RenderTransform.(ScaleTransform.ScaleX)"));
                FishEyeStoryboard.Children.Add(ScaleXAnimation);
                DoubleAnimationUsingKeyFrames ScaleYAnimation = new DoubleAnimationUsingKeyFrames()
                {
                    KeyFrames =
                    {
                        new EasingDoubleKeyFrame(0, TimeSpan.FromSeconds(0)),
                        new EasingDoubleKeyFrame(1, TimeSpan.FromSeconds(0.25), new ExponentialEase() { EasingMode = EasingMode.EaseOut }),
                        new EasingDoubleKeyFrame(0, TimeSpan.FromSeconds(0.5), new ExponentialEase() { EasingMode = EasingMode.EaseIn })
                    }
                };
                Storyboard.SetTarget(ScaleYAnimation, DiceImage);
                Storyboard.SetTargetProperty(ScaleYAnimation, new PropertyPath("RenderTransform.(ScaleTransform.ScaleY)"));
                FishEyeStoryboard.Children.Add(ScaleYAnimation);
                FishEyeStoryboard.Completed += (Sender, E) =>
                {
                    DiceCanvas.Children.Remove(DiceImage);
                };

                Canvas.SetLeft(DiceImage, 50);
                Canvas.SetTop(DiceImage, -25);
                DiceCanvas.Children.Add(DiceImage);
                FishEyeStoryboard.Begin();
            };

            CountProgressBar.BeginAnimation(RangeBase.ValueProperty, new DoubleAnimation()
            {
                From = 0,
                To = 100,
                Duration = TimeSpan.FromSeconds(RollSeconds)
            });

            DiceTimer.Start();
        }

        private void End(Image FinalDiceImage)
        {
            Storyboard FadeStoryboard = new Storyboard();
            DoubleAnimation DiceImage_OpacityAnimation = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTargetProperty(DiceImage_OpacityAnimation, new PropertyPath("Opacity"));
            FadeStoryboard.Children.Add(DiceImage_OpacityAnimation);
            DoubleAnimationUsingKeyFrames DiceImage_ScaleXAnimation = new DoubleAnimationUsingKeyFrames()
            {
                KeyFrames =
                {
                    new EasingDoubleKeyFrame(0, TimeSpan.FromSeconds(0.5), new ExponentialEase() { EasingMode = EasingMode.EaseIn })
                }
            };
            Storyboard.SetTarget(DiceImage_ScaleXAnimation, FinalDiceImage);
            Storyboard.SetTargetProperty(DiceImage_ScaleXAnimation, new PropertyPath("RenderTransform.(ScaleTransform.ScaleX)"));
            FadeStoryboard.Children.Add(DiceImage_ScaleXAnimation);
            DoubleAnimationUsingKeyFrames DiceImage_ScaleYAnimation = new DoubleAnimationUsingKeyFrames()
            {
                KeyFrames =
                {
                    new EasingDoubleKeyFrame(0, TimeSpan.FromSeconds(0.5), new ExponentialEase() { EasingMode = EasingMode.EaseIn })
                }
            };
            Storyboard.SetTarget(DiceImage_ScaleYAnimation, FinalDiceImage);
            Storyboard.SetTargetProperty(DiceImage_ScaleYAnimation, new PropertyPath("RenderTransform.(ScaleTransform.ScaleY)"));
            FadeStoryboard.Children.Add(DiceImage_ScaleYAnimation);

            Success = Result + 1 > Expected;
            if (Success)
            {
                EllipseBackground.Fill = EllipseBackground.Fill.Clone();
                ((SolidColorBrush)EllipseBackground.Fill).BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation()
                {
                    From = ((SolidColorBrush)EllipseBackground.Fill).Color,
                    To = Colors.DarkGreen,
                    Duration = TimeSpan.FromSeconds(1),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                });

                DoubleAnimation CountResetAnimation = new DoubleAnimation()
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(1),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                };
                CountResetAnimation.Completed += delegate
                {
                    ((SolidColorBrush)CountProgressBar.Foreground).BeginAnimation(SolidColorBrush.ColorProperty, null);
                    CountProgressBar.BeginAnimation(RangeBase.ValueProperty, null);
                    CountProgressBar.BeginAnimation(OpacityProperty, null);
                    ((SolidColorBrush)EllipseBackground.Fill).BeginAnimation(SolidColorBrush.ColorProperty, null);

                    ((SolidColorBrush)EllipseBackground.Fill).Color = Colors.DarkGreen;
                    CountProgressBar.Value = 0;
                    CountProgressBar.Opacity = 1;
                    CountProgressBar.Foreground = Brushes.White;
                };
                CountProgressBar.Foreground = CountProgressBar.Foreground.Clone();
                ((SolidColorBrush)CountProgressBar.Foreground).BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation()
                    {
                        From = ((SolidColorBrush)CountProgressBar.Foreground).Color,
                        To = Colors.LimeGreen,
                        Duration = TimeSpan.FromSeconds(1),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                    });
                CountProgressBar.BeginAnimation(OpacityProperty, CountResetAnimation);

                FadeStoryboard.Completed += (Sender, E) =>
                {
                    DiceCanvas.Children.Remove(FinalDiceImage);
                    DiceText.Text = "Succeeded";
                    DiceText.Visibility = Visibility.Visible;
                    DiceText.BeginAnimation(OpacityProperty, new DoubleAnimation()
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    });
                };
            }
            else
            {
                EllipseBackground.Fill = EllipseBackground.Fill.Clone();
                ((SolidColorBrush)EllipseBackground.Fill).BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation()
                {
                    From = ((SolidColorBrush)EllipseBackground.Fill).Color,
                    To = Colors.DarkRed,
                    Duration = TimeSpan.FromSeconds(1),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                });

                DoubleAnimation CountResetAnimation = new DoubleAnimation()
                {
                    From = 100,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(1),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                };
                CountResetAnimation.Completed += delegate
                {
                    ((SolidColorBrush)CountProgressBar.Foreground).BeginAnimation(SolidColorBrush.ColorProperty, null);
                    CountProgressBar.BeginAnimation(RangeBase.ValueProperty, null);
                    CountProgressBar.BeginAnimation(OpacityProperty, null);
                    ((SolidColorBrush)EllipseBackground.Fill).BeginAnimation(SolidColorBrush.ColorProperty, null);

                    ((SolidColorBrush)EllipseBackground.Fill).Color = Colors.DarkRed;
                    CountProgressBar.Value = 0;
                    CountProgressBar.Opacity = 1;
                    CountProgressBar.Foreground = Brushes.White;
                };
                CountProgressBar.Foreground = CountProgressBar.Foreground.Clone();
                ((SolidColorBrush)CountProgressBar.Foreground).BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation()
                    {
                        From = ((SolidColorBrush)CountProgressBar.Foreground).Color,
                        To = Colors.Red,
                        Duration = TimeSpan.FromSeconds(1),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
                    });
                CountProgressBar.BeginAnimation(RangeBase.ValueProperty, CountResetAnimation);

                FadeStoryboard.Completed += (Sender, E) =>
                {
                    DiceCanvas.Children.Remove(FinalDiceImage);
                    DiceText.Text = "Failed";
                    DiceText.Visibility = Visibility.Visible;
                    DiceText.BeginAnimation(OpacityProperty, new DoubleAnimation()
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                    });
                };
            }

            FadeStoryboard.Begin(FinalDiceImage);
        }
    }
}
