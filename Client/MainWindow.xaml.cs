using Client.Socket.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Hosts.Open(T1.Text, T2.Text, T3.Text);
                G1.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Hosts.Start();
                G1.IsEnabled = true;
                new Thread(() =>
                {
                    Thread.Sleep(1000);
                    ConnCount.Text = Hosts.connectMaps.Count.ToString();
                })
                { IsBackground = true }.Start();
            }
            catch(Exception ee)
            {
                MessageBox.Show(ee.Message);
                Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            G1.IsEnabled = false;
            Visibility = Visibility.Hidden;
            try
            {
                Hosts.Close();
            }
            catch { }
        }
    }
}
