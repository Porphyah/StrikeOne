using System;
using System.Drawing.Imaging;
using System.IO;
using StrikeOne.Core;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace StrikeOne.Components
{
    /// <summary>
    /// SkillItem.xaml 的交互逻辑
    /// </summary>
    public partial class SkillItem : UserControl
    {
        public Action<Skill> SelectSyncAction { set; private get; }
        public bool IsSelected { set; get; } = false;

        public SkillItem()
        {
            InitializeComponent();
        }
        public Skill Skill { private set; get; }

        public void Init(Skill Source)
        {
            Skill = Source;

            if (Skill.Image != null)
                using (MemoryStream Stream = new MemoryStream())
                {
                    Skill.Image.Save(Stream, ImageFormat.Png);
                    BitmapImage Temp = new BitmapImage();
                    Temp.BeginInit();
                    Temp.CacheOption = BitmapCacheOption.OnLoad;
                    Temp.StreamSource = Stream;
                    Temp.EndInit();
                    SkillImage.Source = Temp;
                }
            else
                SkillImage.Source = null;
            SkillName.Text = Skill.Name;
        }


        private void OnMouseDown(object Sender, MouseButtonEventArgs E)
        {
            if (IsSelected) return;

            IsSelected = true;
            SelectSyncAction?.Invoke(this.Skill);
            Container.Background = new SolidColorBrush(Color.FromArgb(136, 30, 144, 255));
            ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimationUsingKeyFrames()
            {
                KeyFrames = new DoubleKeyFrameCollection()
                {
                    new EasingDoubleKeyFrame(0.9, TimeSpan.FromSeconds(0.2), 
                        new ExponentialEase() { EasingMode = EasingMode.EaseInOut }),
                    new EasingDoubleKeyFrame(1, TimeSpan.FromSeconds(0.4),
                        new ExponentialEase() { EasingMode = EasingMode.EaseInOut }),
                }
            });
            ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimationUsingKeyFrames()
            {
                KeyFrames = new DoubleKeyFrameCollection()
                {
                    new EasingDoubleKeyFrame(0.9, TimeSpan.FromSeconds(0.2),
                        new ExponentialEase() { EasingMode = EasingMode.EaseInOut }),
                    new EasingDoubleKeyFrame(1, TimeSpan.FromSeconds(0.4),
                        new ExponentialEase() { EasingMode = EasingMode.EaseInOut }),
                }
            });
        }

        private void OnMouseEnter(object Sender, MouseEventArgs E)
        {
            if (IsSelected) return;
            Container.Background = new SolidColorBrush(Color.FromArgb(136, 15, 72, 128));
        }
        private void OnMouseLeave(object Sender, MouseEventArgs E)
        {
            if (IsSelected) return;
            Container.Background = new SolidColorBrush(Color.FromArgb(136, 0, 0, 0));
        }

        public void Select()
        {
            IsSelected = true;
            Container.Background = new SolidColorBrush(Color.FromArgb(136, 30, 144, 255));
        }
        public void Unselect()
        {
            IsSelected = false;
            Container.Background = new SolidColorBrush(Color.FromArgb(136, 0, 0, 0));
        }
    }
}
