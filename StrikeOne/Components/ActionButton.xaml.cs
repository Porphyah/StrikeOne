﻿using System;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StrikeOne.Components
{
    /// <summary>
    /// ActionButton.xaml 的交互逻辑
    /// </summary>
    public partial class ActionButton : UserControl
    {
        public ActionButton()
        {
            InitializeComponent();
        }

        public void Init(string Name, string Image, string Description, int? Probability = null)
        {
            ButtonName.Text = Name;
            ButtonImage.Source = Resources[Image] as BitmapImage;

            ActionDescription.Init(Name, Resources[Image] as BitmapImage, Description, Probability);
        }
        public void Init(string Name, System.Drawing.Image Image, string Description, int? Probability = null)
        {
            ButtonName.Text = Name;

            using (MemoryStream Stream = new MemoryStream())
            {
                Image.Save(Stream, ImageFormat.Png);
                BitmapImage Temp = new BitmapImage();
                Temp.BeginInit();
                Temp.CacheOption = BitmapCacheOption.OnLoad;
                Temp.StreamSource = Stream;
                Temp.EndInit();
                ButtonImage.Source = Temp;
                ActionDescription.Init(Name, Temp, Description, Probability);
            }
        }


        private void OnMouseEnter(object Sender, MouseEventArgs E)
        {
            this.Effect = new DropShadowEffect()
            {
                BlurRadius = 5,
                Color = Colors.White,
                ShadowDepth = 0
            };
        }
        private void OnMouseLeave(object Sender, MouseEventArgs E)
        {
            this.Effect = null;
        }
    }
}
