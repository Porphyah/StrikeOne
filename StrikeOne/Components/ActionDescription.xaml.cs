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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StrikeOne.Components
{
    /// <summary>
    /// ActionDescription.xaml 的交互逻辑
    /// </summary>
    public partial class ActionDescription : UserControl
    {
        public ActionDescription()
        {
            InitializeComponent();
            ActionProbability.Adjust();
            ActionProbability.Init(6, false);
        }

        public void Init(string Name, BitmapImage Image, string Description, int? Probability = null)
        {
            ActionName.Text = Name;
            ActionImage.Source = Image;
            this.Description.Text = Description;
            if (Probability == null)
                ActionProbability.Visibility = Visibility.Hidden;
            else
            {
                ActionProbability.Visibility = Visibility.Visible;
                ActionProbability.SetValue(Probability.Value);
            }
        }
    }
}
