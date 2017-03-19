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
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using StrikeOne.Components;
using StrikeOne.Core;

namespace StrikeOne
{
    /// <summary>
    /// SkillWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SkillWindow : MetroWindow
    {
        public SkillWindow()
        {
            InitializeComponent();
        }
        public Skill SelectedSkill { private set; get; } = null;
        public bool Canceled { private set; get; } = true;

        public void Init(List<Skill> SkillList)
        {
            SkillProbability.Adjust();
            SkillProbability.Init(6, false);
            foreach (var Skill in SkillList)
            {
                var SkillItem = new SkillItem()
                { Height = 100, Width = 80 };
                SkillItem.Init(Skill);
                SkillItem.SelectSyncAction = O =>
                {
                    SelectedSkill = O;
                    foreach (var P in SkillPanel.Children.OfType<SkillItem>())
                        if (!object.ReferenceEquals(P.Skill, O)) P.Unselect();
                    SelectSkill();
                };
                SkillPanel.Children.Add(SkillItem);
            }
        }

        private void SelectSkill()
        {
            if (SelectedSkill == null)
            {
                SkillInfoGrid.Visibility = Visibility.Hidden;
                NonSkillText.Visibility = Visibility.Visible;
                AcceptButton.Visibility = Visibility.Hidden;
                return;
            }

            NonSkillText.Visibility = Visibility.Hidden;
            SkillInfoGrid.Visibility = Visibility.Visible;
            AcceptButton.Visibility = Visibility.Visible;

            if (SelectedSkill.Image != null)
                using (MemoryStream Stream = new MemoryStream())
                {
                    SelectedSkill.Image.Save(Stream, ImageFormat.Png);
                    BitmapImage Temp = new BitmapImage();
                    Temp.BeginInit();
                    Temp.CacheOption = BitmapCacheOption.OnLoad;
                    Temp.StreamSource = Stream;
                    Temp.EndInit();
                    SkillImage.Source = Temp;
                }
            else
                SkillImage.Source = null;
            SkillName.Text = SelectedSkill.Name;
            SkillProbability.SetValue(SelectedSkill.Probability);
            CountText.Text = SelectedSkill.TotalCount.ToString();
            DurationText.Text = SelectedSkill.Duration.ToString();
            CoolDownText.Text = SelectedSkill.CoolDown.ToString();
            SkillDescription.Text = SelectedSkill.Description;
        }
        public void SelectSkill(Skill Target)
        {
            if (Target == null) return;
            foreach (var P in SkillPanel.Children.OfType<SkillItem>())
                if (ReferenceEquals(P.Skill, Target)) P.Select();
            this.SelectedSkill = Target;

            NonSkillText.Visibility = Visibility.Hidden;
            SkillInfoGrid.Visibility = Visibility.Visible;
            AcceptButton.Visibility = Visibility.Visible;

            if (SelectedSkill.Image != null)
                using (MemoryStream Stream = new MemoryStream())
                {
                    SelectedSkill.Image.Save(Stream, ImageFormat.Png);
                    BitmapImage Temp = new BitmapImage();
                    Temp.BeginInit();
                    Temp.CacheOption = BitmapCacheOption.OnLoad;
                    Temp.StreamSource = Stream;
                    Temp.EndInit();
                    SkillImage.Source = Temp;
                }
            else
                SkillImage.Source = null;
            SkillName.Text = SelectedSkill.Name;
            SkillProbability.SetValue(SelectedSkill.Probability);
            CountText.Text = SelectedSkill.TotalCount == -1 ?
               "无限制" : SelectedSkill.TotalCount.ToString();
            DurationText.Text = SelectedSkill.Duration.ToString();
            CoolDownText.Text = SelectedSkill.CoolDown.ToString();
            SkillDescription.Text = SelectedSkill.Description;
        }

        private void Cancel_Click(object Sender, RoutedEventArgs E)
        {
            Canceled = true;
            this.Close();
        }

        private void Accept_Click(object Sender, RoutedEventArgs E)
        {
            Canceled = false;
            this.Close();
        }
    }
}
