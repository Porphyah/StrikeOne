using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace StrikeOne.Components
{
    public class BitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var Bitmap = value as Bitmap;
            if (Bitmap == null) return null;
            var ImageFormat = (ImageFormat)typeof(ImageFormat).GetProperties(BindingFlags.Public |
                    BindingFlags.Static).First(O => O.Name.Equals((string)parameter, 
                    StringComparison.CurrentCultureIgnoreCase)).GetValue(typeof(ImageFormat));
            using (MemoryStream Stream = new MemoryStream())
            {
                Bitmap.Save(Stream, ImageFormat);
                BitmapImage Temp = new BitmapImage();
                Temp.BeginInit();
                Temp.CacheOption = BitmapCacheOption.OnLoad;
                Temp.StreamSource = Stream;
                Temp.EndInit();
                return Temp;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
