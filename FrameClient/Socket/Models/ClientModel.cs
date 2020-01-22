using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace FrameClient.Socket.Models
{
    public class ConnectMap: ViewModelBase
    {
        private int @in = 0;
        private int @out = 0;


        public string Outip { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;

        public TcpClient InClient { get; set; }

        public TcpClient OutClient { get; set; }

        public int In
        {
            get => @in; set
            {
                @in = value;
                PCEH();
            }
        }
        public int Out
        {
            get => @out; set
            {
                @out = value;
                PCEH();
            }
        }
        public int State { get; set; } = 1;

        public string ID { get; set; } = Guid.NewGuid().ToString("N");

        public int Index { get; set; }
    }

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 通知界面更新指定属性名
        /// </summary>
        /// <param name="propertyName"></param>
        public void PC(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// 通知界面更新指定属性名(属性名由调用方法获取)
        /// </summary>
        /// <param name="propertyName"></param>
        public void PCEH([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
