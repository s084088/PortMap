using Client.Model;
using Client.ViewModel.Base;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Client.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region 自身属性

        private string title;
        private ConnentIP connentIP;
        private ConnentPort connentPort;

        public string Title { get => title; set { title = value; PCEH(); } }

        public List<Swatch> Swatches { get; private set; }


        public ConnentIP ConnentIP { get => connentIP; set { connentIP = value; PCEH(); } }

        public ConnentPort ConnentPort { get => connentPort; set { connentPort = value; PCEH(); } }

        #endregion

        #region 按钮接口

        public ICommand ToggleBaseCommand { get; set; }
        public ICommand ApplyPrimaryCommand { get; set; }
        public ICommand ConnectCommand { get; set; }
        public ICommand DisConnectCommand { get; set; }
        public ICommand StartCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        #endregion

        #region 构造函数

        public MainViewModel()
        {
            Title = "端口映射器 --- ByNemo";
            Swatches = new SwatchesProvider().Swatches.ToList();
            ConnentIP = new ConnentIP();
            ToggleBaseCommand = new CommandBase(o => ApplyBase((bool)o));
            ApplyPrimaryCommand = new CommandBase(o => ApplyPrimary((Swatch)o));
            ConnectCommand = new CommandBase(l => ConnentIP.Start(), k => ConnentIP.State == 0);
            DisConnectCommand = new CommandBase(l => ConnentIP.Stop(), k => ConnentIP.State == 1);
            StartCommand = new CommandBase(l => ConnentIP.StartPort());
            CloseCommand = new CommandBase(l => ConnentIP.Close((ConnentPort)l));
            ApplyPrimary(Swatches[Math.Abs(Guid.NewGuid().GetHashCode()) % Swatches.Count]);
        }

        #endregion

        #region 私有方法

        private void ModifyTheme(Action<ITheme> modificationAction)
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();

            modificationAction?.Invoke(theme);

            paletteHelper.SetTheme(theme);
        }

        private void ApplyBase(bool isDark)
        {
            ModifyTheme(theme => theme.SetBaseTheme(isDark ? Theme.Dark : Theme.Light));
        }

        private void ApplyPrimary(Swatch swatch)
        {
            ModifyTheme(theme => theme.SetPrimaryColor(swatch.ExemplarHue.Color));
        }

        #endregion
    }
}
