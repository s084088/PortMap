using FrameClient.Page;
using FrameClient.Socket.Map;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace FrameClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Hosts hosts = new Hosts();
        public MainWindow() => InitializeComponent();

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await hosts.Open(T1.Text, T2.Text, T3.Text);
                G1.IsEnabled = false;
            }
            catch (Exception ex) { await Show(ex.Message); }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            G1.IsEnabled = false;
            button2.IsEnabled = false;
            D1.ItemsSource = hosts.connectMaps;
            IC1.ItemsSource = new SwatchesProvider().Swatches;
            new Thread(() =>
            {
                while (true)
                {
                    hosts.CheckConnect();
                    Thread.Sleep(1000);
                    Dispatcher.Invoke(new Action(() => ConnCount.Text = hosts.connectMaps.Count.ToString()));
                }
            })
            { IsBackground = true, Priority = ThreadPriority.BelowNormal }.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => Environment.Exit(0);

        public async Task Show(string s) => await DialogHost.Show(new MessageBoxOK { Message = { Text = s } }, "RootDialog");


        private async void button3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                T5.IsEnabled = false;
                button3.IsEnabled = false;
                P1.Visibility = Visibility.Visible;
                await hosts.Start(T5.Text);
                P1.Visibility = Visibility.Hidden;
                G1.IsEnabled = true;
                button2.IsEnabled = true;
                T4.Text = T5.Text;
            }
            catch (Exception ee)
            {
                T5.IsEnabled = true;
                button3.IsEnabled = true;
                button2.IsEnabled = false;
                G1.IsEnabled = false;
                P1.Visibility = Visibility.Hidden;
                await Show(ee.Message);
            }
        }

        private async void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                G1.IsEnabled = false;
                button2.IsEnabled = false;
                await hosts.Close();
            }
            catch { }

            T5.IsEnabled = true;
            button3.IsEnabled = true;
        }

        private void ModifyTheme(Action<ITheme> modificationAction)
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();

            modificationAction?.Invoke(theme);

            paletteHelper.SetTheme(theme);
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e) => ModifyTheme(theme => theme.SetBaseTheme(TB1.IsChecked == true ? Theme.Dark : Theme.Light));

        private void PackIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => ModifyTheme(theme => theme.SetPrimaryColor((((PackIcon)sender).Tag as Swatch).ExemplarHue.Color));
    }

    public class SpeedConver : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int i = System.Convert.ToInt32(value);
            if (i < 1000) return i.ToString() + "B";
            if (i < 1000 * 1024) return (i / 1024D).ToString("#.##") + "KB";
            if (i < 1000 * 1024 * 1024) return (i / 1024D / 1024D).ToString("#.##") + "MB";
            else return (i / 1024D / 1024D / 1024D).ToString("#.##") + "GB";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
