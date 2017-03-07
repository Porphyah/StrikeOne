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
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace StrikeOne
{
    /// <summary>
    /// AiWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AiWindow : MetroWindow
    {
        public bool Canceled { private set; get; } = true;

        public AiWindow()
        {
            InitializeComponent();
            UserGrid.Tag = false;
            AiGrid.Tag = false;

            UserName.Text = App.CurrentUser.Name;
            if (App.CurrentUser.Avator != null)
                using (MemoryStream Stream = new MemoryStream())
                {
                    App.CurrentUser.Avator.Save(Stream, App.CurrentUser.AvatorFormat);
                    BitmapImage Temp = new BitmapImage();
                    Temp.BeginInit();
                    Temp.CacheOption = BitmapCacheOption.OnLoad;
                    Temp.StreamSource = Stream;
                    Temp.EndInit();
                    AvatorImage.ImageSource = Temp;
                }
        }

        private void MouseEnterGrid(object Sender, MouseEventArgs E)
        {
            var Target = Sender as Grid;
            if ((bool) Target.Tag) return;
            Target.Background = new SolidColorBrush(Color.FromArgb(136, 15, 72, 128));
        }
        private void MouseLeaveGrid(object Sender, MouseEventArgs E)
        {
            var Target = Sender as Grid;
            if ((bool)Target.Tag) return;
            Target.Background = new SolidColorBrush(Color.FromArgb(136, 0, 0, 0));
        }
        private void UserGrid_Click(object Sender, MouseButtonEventArgs E)
        {
            UserGrid.Tag = true;
            UserGrid.Background = new SolidColorBrush(Color.FromArgb(136, 30, 144, 255));
            if ((bool) AiGrid.Tag)
            {
                AiGrid.Background = new SolidColorBrush(Color.FromArgb(136, 0, 0, 0));
                AiGrid.Tag = false;
            }
            AcceptButton.Visibility = Visibility.Visible;
        }
        private void AiGrid_Click(object Sender, MouseButtonEventArgs E)
        {
            AiGrid.Tag = true;
            AiGrid.Background = new SolidColorBrush(Color.FromArgb(136, 30, 144, 255));
            if ((bool)UserGrid.Tag)
            {
                UserGrid.Background = new SolidColorBrush(Color.FromArgb(136, 0, 0, 0));
                UserGrid.Tag = false;
            }
            AcceptButton.Visibility = Visibility.Visible;
        }

        private void Cancel_Click(object Sender, RoutedEventArgs E)
        {
            Canceled = true;
            this.Close();
        }
    }
}
