using Client.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client.Model
{
    /// <summary>
    /// 转发模块
    /// </summary>
    public class ConnectMap : ViewModelBase
    {
        public ConnentPort ConnentPort;

        private long @in = 0;
        private long @out = 0;

        public string ID { get; set; } = Guid.NewGuid().ToString("N");

        public int Index { get; set; }

        /// <summary>
        /// 状态,0停止,1启动
        /// </summary>
        public int State { get; set; } = 1;

        /// <summary>
        /// 外部IP
        /// </summary>
        public string Outip { get; set; }

        /// <summary>
        /// 开启时间
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 客户端对内部服务器连接
        /// </summary>
        public TcpClient InClient { get; set; }

        /// <summary>
        /// 客户端对外部服务器连接
        /// </summary>
        public TcpClient OutClient { get; set; }

        /// <summary>
        /// 传入数据
        /// </summary>
        public long In { get => @in; set { @in = value; PCEH(); } }

        /// <summary>
        /// 传出数据
        /// </summary>
        public long Out { get => @out; set { @out = value; PCEH(); } }

        public void SetIn(int v)
        {
            In += v;
            ConnentPort.SetIn(v);
        }

        public void SetOut(int v)
        {
            Out += v;
            ConnentPort.SetOut(v);
        }

        public void Stop()
        {
            InClient.GetStream().Dispose();
            OutClient.GetStream().Dispose();
            InClient.Close();
            OutClient.Close();
            InClient.Dispose();
            OutClient.Dispose();
        }
    }
}
