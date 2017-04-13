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

namespace StrikeOne.Components
{
    /// <summary>
    /// NavigationControl.xaml 的交互逻辑
    /// </summary>
    public partial class NavigationControl : UserControl
    {
        public NavigationControl()
        {
            InitializeComponent();
        }

        public int Index { set; get; }
        public Grid RelatedGrid { set; get; }
        public bool IsSelected { private set; get; }
        public bool IsViewed { private set; get; } = false;
        public Action SelectAction { set; private get; }

        public void Select()
        {
            IsViewed = true;
            SelectAction?.Invoke();
            IsSelected = true;
            
            Ellipse.Fill = Ellipse.Fill.Clone();
            ((SolidColorBrush)Ellipse.Fill).BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation()
                {
                    From = ((SolidColorBrush)Ellipse.Fill).Color,
                    To = Colors.White,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });

            Ellipse.Stroke = Ellipse.Stroke.Clone();
            ((SolidColorBrush)Ellipse.Stroke).BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation()
                {
                    From = ((SolidColorBrush)Ellipse.Stroke).Color,
                    To = Colors.DodgerBlue,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });
        }

        public void Unselect()
        {
            IsSelected = false;

            Ellipse.Fill = Ellipse.Fill.Clone();
            ((SolidColorBrush)Ellipse.Fill).BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation()
                {
                    From = ((SolidColorBrush)Ellipse.Fill).Color,
                    To = Colors.DodgerBlue,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                });

            Ellipse.Stroke = Ellipse.Stroke.Clone();
            ((SolidColorBrush)Ellipse.Stroke).BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation()
                {
                    From = ((SolidColorBrush)Ellipse.Stroke).Color,
                    To = Colors.Transparent,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                });
        }

        private void OnMouseDown(object Sender, MouseButtonEventArgs E)
        {
            if (!IsSelected) Select();
        }
    }
}
