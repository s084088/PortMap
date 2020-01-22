using Client.Helper;
using Client.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client.Model
{
    /// <summary>
    /// 单个端口转发模块
    /// </summary>
    public class ConnentPort : ViewModelBase
    {
        public ConnentIP connentIP;
        private ObservableCollection<ConnectMap> connectMaps = new ObservableCollection<ConnectMap>();
        private string inIP = "127.0.0.1";
        private int inPort;
        private long @in = 0;
        private long @out = 0;
        private string outIP;
        private int outPort;
        private int state = 0;
        private int count;
        private string errMsg;
        private int indexCount = 1;

        public string ID { get; set; } = Guid.NewGuid().ToString("N");

        public int Index { get; set; }

        public string ErrMsg { get => errMsg; set { errMsg = value; PCEH(); } }

        /// <summary>
        /// 转发模块
        /// </summary>
        public ObservableCollection<ConnectMap> ConnectMaps { get => connectMaps; set { connectMaps = value; PCEH(); } }

        /// <summary>
        /// 传入数据
        /// </summary>
        public long In { get => @in; set { @in = value; PCEH(); } }

        /// <summary>
        /// 传出数据
        /// </summary>
        public long Out { get => @out; set { @out = value; PCEH(); } }

        /// <summary>
        /// 对内IP
        /// </summary>
        public string InIP { get => inIP; set { inIP = value; PCEH(); } }

        /// <summary>
        /// 对内端口
        /// </summary>
        public int InPort { get => inPort; set { inPort = value; PCEH(); } }

        /// <summary>
        /// 对外IP
        /// </summary>
        public string OutIP { get => outIP; set { outIP = value; PCEH(); } }

        /// <summary>
        /// 对外端口
        /// </summary>
        public int OutPort { get => outPort; set { outPort = value; PCEH(); } }

        /// <summary>
        /// 启动状态,0停止,1启动
        /// </summary>
        public int State { get => state; set { state = value; PCEH(); } }

        /// <summary>
        /// 当前客户端数量
        /// </summary>
        public int Count { get => count; set { count = value; PCEH(); } }

        public void SetIn(int v)
        {
            In += v;
            connentIP.SetIn(v);
        }

        public void SetOut(int v)
        {
            Out += v;
            connentIP.SetOut(v);
        }

        /// <summary>
        /// 被动停止
        /// </summary>
        public void ErrStop()
        {
            Close();
        }

        /// <summary>
        /// 主动停止
        /// </summary>
        public async void Stop()
        {
            string[] ss = {
                "s_Stop",
                ID,
                };
            await connentIP.kz.H_SendAsync(string.Join(",", ss));
            Close();
        }

        /// <summary>
        /// 停止子连接
        /// </summary>
        private void Close()
        {
            foreach (ConnectMap connectMap in ConnectMaps) connectMap.Stop();
            ConnectMaps.Clear();
        }

        /// <summary>
        /// 创建新连接
        /// </summary>
        internal void NewConn(string port, string outip, string outport)
        {
            TcpClient tc1 = new TcpClient(InIP, InPort);
            TcpClient tc2 = new TcpClient(OutIP, Convert.ToInt32(port));
            ConnectMap connectMap = new ConnectMap
            {
                ConnentPort = this,
                InClient = tc1,
                OutClient = tc2,
                Outip = outip,
                Index = indexCount++,
            };
            Count++;
            ConnectMaps.Add(connectMap);
            PortHelper.Lianjie(connectMap);
        }
    }
}
