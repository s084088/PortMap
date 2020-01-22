using Client.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region 自身属性

        private string title;


        public string Title { get => title; set { title = value; PCEH(); } }

        #endregion

        public MainViewModel()
        {
            Title = "端口映射器 --- ByNemo";
        }
    }
}
