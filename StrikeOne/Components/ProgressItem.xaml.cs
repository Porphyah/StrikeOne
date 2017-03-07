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

namespace StrikeOne.Components
{
    /// <summary>
    /// ProgressItem.xaml 的交互逻辑
    /// </summary>
    public partial class ProgressItem : UserControl
    {
        public ProgressItem()
        {
            InitializeComponent();
        }

        public void SetProgress(double CurrentLength)
        {
            double Progress = CurrentLength/ProgressBar.Maximum*100;
            ProgressText.Text = Progress.ToString("0.0") + "%";
            ProgressBar.BeginAnimation(RangeBase.ValueProperty, new DoubleAnimation()
            {
                From = ProgressBar.Value,
                To = CurrentLength,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
            });
        }
    }
}
