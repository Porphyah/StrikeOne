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
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using StrikeOne.Core.Lua;

namespace StrikeOne
{
    /// <summary>
    /// DebugWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DebugWindow : MetroWindow
    {
        private SolidColorBrush CurrentParagraphBrush = new SolidColorBrush(Colors.AliceBlue);

        public DebugWindow()
        {
            InitializeComponent();
            Box.Document.Blocks.Clear();
        }

        public void SetLog(string Text, Color Color, bool Bold = false, bool Italic = false)
        {
            this.Dispatcher.Invoke(() =>
            {
                CurrentParagraphBrush = CurrentParagraphBrush.Color == Colors.AliceBlue
                    ? new SolidColorBrush(Colors.White)
                    : new SolidColorBrush(Colors.AliceBlue);
                Box.Document.Blocks.Add(new Paragraph()
                {
                    Inlines =
                    {
                        new Run()
                        {
                            Text = "[" + DateTime.Now + "] ",
                            Foreground = new SolidColorBrush(Colors.Black)
                        },
                        new Run()
                        {
                            Text = Text,
                            Foreground = new SolidColorBrush(Color),
                            FontWeight = Bold ? FontWeights.Bold : FontWeights.Normal,
                            FontStyle = Italic ? FontStyles.Italic : FontStyles.Normal
                        }
                    },
                    Background = CurrentParagraphBrush,
                    Margin = new Thickness(0, 0, 0, 0)
                });
            });
        }

        private void ClearLog(object sender, RoutedEventArgs e)
        {
            Box.Document.Blocks.Clear();
        }

        private void SendConsoleMessage(object Sender, KeyEventArgs E)
        {
            if (E.Key == Key.Enter)
            {
                ((LuaDebug)LuaMain.LuaState["Debug"]).Console(ConsoleText.Text);
                ConsoleText.Text = "";
            }
        }
    }
}
