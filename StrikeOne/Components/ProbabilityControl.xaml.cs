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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StrikeOne.Components
{
    /// <summary>
    /// ProbabilityControl.xaml 的交互逻辑
    /// </summary>
    public partial class ProbabilityControl : UserControl
    {
        private List<Rectangle> Rectangles => WrapPanel.Children
            .OfType<Rectangle>().ToList();
        public int Denominator { private set; get; }
        public int Numerator { private set; get; } = 0;

        public ProbabilityControl()
        {
            InitializeComponent();
        }

        public void Init(int Denominator, bool CanSelect)
        {
            this.Denominator = Denominator;
            double Width = Denominator > 10 ? 480.0 / Denominator : 240.0 / Denominator;
            for (int i = 0; i < Denominator; i++)
            {
                int Index = i;
                Rectangle Rect = new Rectangle()
                {
                    Height = 15,
                    Width = Width,
                    Fill = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                    Effect = new DropShadowEffect()
                    {
                        ShadowDepth = 0,
                        BlurRadius = 5,
                        Color = Colors.Gray
                    }
                };
                if (CanSelect)
                    Rect.MouseDown += delegate
                    {
                        Numerator = Index + 1;

                        Color SelectColor;
                        if (Numerator /(double) Denominator <= 0.25)
                            SelectColor = Colors.Red;
                        else if (Numerator / (double)Denominator <= 0.5)
                            SelectColor = Colors.Orange;
                        else if (Numerator / (double)Denominator <= 0.75)
                            SelectColor = Colors.YellowGreen;
                        else
                            SelectColor = Colors.LimeGreen;
                        for (int j = 0; j < Denominator; j++)
                            if (j <= Index)
                            {
                                Rectangles[j].Fill = new SolidColorBrush(
                                    Color.FromArgb(128, SelectColor.R, SelectColor.G, SelectColor.B));
                                ((DropShadowEffect) Rectangles[j].Effect).Color = SelectColor;
                            }
                            else
                            {
                                Rectangles[j].Fill = new SolidColorBrush(
                                    Color.FromArgb(128, 0, 0, 0));
                                ((DropShadowEffect) Rectangles[j].Effect).Color = Colors.Gray;
                            }
                        ProbabilityText.Text = Numerator + "/" + Denominator;
                        ProbabilityText.Foreground = new SolidColorBrush(SelectColor);
                    };
                WrapPanel.Children.Add(Rect);
            }

            ProbabilityText.Text = "0/" + Denominator;
            ProbabilityText.Foreground = new SolidColorBrush(Colors.Gray);
        }
        public void SetValue(int Numerator)
        {
            this.Numerator = Numerator;

            Color SelectColor;
            if (Numerator / (double)Denominator <= 0.25)
                SelectColor = Colors.Red;
            else if (Numerator / (double)Denominator <= 0.5)
                SelectColor = Colors.Orange;
            else if (Numerator / (double)Denominator <= 0.75)
                SelectColor = Colors.YellowGreen;
            else
                SelectColor = Colors.LimeGreen;
            for (int j = 0; j < Denominator; j++)
                if (j < Numerator)
                {
                    Rectangles[j].Fill = new SolidColorBrush(
                        Color.FromArgb(128, SelectColor.R, SelectColor.G, SelectColor.B));
                    ((DropShadowEffect)Rectangles[j].Effect).Color = SelectColor;
                }
                else
                {
                    Rectangles[j].Fill = new SolidColorBrush(
                        Color.FromArgb(128, 0, 0, 0));
                    ((DropShadowEffect)Rectangles[j].Effect).Color = Colors.Gray;
                }
            ProbabilityText.Text = Numerator + "/" + Denominator;
            ProbabilityText.Foreground = new SolidColorBrush(SelectColor);
        }

    }
}
