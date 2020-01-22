using Client.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Client
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
            Res.mutex = new Mutex(true, "FrameClient", out bool ret);
            if (!ret)
            {
                MessageBox.Show("程序已在运行中!");
                Environment.Exit(0);
            }
            Res.LogHelper = new LogHelper("log.txt");
        }
    }
}
