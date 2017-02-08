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
    /// Tailor.xaml 的交互逻辑
    /// </summary>
    public partial class Tailor : UserControl
    {
        public Tailor()
        {
            InitializeComponent();
        }

        public bool Dragging { set; get; } = false;
        public bool Sizing { set; get; } = false;
        public Point LastPoint { set; get; }
        public Canvas ParentCanvas { set; private get; }

        public void StopAction()
        {
            Dragging = false;
            Sizing = false;
        }

        private void StartDrag(object Sender, MouseButtonEventArgs E)
        {
            Dragging = true;
            Sizing = false;
            LastPoint = E.GetPosition(ParentCanvas);
        }
        private void StartSize(object Sender, MouseButtonEventArgs E)
        {
            Dragging = false;
            Sizing = true;
            LastPoint = E.GetPosition(ParentCanvas);
        }
    }
}
