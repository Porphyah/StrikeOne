using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace StrikeOne
{
    /// <summary>
    /// EditAiSkillWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditAiSkillWindow : MetroWindow
    {
        public bool Canceled { private set; get; } = true;
        public string ConditionScript
        {
            set
            {
                ConditionScriptBox.Document.Blocks.Clear();
                ConditionScriptBox.Document.Blocks.Add(new Paragraph(
                    new Run(value)));
            }
            get
            {
                return ConditionScriptBox.Document.Blocks.OfType<Paragraph>()
                    .SelectMany(O => O.Inlines.OfType<Run>().Select(P => P.Text))
                    .Aggregate((A, B) => A + "\n" + B);
            }
        }
        public string TargetScript
        {
            set
            {
                TargetScriptBox.Document.Blocks.Clear();
                TargetScriptBox.Document.Blocks.Add(new Paragraph(
                    new Run(value)));
            }
            get
            {
                return TargetScriptBox.Document.Blocks.OfType<Paragraph>()
                    .SelectMany(O => O.Inlines.OfType<Run>().Select(P => P.Text))
                    .Aggregate((A, B) => A + "\n" + B);
            }
        }

        public EditAiSkillWindow()
        {
            InitializeComponent();
        }

        private void Confirm_Click(object Sender, RoutedEventArgs E)
        {
            Canceled = false;
            this.Close();
        }
        private void Cancel_Click(object Sender, RoutedEventArgs E)
        {
            Canceled = true;
            this.Close();
        }

        private void OnWindowClosing(object Sender, CancelEventArgs E)
        {
            if (Canceled && MessageBox.Show("确实要放弃当前所做的改动吗？", "设置技能使用脚本",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                E.Cancel = true;
        }
    }
}
