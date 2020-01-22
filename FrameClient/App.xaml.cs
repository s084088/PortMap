using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace FrameClient
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            Res.mutex = new Mutex(true, "Pipelined terminal", out bool ret);
            if (!ret)
            {
                MessageBox.Show("程序已在运行中!");
                Environment.Exit(0);
            }
        }
    }

    public static class Res
    {
        public static Mutex mutex;

        public static string version = "1.0.0";
    }
}
