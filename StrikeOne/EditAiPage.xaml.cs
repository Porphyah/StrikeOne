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
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using StrikeOne.Core;

namespace StrikeOne
{
    /// <summary>
    /// EditAiPage.xaml 的交互逻辑
    /// </summary>
    public partial class EditAiPage : UserControl
    {
        public Action LeaveAction { set; private get; }

        private AI CurrentAi { set; get; }
        private Skill CurrentSkill { set; get; }
        private System.Drawing.Image Avator { set; get; }
        private System.Drawing.Image Drawing { set; get; } 

        public EditAiPage()
        {
            InitializeComponent();
        }

        public void PageEnter()
        {
            AiListBox.ItemsSource = App.AiList;
            ContentViewer.Visibility = Visibility.Hidden;

            DoubleAnimation OpacityAnimation = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.75),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            };
            ThicknessAnimation MarginAnimation = new ThicknessAnimation()
            {
                From = new Thickness(
                    TitleGrid.Margin.Left - 50,
                    TitleGrid.Margin.Top,
                    TitleGrid.Margin.Right + 50,
                    TitleGrid.Margin.Bottom),
                To = TitleGrid.Margin,
                Duration = TimeSpan.FromSeconds(0.75),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            };

            MarginAnimation.Completed += delegate
            {
                AiListGrid.BeginAnimation(MarginProperty, new ThicknessAnimation()
                {
                    From = AiListGrid.Margin,
                    To = new Thickness(0, 100, 0, 0),
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });
            };

