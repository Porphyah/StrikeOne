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
using StrikeOne.Core;

namespace StrikeOne.Components
{
    /// <summary>
    /// LuckGraph.xaml 的交互逻辑
    /// </summary>
    public partial class LuckGraph : UserControl
    {
        public LuckGraph()
        {
            InitializeComponent();
        }
        private List<DiceRoll> Source { set; get; }
        private List<double> Data { set; get; }

        public void Init(List<DiceRoll> Source)
        {
            this.Source = Source;
            this.Data = new List<double>();
            Data.Add(0);
            for (int i = 0; i < Source.Count; i++)
            {
                var Temp = Source.Take(i + 1).Where(P => P.Probability.Key != P.Probability.Value).ToList();
                double Ratio = Temp.Where(P => P.Success).Sum(P => (P.Probability.Value - P.Probability.Key) / (double)P.Probability.Value)
                     / Temp.Sum(P => (P.Probability.Value - P.Probability.Key) / (double)P.Probability.Value);
                Data.Add(Ratio);
            }

            double Interval = 325.0 / Source.Count;
            int Count = 1;
            PathLine.Data = new PathGeometry(
                new PathFigureCollection()
                {
                    new PathFigure()
                    {
                        StartPoint = new Point(0, 0),
                        IsFilled = false,
                        Segments = { new LineSegment(new Point(0, 145), false) }
                    },
                    new PathFigure()
                    {
                        StartPoint = new Point(0, 145),
                        Segments = new PathSegmentCollection(
                            Source.Select(O => new LineSegment(
                                new Point(Interval*Count,
                                    145*(1 - Data[Count++])), true))
                            .Union(new List<LineSegment>()
                            {
                                new LineSegment(new Point(325, 145), false),
                                new LineSegment(new Point(0, 145), false),
                            }))
                    },
                });
            StrokeColor.Color = GetColor(Data.Max());
            FillColor.Color = GetColor(Data.Max());
        }
        public void SetColor(Color Color)
        {
            DisplayLine.Stroke = new SolidColorBrush(Color);
            DisplayCircle.Fill = new SolidColorBrush(Color);
        }
        private Color GetColor(double Ratio)
        {
            if (Ratio <= 0.25)
                return Colors.Red;
            if (Ratio <= 0.5)
                return Colors.DarkOrange;
            if (Ratio <= 0.75)
                return Colors.YellowGreen;
            else
                return Colors.LimeGreen;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var Canvas = sender as Canvas;
            int Index = (int)Math.Round(Data.Count * (e.GetPosition(Canvas).X / Canvas.ActualWidth));
            if (Index >= Data.Count) Index = Data.Count - 1;
            Point Location = new Point(Index*(Canvas.ActualWidth/Data.Count), 
                Canvas.ActualHeight * (1 - Data[Index]));

            DisplayLine.X1 = Location.X;
            DisplayLine.X2 = Location.X;
            Canvas.SetLeft(DisplayCircle, Location.X - 5);
            Canvas.SetTop(DisplayCircle, Location.Y - 5);
            Canvas.ToolTip = Index == 0 ? "初始化战斗：0%" :
               GetRollTypeName(Source[Index - 1].Type) + (Source[Index - 1].Success ? "成功" : "失败") + "：" + (Data[Index]*100).ToString("0.0") + "%";
        }

        private void OnMouseEnter(object Sender, MouseEventArgs E)
        {
            DisplayLine.Visibility = Visibility.Visible;
            DisplayCircle.Visibility = Visibility.Visible;
        }

        private void OnMouseLeave(object Sender, MouseEventArgs E)
        {
            DisplayLine.Visibility = Visibility.Hidden;
            DisplayCircle.Visibility = Visibility.Hidden;
        }

        private string GetRollTypeName(DiceRoll.RollType RollType)
        {
            switch (RollType)
            {
                case DiceRoll.RollType.Attack: return "攻击";
                case DiceRoll.RollType.Defense: return "防御";
                case DiceRoll.RollType.Counter: return "反击";
                case DiceRoll.RollType.UseSkill: return "使用技能";
            }
            return null;
        }
    }
}
