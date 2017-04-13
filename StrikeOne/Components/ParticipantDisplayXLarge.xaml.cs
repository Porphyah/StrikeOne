using System;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StrikeOne.Core;

namespace StrikeOne.Components
{
    /// <summary>
    /// ParticipantDisplayXLarge.xaml 的交互逻辑
    /// </summary>
    public partial class ParticipantDisplayXLarge : UserControl
    {
        public ParticipantDisplayXLarge()
        {
            InitializeComponent();
        }

        public void Init(Player Player)
        {
            if (Player.Avator != null)
                using (MemoryStream Stream = new MemoryStream())
                {
                    Player.Avator.Save(Stream, ImageFormat.Png);
                    BitmapImage Temp = new BitmapImage();
                    Temp.BeginInit();
                    Temp.CacheOption = BitmapCacheOption.OnLoad;
                    Temp.StreamSource = Stream;
                    Temp.EndInit();
                    AvatorImage.ImageSource = Temp;
                }
            PlayerName.Text = Player.Name;
            Description.Text = Player.Introduction;
            ShadowEffect.Color = Player.BattleData.Group.Color;
        }
    }
}