            TitleGrid.BeginAnimation(OpacityProperty, OpacityAnimation);
            TitleGrid.BeginAnimation(MarginProperty, MarginAnimation);
        }
        public void PageLeave()
        {
            if (ContentViewer.Visibility == Visibility.Visible)
                ContentViewer.BeginAnimation(OpacityProperty, new DoubleAnimation()
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                });

            var SkillListMarginAnimation = new ThicknessAnimation()
            {
                From = AiListGrid.Margin,
                To = new Thickness(-300, 100, 0, 0),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
            };
            SkillListMarginAnimation.Completed += delegate
            {
                DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.75) };
                Timer.Tick += delegate
                {
                    LeaveAction?.Invoke();
                    Timer.Stop();
                };
                TitleGrid.BeginAnimation(OpacityProperty, new DoubleAnimation()
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.75),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                });
                TitleGrid.BeginAnimation(MarginProperty, new ThicknessAnimation()
                {
                    From = TitleGrid.Margin,
                    To = new Thickness(
                        TitleGrid.Margin.Left + 50,
                        TitleGrid.Margin.Top,
                        TitleGrid.Margin.Right - 50,
                        TitleGrid.Margin.Bottom),
                    Duration = TimeSpan.FromSeconds(0.75),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                });
                Timer.Start();
            };
            AiListGrid.BeginAnimation(MarginProperty, SkillListMarginAnimation);

        }

        private void Back_Click(object Sender, RoutedEventArgs E)
        {
            if (CurrentAi != null) SaveAi();

            LeaveAction = MainWindow.Instance.EnterEditorMode;
            this.PageLeave();
        }

        private void SelectAi(object Sender, SelectionChangedEventArgs E)
        {
            var Ai = AiListBox.SelectedItem as AI;
            if (Ai == null)
            {
                CurrentAi = null;
                ContentViewer.Visibility = Visibility.Hidden;
                DeleteButton.Visibility = Visibility.Hidden;
                return;
            }
            if (CurrentAi != null) SaveAi();

            Avator = null;
            Drawing = null;
            CurrentAi = Ai;
            AiName.Text = Ai.Name;
            AiDescription.Text = Ai.Introduction;
            if (Ai.Avator != null)
            {
                Avator = Ai.Avator;
                using (MemoryStream Stream = new MemoryStream())
                {
                    Ai.Avator.Save(Stream, ImageFormat.Png);
                    BitmapImage Temp = new BitmapImage();
                    Temp.BeginInit();
                    Temp.CacheOption = BitmapCacheOption.OnLoad;
                    Temp.StreamSource = Stream;
                    Temp.EndInit();
                    AvatorImage.ImageSource = Temp;
                }
            }
            else
                AvatorImage.ImageSource = Resources["EmptyIcon"] as BitmapImage;
            if (Ai.Drawing != null)
            {
                Drawing = Ai.Drawing;
                using (MemoryStream Stream = new MemoryStream())
                {
                    Ai.Drawing.Save(Stream, ImageFormat.Png);
                    BitmapImage Temp = new BitmapImage();
                    Temp.BeginInit();
                    Temp.CacheOption = BitmapCacheOption.OnLoad;
                    Temp.StreamSource = Stream;
                    Temp.EndInit();
                    DrawingImage.Source = Temp;
                }
                SetDrawingButton.Visibility = Visibility.Hidden;
            }
            else
            {
                DrawingImage.Source = null;
                SetDrawingButton.Visibility = Visibility.Visible;
            }

            SkillListBox.ItemsSource = Ai.SkillPool.Keys;
            DeleteSkillButton.IsEnabled = false;
            RadicalSlider.Value = Ai.RadicalRatio;
            RadicalText.Text = (Ai.RadicalRatio*100) + "%";
            DeleteButton.Visibility = Visibility.Visible;
            if (ContentViewer.Visibility == Visibility.Hidden)
            {
                ContentViewer.Visibility = Visibility.Visible;
                ContentViewer.BeginAnimation(OpacityProperty, new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
                });
            }
        }

        private void AddAi_Click(object Sender, RoutedEventArgs E)
        {
            var NewAi = new AI()
            {
                Id = Guid.NewGuid(),
                Name = "未命名",
                RadicalRatio = 0.5
            };
            App.AiList.Add(NewAi);
            IO.SaveAi(NewAi);

            AiListBox.ItemsSource = App.AiList;
            AiListBox.Items.Refresh();
            AiListBox.SelectedIndex = AiListBox.Items.Count - 1;
        }

        private void DeleteAi_Click(object Sender, RoutedEventArgs E)
        {
            if (MessageBox.Show("确实要删除当前AI：" + CurrentAi.Name + "吗？", "删除AI",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
            App.AiList.Remove(CurrentAi);
            IO.DeleteAi(CurrentAi);
            CurrentAi = null;
            AiListBox.ItemsSource = App.AiList;
            AiListBox.Items.Refresh();
        }

        private void SaveAi()
        {
            CurrentAi.Name = AiName.Text;
            CurrentAi.Introduction = AiDescription.Text;
            CurrentAi.Avator = Avator;
            CurrentAi.Drawing = Drawing;
            CurrentAi.RadicalRatio = Math.Round(RadicalSlider.Value, 2);

            IO.SaveAi(CurrentAi);
            AiListBox.Items.Refresh();
        }

        private void SetAiAvator_Click(object Sender, RoutedEventArgs E)
        {
            OpenFileDialog FileDialog = new OpenFileDialog()
            {
                Filter = "图像文件|*.png",
                CheckFileExists = true,
                CheckPathExists = true,
                Title = "选择头像图片",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };
            bool? Result = FileDialog.ShowDialog();
            if (!Result.HasValue || !Result.Value) return;
            string ImagePath = FileDialog.FileName;

            AvatorWindow Dialog = new AvatorWindow();
            Dialog.Init(ImagePath);
            Dialog.ShowDialog();
            if (Dialog.Canceled) return;

            Avator = Dialog.Target;

            using (MemoryStream Stream = new MemoryStream())
            {
                Avator.Save(Stream, ImageFormat.Png);
                BitmapImage Temp = new BitmapImage();
                Temp.BeginInit();
                Temp.CacheOption = BitmapCacheOption.OnLoad;
                Temp.StreamSource = Stream;
                Temp.EndInit();
                AvatorImage.ImageSource = Temp;
            }
        }

        private void SetAiDrawing_Click(object Sender, RoutedEventArgs E)
        {
            OpenFileDialog FileDialog = new OpenFileDialog()
            {
                Title = "选择AI立绘",
                Filter = "图像文件(*.png)|*.png",
                CheckFileExists = true,
                CheckPathExists = true
            };
            bool? Result = FileDialog.ShowDialog();
            if (!Result.HasValue || !Result.Value) return;
            string ImagePath = FileDialog.FileName;

            Drawing = System.Drawing.Image.FromFile(ImagePath);

            using (MemoryStream Stream = new MemoryStream())
            {
                Drawing.Save(Stream, ImageFormat.Png);
                BitmapImage Temp = new BitmapImage();
                Temp.BeginInit();
                Temp.CacheOption = BitmapCacheOption.OnLoad;
                Temp.StreamSource = Stream;
                Temp.EndInit();
                DrawingImage.Source = Temp;
            }

            SetDrawingButton.Visibility = Drawing != null ? Visibility.Visible : Visibility.Hidden;
        }

        private void Drawing_MouseEnter(object Sender, MouseEventArgs E)
        {
            if (DrawingImage.Source == null) return;
            DrawingBlurEffect.BeginAnimation(BlurEffect.RadiusProperty, new DoubleAnimation()
            {
                From = DrawingBlurEffect.Radius,
                To = 10,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
            });
            SetDrawingButton.Visibility = Visibility.Visible;
        }
        private void Drawing_MouseLeave(object Sender, MouseEventArgs E)
        {
            if (DrawingImage.Source == null) return;
            DrawingBlurEffect.BeginAnimation(BlurEffect.RadiusProperty, new DoubleAnimation()
            {
                From = DrawingBlurEffect.Radius,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }
            });
            SetDrawingButton.Visibility = Visibility.Hidden;
        }


        private void SelectSkillPool(object Sender, SelectionChangedEventArgs E)
        {
            var Skill = SkillListBox.SelectedItem as Skill;
            if (Skill == null)
            {
                DeleteSkillButton.IsEnabled = false;
                EditSkillButton.IsEnabled = false;
                return;
            }

            CurrentSkill = Skill;
            DeleteSkillButton.IsEnabled = true;
            EditSkillButton.IsEnabled = true;
        }

        private void AddSkill_Click(object Sender, RoutedEventArgs E)
        {
            SkillWindow Dialog = new SkillWindow();
            Dialog.Init(App.SkillList.Except(CurrentAi.SkillPool.Keys).ToList());
            Dialog.ShowDialog();
            if (Dialog.Canceled) return;

            CurrentAi.SkillPool.Add(Dialog.SelectedSkill, new string[2]);
            SkillListBox.ItemsSource = CurrentAi.SkillPool.Keys;
            SkillListBox.Items.Refresh();
        }
        private void DeleteSkill_Click(object Sender, RoutedEventArgs E)
        {
            if (MessageBox.Show("确实要删除AI：" + CurrentAi.Name + "当前的技能：" + CurrentSkill.Name + "吗？", "删除技能",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
            CurrentAi.SkillPool.Remove(CurrentSkill);
            CurrentSkill = null;
            SkillListBox.ItemsSource = CurrentAi.SkillPool.Keys;
            SkillListBox.Items.Refresh();
        }

        private void SetRadicalValue(object Sender, RoutedPropertyChangedEventArgs<double> E)
        {
            if (CurrentAi == null) return;
            double Value = Math.Round(RadicalSlider.Value, 2);
            RadicalText.Text = (Value*100) + "%";
            CurrentAi.RadicalRatio = Value;
        }

        private void EditSkill_Click(object Sender, RoutedEventArgs E)
        {
            EditAiSkillWindow Dialog = new EditAiSkillWindow
            {
                ConditionScript = CurrentAi.SkillPool[CurrentSkill][0],
                TargetScript = CurrentAi.SkillPool[CurrentSkill][1]
            };
            Dialog.ShowDialog();
            if (Dialog.Canceled) return;

            CurrentAi.SkillPool[CurrentSkill][0] = Dialog.ConditionScript;
            CurrentAi.SkillPool[CurrentSkill][1] = Dialog.TargetScript;
        }
    }
}
