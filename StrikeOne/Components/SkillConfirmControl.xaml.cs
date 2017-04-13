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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StrikeOne.Core;

namespace StrikeOne.Components
{
    /// <summary>
    /// SkillConfirmControl.xaml 的交互逻辑
    /// </summary>
    public partial class SkillConfirmControl : UserControl
    {
        public SkillConfirmControl()
        {
            InitializeComponent();
            SkillProbability.Adjust();
            SkillProbability.Init(6, false);
        }
        public Action ConfirmAction { set; private get; }
        public Action CancelAction { set; private get; }

        public void Init(Skill Skill)
        {
            if (Skill.Image != null)
                using (MemoryStream Stream = new MemoryStream())
                {
                    Skill.Image.Save(Stream, ImageFormat.Png);
                    BitmapImage Temp = new BitmapImage();
                    Temp.BeginInit();
                    Temp.CacheOption = BitmapCacheOption.OnLoad;
                    Temp.StreamSource = Stream;
                    Temp.EndInit();
                    SkillImg.Source = Temp;
                }
            else
                SkillImg.Source = null;
            SkillName.Text = Skill.Name;
            SkillProbability.SetValue(Skill.Probability);
            CountText.Text = Skill.RemainedCount + "/" + Skill.TotalCount;
            DurationText.Text = Skill.Duration.ToString();
            CoolDownText.Text = Skill.CoolDown.ToString();
            DescriptionText.Text = Skill.Description;
        }

        private void Confirm_Click(object Sender, RoutedEventArgs E)
        {
            ConfirmAction?.Invoke();
        }
        private void Cancel_Click(object Sender, RoutedEventArgs E)
        {
            CancelAction?.Invoke();
        }
    }
}
