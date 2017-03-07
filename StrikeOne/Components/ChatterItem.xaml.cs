using System;
using System.Collections.Generic;
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
using StrikeOne.Core.Network;

namespace StrikeOne.Components
{
    /// <summary>
    /// ChatterItem.xaml 的交互逻辑
    /// </summary>
    public partial class ChatterItem : UserControl
    {
        //public double DisplayHeight { private set; get; }

        public ChatterItem()
        {
            InitializeComponent();
        }

        public void Init(string Text, Player Speaker = null, bool Host = false)
        {
            if (Speaker != null)
            {
                if (Speaker.Id == App.CurrentUser.Id)
                {
                    AvatorGrid.HorizontalAlignment = HorizontalAlignment.Right;
                    AvatorGrid.Margin = new Thickness(0, 5, 5, 0);
                    UserName.HorizontalAlignment = HorizontalAlignment.Right;
                    UserName.Margin = new Thickness(0, 3, 60, 0);
                    ConversationGrid.HorizontalAlignment = HorizontalAlignment.Right;
                    ConversationGrid.Margin = new Thickness(0, 25, 60, 3);
                    LeftTriangle.Visibility = Visibility.Hidden;
                    RightTriangle.Visibility = Visibility.Visible;
                    ConversationGridEffect.Color = Colors.DodgerBlue;
                    AvatorEllipse.Stroke = Brushes.DodgerBlue;
                }
                else
                {
                    ConversationGridEffect.Color = Colors.Gray;
                    AvatorEllipse.Stroke = Brushes.Gray;
                }
                UserName.Text = Speaker.Name;
                if (Speaker.Avator != null)
                    using (MemoryStream Stream = new MemoryStream())
                    {
                        Speaker.Avator.Save(Stream, Speaker.AvatorFormat);
                        BitmapImage Temp = new BitmapImage();
                        Temp.BeginInit();
                        Temp.CacheOption = BitmapCacheOption.OnLoad;
                        Temp.StreamSource = Stream;
                        Temp.EndInit();
                        AvatorImage.ImageSource = Temp;
                    }
                HostImg.Visibility = Host ? Visibility.Visible : Visibility.Hidden;
            }
            else
            {
                AvatorImage.ImageSource = Resources["System"] as BitmapImage;
                UserName.Text = "System";
                AvatorEllipse.Stroke = Brushes.Orange;
                ConversationGridEffect.Color = Colors.Orange;
                ConversationBox.Foreground = Brushes.OrangeRed;
                HostImg.Visibility = Visibility.Hidden;
            }

            ConversationBox.Text = Text;
            double ConversationBoxWidth = new FormattedText(Text, System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, new Typeface(ConversationBox.FontFamily.ToString()),
                ConversationBox.FontSize, ConversationBox.Foreground).WidthIncludingTrailingWhitespace;
            int LineCount = (int)(ConversationBoxWidth/530) + 1;
            ConversationGrid.Width = ConversationBoxWidth > 530 ? 530 : ConversationBoxWidth + 20;
            this.Height = 37.5 + LineCount * (ConversationBox.FontSize + 5);
            //DisplayHeight = 40 + LineCount*(ConversationBox.FontSize + 2);
        }
        public ProgressItem SystemProgressInit(string Description, double Length)
        {
            AvatorImage.ImageSource = Resources["System"] as BitmapImage;
            UserName.Text = "System";
            AvatorEllipse.Stroke = Brushes.Orange;
            ConversationGridEffect.Color = Colors.Orange;
            ConversationBox.Foreground = Brushes.OrangeRed;
            HostImg.Visibility = Visibility.Hidden;
            ConversationGrid.Children.Remove(ConversationBox);
            ProgressItem ProgressItem = new ProgressItem
            {
                Height = 50, Width = 530,
                Margin = new Thickness(0),
                Description = {Text = Description},
                ProgressBar = {Maximum = Length}
            };
            ConversationGrid.Children.Add(ProgressItem);
            this.Height = 78;
            return ProgressItem;
        }
    }
}
