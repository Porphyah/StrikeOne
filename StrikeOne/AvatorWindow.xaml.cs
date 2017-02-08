using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace StrikeOne
{
    /// <summary>
    /// AvatorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AvatorWindow : MetroWindow
    {
        public bool Canceled { private set; get; } = true;
        public System.Drawing.Image Source { set; get; }
        public System.Drawing.Image Target { set; get; }
        public ImageFormat ImageFormat { private set; get; }

        public AvatorWindow()
        {
            InitializeComponent();
        }

        private void Upload_Click(object Sender, RoutedEventArgs E)
        {
            Canceled = false;
            this.Close();
        }
        private void Cancel_Click(object Sender, RoutedEventArgs E)
        {
            Canceled = true;
            this.Close();
        }

        public void Init(string ImagePath)
        {
            Source = System.Drawing.Image.FromFile(ImagePath);
            Tailor.ParentCanvas = Canvas;

            using (var Stream = new MemoryStream())
            {
                if (ImagePath.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase))
                    ImageFormat = ImageFormat.Jpeg;
                else if (ImagePath.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase))
                    ImageFormat = ImageFormat.Png;
                else if (ImagePath.EndsWith(".tif", StringComparison.CurrentCultureIgnoreCase))
                    ImageFormat = ImageFormat.Tiff;
                else if(ImagePath.EndsWith(".bmp", StringComparison.CurrentCultureIgnoreCase))
                    ImageFormat = ImageFormat.Bmp;

                Stream.Seek(0, SeekOrigin.Begin);
                Source.Save(Stream, ImageFormat);
                BitmapImage ImageSource = new BitmapImage();
                ImageSource.BeginInit();
                ImageSource.CacheOption = BitmapCacheOption.OnLoad;
                ImageSource.StreamSource = Stream;
                ImageSource.EndInit();
                SourceImage.Source = ImageSource;
            }
        }

        private void OnMouseMove(object Sender, MouseEventArgs E)
        {
            Point MousePoint = E.GetPosition(Canvas);
            if (Tailor.Dragging)
            {
                double Left = Canvas.GetLeft(Tailor) + (MousePoint.X - Tailor.LastPoint.X);
                double Top = Canvas.GetTop(Tailor) + (MousePoint.Y - Tailor.LastPoint.Y);

                if (Left < Canvas.GetLeft(SourceImage))
                    Left = Canvas.GetLeft(SourceImage);
                else if (Left + Canvas.GetLeft(Tailor) > Canvas.GetLeft(SourceImage) + SourceImage.Width)
                    Left = Canvas.GetLeft(SourceImage) + SourceImage.Width - Canvas.GetLeft(Tailor);

                if (Top < Canvas.GetTop(SourceImage))
                    Top = Canvas.GetTop(SourceImage);
                else if (Top + Canvas.GetTop(Tailor) > Canvas.GetTop(SourceImage) + SourceImage.Height)
                    Top = Canvas.GetTop(SourceImage) + SourceImage.Height - Canvas.GetTop(Tailor);

                Canvas.SetLeft(Tailor, Left);
                Canvas.SetTop(Tailor, Top);

                FocusEllipse.Center = new Point(Left + Tailor.Width / 2, Top + Tailor.Height / 2);

                Tailor.LastPoint = MousePoint;
            }
            else if (Tailor.Sizing)
            {
                double SizeWidth = Tailor.Width + (MousePoint.X - Tailor.LastPoint.X);
                double SizeHeight = Tailor.Height + (MousePoint.Y - Tailor.LastPoint.Y);

                if (SizeWidth < 0)
                    SizeWidth = 0;
                else if (Canvas.GetLeft(Tailor) + SizeWidth > Canvas.GetLeft(SourceImage) + SourceImage.Width)
                    SizeWidth = Canvas.GetLeft(SourceImage) + SourceImage.Width - Canvas.GetLeft(Tailor);

                if (SizeHeight < 0)
                    SizeHeight = 0;
                else if (Canvas.GetTop(Tailor) + SizeHeight > Canvas.GetTop(SourceImage) + SourceImage.Height)
                    SizeHeight = Canvas.GetTop(SourceImage) + SourceImage.Height - Canvas.GetTop(Tailor);

                if (SizeHeight > SizeWidth)
                    SizeWidth = SizeHeight;
                else
                    SizeHeight = SizeWidth;

                Tailor.Width = SizeWidth;
                Tailor.Height = SizeHeight;

                FocusEllipse.Center = new Point(Canvas.GetLeft(Tailor) + Tailor.Width / 2, 
                    Canvas.GetTop(Tailor) + Tailor.Height / 2);
                FocusEllipse.RadiusX = SizeWidth/2;
                FocusEllipse.RadiusY = SizeHeight/2;

                Tailor.LastPoint = MousePoint;
            }
        }

        private void RefreshAvator()
        {
            double TailHeight = Source.Height*Tailor.Height/SourceImage.Height;
            double TailWidth = Source.Width*Tailor.Width/SourceImage.Width;
            double TailLeft = Source.Width*(Canvas.GetLeft(Tailor) - Canvas.GetLeft(SourceImage))/SourceImage.Width;
            double TailTop = Source.Height*(Canvas.GetTop(Tailor) - Canvas.GetTop(SourceImage))/SourceImage.Height;

            Target = new Bitmap((int)TailWidth, (int)TailHeight); //目标图
            var Graphic = Graphics.FromImage(Target);
            Graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            Graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            Graphic.Clear(System.Drawing.Color.Transparent);

            var SrcRect = new System.Drawing.Rectangle
                ((int)TailLeft, (int)TailTop, 
                (int)TailWidth, (int)TailHeight);
            var DestRect = new System.Drawing.Rectangle
                (0, 0, (int)TailWidth, (int)TailHeight);

            Graphic.DrawImage(Source, DestRect, SrcRect, GraphicsUnit.Pixel);
            Graphic.Save();
            Graphic.Dispose();

            using (MemoryStream Stream = new MemoryStream())
            {
                Target.Save(Stream, ImageFormat);
                BitmapImage Temp = new BitmapImage();
                Temp.BeginInit();
                Temp.CacheOption = BitmapCacheOption.OnLoad;
                Temp.StreamSource = Stream;
                Temp.EndInit();
                AvatorImage.ImageSource = Temp;
            }

            SizeText.Text = "尺寸：" + Target.Width + "×" + Target.Height;
        }

        private void OnMouseUp(object Sender, MouseButtonEventArgs E)
        {
            if (!Tailor.Dragging && !Tailor.Sizing) return;
            Tailor.StopAction();
            RefreshAvator();
        }
        private void OnMouseLeave(object Sender, MouseEventArgs E)
        {
            if (!Tailor.Dragging && !Tailor.Sizing) return;
            Tailor.StopAction();
            RefreshAvator();
        }

        private void WindowLoaded(object Sender, RoutedEventArgs E)
        {
            if (SourceImage.Source.Height > SourceImage.Source.Width)
            {
                if (SourceImage.Source.Height > Canvas.ActualHeight)
                {
                    SourceImage.Width = SourceImage.Source.Width * Canvas.ActualHeight / SourceImage.Source.Height;
                    SourceImage.Height = Canvas.ActualHeight;
                    Canvas.SetLeft(SourceImage, (Canvas.ActualWidth - SourceImage.Width) / 2);
                    Canvas.SetTop(SourceImage, 0);
                }
                else
                {
                    SourceImage.Width = SourceImage.Source.Width;
                    SourceImage.Height = SourceImage.Source.Height;
                    Canvas.SetLeft(SourceImage, (Canvas.ActualWidth - SourceImage.Source.Width) / 2);
                    Canvas.SetTop(SourceImage, (Canvas.ActualHeight - SourceImage.Source.Height) / 2);
                }
            }
            else if (SourceImage.Source.Width > SourceImage.Source.Height)
            {
                if (SourceImage.Source.Width > Canvas.ActualWidth)
                {
                    SourceImage.Height = SourceImage.Source.Height * Canvas.ActualWidth / SourceImage.Source.Width;
                    SourceImage.Width = Canvas.ActualWidth;
                    Canvas.SetLeft(SourceImage, 0);
                    Canvas.SetTop(SourceImage, (Canvas.ActualHeight - SourceImage.Height) / 2);
                }
                else
                {
                    SourceImage.Width = SourceImage.Source.Width;
                    SourceImage.Height = SourceImage.Source.Height;
                    Canvas.SetLeft(SourceImage, (Canvas.ActualWidth - SourceImage.Source.Width) / 2);
                    Canvas.SetTop(SourceImage, (Canvas.ActualHeight - SourceImage.Source.Height) / 2);
                }
            }
            else
            {
                SourceImage.Width = Canvas.ActualWidth;
                SourceImage.Height = Canvas.ActualHeight;
                Canvas.SetLeft(SourceImage, 0);
                Canvas.SetTop(SourceImage, 0);
            }

            if (SourceImage.Height > Source.Width)
            {
                if (SourceImage.Width >= 50)
                {
                    Tailor.Height = Tailor.Width = 50;
                    Canvas.SetLeft(Tailor, (Canvas.ActualWidth - 50) / 2);
                    Canvas.SetTop(Tailor, (Canvas.ActualHeight - 50) / 2);
                    FocusEllipse.Center = new Point(Canvas.ActualWidth / 2, Canvas.ActualHeight / 2);
                    FocusEllipse.RadiusX = 25;
                    FocusEllipse.RadiusY = 25;
                }
                else
                {
                    Tailor.Height = Tailor.Width = SourceImage.Width;
                    Canvas.SetLeft(Tailor, (Canvas.ActualWidth - SourceImage.Width) / 2);
                    Canvas.SetTop(Tailor, (Canvas.ActualHeight - SourceImage.Width) / 2);
                    FocusEllipse.Center = new Point(Canvas.ActualWidth / 2, Canvas.ActualHeight / 2);
                    FocusEllipse.RadiusX = SourceImage.Width / 2;
                    FocusEllipse.RadiusY = SourceImage.Width / 2;
                }
            }
            else
            {
                if (SourceImage.Height >= 50)
                {
                    Tailor.Height = Tailor.Width = 50;
                    Canvas.SetLeft(Tailor, (Canvas.ActualWidth - 50) / 2);
                    Canvas.SetTop(Tailor, (Canvas.ActualHeight - 50) / 2);
                    FocusEllipse.Center = new Point(Canvas.ActualWidth / 2, Canvas.ActualHeight / 2);
                    FocusEllipse.RadiusX = 25;
                    FocusEllipse.RadiusY = 25;
                }
                else
                {
                    Tailor.Height = Tailor.Width = SourceImage.Height;
                    Canvas.SetLeft(Tailor, (Canvas.ActualWidth - SourceImage.Height) / 2);
                    Canvas.SetTop(Tailor, (Canvas.ActualHeight - SourceImage.Height) / 2);
                    FocusEllipse.Center = new Point(Canvas.ActualWidth / 2, Canvas.ActualHeight / 2);
                    FocusEllipse.RadiusX = SourceImage.Height / 2;
                    FocusEllipse.RadiusY = SourceImage.Height / 2;
                }
            }

            RefreshAvator();
        }

    }
}
