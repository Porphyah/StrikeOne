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
    /// SkillSelector.xaml 的交互逻辑
    /// </summary>
    public partial class SkillSelector : UserControl
    {
        public SkillSelector()
        {
            InitializeComponent();
        }
        public Skill SelectedSkill { private set; get; }

        public void EnableSelect(bool Enabled)
        {
            SkillButton.Visibility = Enabled ? Visibility.Visible : Visibility.Hidden;
        }

        private void SkillSelect_Click(object Sender, RoutedEventArgs E)
        {
            SkillWindow Dialog = new SkillWindow();
            Dialog.Init(App.SkillList);
            Dialog.SelectSkill(SelectedSkill);
            Dialog.ShowDialog();
            if (Dialog.Canceled) return;

            this.SelectedSkill = Dialog.SelectedSkill.Clone() as Skill;

            if (SelectedSkill.Image != null)
                using (MemoryStream Stream = new MemoryStream())
                {
                    SelectedSkill.Image.Save(Stream, ImageFormat.Png);
                    BitmapImage Temp = new BitmapImage();
                    Temp.BeginInit();
                    Temp.CacheOption = BitmapCacheOption.OnLoad;
                    Temp.StreamSource = Stream;
                    Temp.EndInit();
                    SkillImg.Source = Temp;
                }
            else
                SkillImg.Source = null;
            SkillName.Text = SelectedSkill.Name;
        }

        public void Select(Skill Target)
        {
            if (Target == null) return;
            SelectedSkill = Target.Clone() as Skill;

            if (SelectedSkill.Image != null)
                using (MemoryStream Stream = new MemoryStream())
                {
                    SelectedSkill.Image.Save(Stream, ImageFormat.Png);
                    BitmapImage Temp = new BitmapImage();
                    Temp.BeginInit();
                    Temp.CacheOption = BitmapCacheOption.OnLoad;
                    Temp.StreamSource = Stream;
                    Temp.EndInit();
                    SkillImg.Source = Temp;
                }
            else
                SkillImg.Source = null;
            SkillName.Text = SelectedSkill.Name;
        }
    }
}
