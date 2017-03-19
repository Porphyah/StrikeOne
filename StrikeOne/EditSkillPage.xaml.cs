using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using StrikeOne.Core;

namespace StrikeOne
{
    /// <summary>
    /// EditSkillPage.xaml 的交互逻辑
    /// </summary>
    public partial class EditSkillPage : UserControl
    {
        public Action LeaveAction { set; private get; }
        public Skill CurrentSkill { private set; get; }
        private System.Drawing.Image SkillImg { set; get; }

        public EditSkillPage()
        {
            InitializeComponent();
        }

        public void PageEnter()
        {
            SkillListBox.ItemsSource = App.SkillList;
            SkillTargetConverter.AddSkillTarget += AddSkillTarget_Click;
            SkillProbability.Init(6, true);
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
                SkillListGrid.BeginAnimation(MarginProperty, new ThicknessAnimation()
                {
                    From = SkillListGrid.Margin,
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
                From = SkillListGrid.Margin,
                To = new Thickness(-300, 100, 0, 0),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new ExponentialEase() {EasingMode = EasingMode.EaseIn}
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
            SkillListGrid.BeginAnimation(MarginProperty, SkillListMarginAnimation);

        }

        private void SelectSkill(object Sender, SelectionChangedEventArgs E)
        {
            var Skill = SkillListBox.SelectedItem as Skill;
            if (Skill == null)
            {
                CurrentSkill = null;
                ContentViewer.Visibility = Visibility.Hidden;
                DeleteButton.Visibility = Visibility.Hidden;
                return;
            }
            if (CurrentSkill != null) SaveSkill();

            SkillImg = null;
            CurrentSkill = Skill;
            SkillName.Text = Skill.Name;
            SkillDescription.Text = Skill.Description;
            if (Skill.Image != null)
            {
                SkillImg = Skill.Image;
                using (MemoryStream Stream = new MemoryStream())
                {
                    Skill.Image.Save(Stream, ImageFormat.Png);
                    BitmapImage Temp = new BitmapImage();
                    Temp.BeginInit();
                    Temp.CacheOption = BitmapCacheOption.OnLoad;
                    Temp.StreamSource = Stream;
                    Temp.EndInit();
                    SkillImage.Source = Temp;
                }
            }
            else
                SkillImage.Source = null;

            SkillProbability.SetValue(Skill.Probability);
            SkillOccasionBox.SelectedItem = SkillOccasionConverter.ConverterDictionary[Skill.Occasion];
            SkillTargetsListBox.ItemsSource = Skill.TargetSelections
                .Select(O => SkillTargetConverter.ConverterDictionary[O]);
            LaunchScriptBox.Document.Blocks.Clear();
            LaunchScriptBox.Document.Blocks.Add(new Paragraph(
                new Run(Skill.LaunchScript)));
            CountCheckBox.IsChecked = Skill.TotalCount != -1;
            CountText.Text = Skill.TotalCount.ToString();
            DurationCheckBox.IsChecked = Skill.Duration != 0;
            DurationText.Text = Skill.Duration.ToString();
            CoolDownCheckBox.IsChecked = Skill.CoolDown != 0;
            CoolDownText.Text = Skill.CoolDown.ToString();
            AffectScriptBox.Document.Blocks.Clear();
            AffectScriptBox.Document.Blocks.Add(new Paragraph(
                new Run(Skill.AffectScript)));

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

        private void AddSkill_Click(object Sender, RoutedEventArgs E)
        {
            var NewSkill = new Skill()
            {
                Id = Guid.NewGuid(),
                Name = "未命名",
            };
            App.SkillList.Add(NewSkill);
            IO.SaveSkill(NewSkill);

            SkillListBox.ItemsSource = App.SkillList;
            SkillListBox.Items.Refresh();
            SkillListBox.SelectedIndex = SkillListBox.Items.Count - 1;
        }

        private void DeleteSkill_Click(object Sender, RoutedEventArgs E)
        {
            if (MessageBox.Show("确实要删除当前技能：" + CurrentSkill.Name + "吗？", "删除技能",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
            App.SkillList.Remove(CurrentSkill);
            IO.DeleteSkill(CurrentSkill);
            CurrentSkill = null;
            SkillListBox.ItemsSource = App.SkillList;
            SkillListBox.Items.Refresh();
        }

        private void SaveSkill()
        {
            CurrentSkill.Name = SkillName.Text;
            CurrentSkill.Description = SkillDescription.Text;
            CurrentSkill.Image = SkillImg;
            CurrentSkill.Probability = SkillProbability.Numerator;
            CurrentSkill.Occasion = SkillOccasionConverter.ConverterDictionary
                .ToDictionary(P => P.Value, Q => Q.Key)
                [(string) SkillOccasionBox.SelectedItem];
            CurrentSkill.TargetSelections = SkillTargetsListBox.Items.Cast<string>()
                .Select(O => SkillTargetConverter.ConverterDictionary
                .ToDictionary(P => P.Value, Q => Q.Key)[O]).ToList();
            CurrentSkill.LaunchScript = LaunchScriptBox.Document.Blocks.OfType<Paragraph>()
                .SelectMany(O => O.Inlines.OfType<Run>().Select(P => P.Text))
                .Aggregate((A, B) => A + "\n" + B);
            CurrentSkill.AffectScript = AffectScriptBox.Document.Blocks.OfType<Paragraph>()
                .SelectMany(O => O.Inlines.OfType<Run>().Select(P => P.Text))
                .Aggregate((A, B) => A + "\n" + B);

            int Count;
            if (CountCheckBox.IsChecked.Value && int.TryParse(CountText.Text, out Count))
                CurrentSkill.TotalCount = Count;
            else
                CurrentSkill.TotalCount = -1;

            int Duration;
            if (DurationCheckBox.IsChecked.Value && int.TryParse(DurationText.Text, out Duration))
                CurrentSkill.Duration = Duration;
            else
                CurrentSkill.Duration = 0;

            int CoolDown;
            if (CoolDownCheckBox.IsChecked.Value && int.TryParse(CoolDownText.Text, out CoolDown))
                CurrentSkill.CoolDown = CoolDown;
            else
                CurrentSkill.CoolDown = 0;

            IO.SaveSkill(CurrentSkill);
            SkillListBox.Items.Refresh();
        }

        private void SetSkillImg_Click(object Sender, RoutedEventArgs E)
        {
            OpenFileDialog FileDialog = new OpenFileDialog()
            {
                Title = "选择技能图标",
                Filter = "图像文件(*.png)|*.png",
                CheckFileExists = true,
                CheckPathExists = true
            };
            bool? Result = FileDialog.ShowDialog();
            if (!Result.HasValue || !Result.Value) return;
            string ImagePath = FileDialog.FileName;

            SkillImg = System.Drawing.Image.FromFile(ImagePath);

            using (MemoryStream Stream = new MemoryStream())
            {
                SkillImg.Save(Stream, ImageFormat.Png);
                BitmapImage Temp = new BitmapImage();
                Temp.BeginInit();
                Temp.CacheOption = BitmapCacheOption.OnLoad;
                Temp.StreamSource = Stream;
                Temp.EndInit();
                SkillImage.Source = Temp;
            }
        }

        private void AddSkillTarget_Click(object Sender, RoutedEventArgs E)
        {
            var Item = Sender as MenuItem;
            if (Item == null) return;
            CurrentSkill.TargetSelections.Add((SkillTarget)Item.Tag);
            SkillTargetsListBox.ItemsSource = CurrentSkill.TargetSelections
                .Select(O => SkillTargetConverter.ConverterDictionary[O]);
            SkillTargetsListBox.Items.Refresh();
        }
        private void SelectSkillTarget(object Sender, SelectionChangedEventArgs E)
        {
            string Target = SkillTargetsListBox.SelectedItem as string;
            DeleteSkillTargetButton.IsEnabled = Target != null;
        }
        private void DeleteSkillTarget_Click(object Sender, RoutedEventArgs E)
        {
            if (!(SkillTargetsListBox.SelectedItem is string)) return;
            CurrentSkill.TargetSelections.RemoveAt(SkillTargetsListBox.SelectedIndex);
            SkillTargetsListBox.ItemsSource = CurrentSkill.TargetSelections
                .Select(O => SkillTargetConverter.ConverterDictionary[O]);
            SkillTargetsListBox.Items.Refresh();
        }

        private void Back_Click(object Sender, RoutedEventArgs E)
        {
            if (CurrentSkill != null) SaveSkill();

            LeaveAction = MainWindow.Instance.EnterEditorMode;
            this.PageLeave();
        }
    }

    public class SkillOccasionConverter : IValueConverter
    {
        public static readonly Dictionary<SkillOccasion, string> ConverterDictionary = new Dictionary<SkillOccasion, string>()
        {
            { SkillOccasion.UnderAttack, "遭到攻击时" },
            { SkillOccasion.Defending, "选择防御时" },
            { SkillOccasion.Defended, "防御结束时" },
            { SkillOccasion.CounterAttacking, "选择反击时" },
            { SkillOccasion.CounterAttacked, "反击结束时" },
            { SkillOccasion.BeforeAttacking, "攻击前" },
            { SkillOccasion.AfterAttacking, "攻击后" }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var SourceList = value as IEnumerable<SkillOccasion>;
            return SourceList?.Select(Enum => ConverterDictionary[Enum]).ToList();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var SourceList = value as IEnumerable<string>;
            return SourceList?.Select(Enum => ConverterDictionary
                .ToDictionary(O => O.Value, P => P.Key)[Enum]).ToList();
        }
    }
    public class SkillTargetConverter : IValueConverter
    {
        public static readonly Dictionary<SkillTarget, string> ConverterDictionary = new Dictionary<SkillTarget, string>()
        {
            { SkillTarget.Self, "施法者" },
            { SkillTarget.Ally, "单个盟友（包含施法者）" },
            { SkillTarget.Enemy, "单个敌人" },
            { SkillTarget.AllyWithoutSelf, "单个盟友（不含施法者）" },
            { SkillTarget.SelfGroup, "施法者小组" },
            { SkillTarget.EnemyGroup, "敌军小组" },
            { SkillTarget.AllEnemies, "所有敌人" },
            { SkillTarget.AllPlayers, "所有玩家" }
        };

        public static event RoutedEventHandler AddSkillTarget;
        private static void OnAddSkillTarget(object Sender, RoutedEventArgs E)
        {
            AddSkillTarget?.Invoke(Sender, E);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var SourceList = (IEnumerable<SkillTarget>)value;
            if (SourceList == null) return null;
            if (bool.Parse(parameter.ToString()))
                return SourceList.Select(Enum =>
                {
                    var MenuItem = new MenuItem()
                    {
                        Header = ConverterDictionary[Enum],
                        Tag = Enum
                    };
                    MenuItem.Click += OnAddSkillTarget;
                    return MenuItem;
                }).ToList();
            else
                return SourceList.Select(O => ConverterDictionary[O]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (bool.Parse(parameter.ToString()))
            {
                var SourceList = (IEnumerable<MenuItem>)value;
                return SourceList.Select(Enum => (SkillTarget) Enum.Tag).ToList();
            }
            else
            {
                var SourceList = (IEnumerable<string>)value;
                return SourceList.Select(O => ConverterDictionary.ToDictionary
                    (P => P.Value, Q => Q.Key)[O]).ToList();
            }
        }
    }
}
