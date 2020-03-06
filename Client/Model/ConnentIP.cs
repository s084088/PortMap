using Client.Helper;
using Client.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Client.Model
{
    public class ConnentIP : ViewModelBase
    {

        private int indexCount = 1;
        private ObservableCollection<ConnentPort> connentPorts = new ObservableCollection<ConnentPort>();
        private long @in = 0;
        private long @out = 0;
        private string outIP = "port.jiyiwm.cn";
        private int outPort = 4999;
        private int state = 0;
        public TcpClient tc;   //控制器
        public NetworkStream kz = null;  //服务器连接流
        private string errMsg;
        private string sInIP = "localhost";
        private int sInPort = 3306;
        private int sOutPort = 4408;

        public string ID { get; set; } = Guid.NewGuid().ToString("N");

        public int Index { get; set; }

        public string SInIP { get => sInIP; set { sInIP = value; PCEH(); } }
        public int SInPort { get => sInPort; set { sInPort = value; PCEH(); } }
        public int SOutPort { get => sOutPort; set { sOutPort = value; PCEH(); } }

        public ObservableCollection<ConnentPort> ConnentPorts { get => connentPorts; set { connentPorts = value; PCEH(); } }

        /// <summary>
        /// 传入数据
        /// </summary>
        public long In { get => @in; set { @in = value; PCEH(); } }

        /// <summary>
        /// 传出数据
        /// </summary>
        public long Out { get => @out; set { @out = value; PCEH(); } }

        /// <summary>
        /// 启动状态,0停止,1启动
        /// </summary>
        public int State { get => state; set { state = value; PCEH(); CommandManager.InvalidateRequerySuggested(); } }

        /// <summary>
        /// 服务器IP
        /// </summary>
        public string OutIP { get => outIP; set { outIP = value; PCEH(); } }

        public void Close(ConnentPort l)
        {
            l.Stop();
            connentPorts.Remove(l);
        }

        /// <summary>
        /// 服务器端口
        /// </summary>
        public int OutPort { get => outPort; set { outPort = value; PCEH(); } }

        public string ErrMsg { get => errMsg; set { errMsg = value; PCEH(); Res.LogHelper.Error(value); } }

        public void SetIn(int v)
        {
            In += v;
        }

        public void SetOut(int v)
        {
            Out += v;
        }

        /// <summary>
        /// 启动
        /// </summary>
        public async void Start()
        {
            State = 2;
            tc = new TcpClient();
            try
            {
                await tc.ConnectAsync(outIP, outPort);
                kz = tc.GetStream();
                string[] s = {
                    "check",
                    Res.version,
                    Environment.OSVersion.ToString(),
                    Environment.MachineName,
                    Environment.UserName,
                    OtherHelper.GetInfo(),
                };
                kz.H_Send(string.Join(",", s));
                string[] ss = kz.H_Recv().Split(',');
                if (ss[0] == "fail") throw new Exception(ss[1]);
                State = 1;
                ErrMsg = "连接成功!";
                Lisen();
            }
            catch (Exception e)
            {
                State = 0;
                ErrMsg = e.Message;
            }

        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            State = 2;
            try { kz.H_Send($"stop"); } catch { }
            kz.Dispose();
            tc.Close();
            State = 0;
            ErrStop();
        }

        /// <summary>
        /// 监听线程
        /// </summary>
        private void Lisen()
        {
            new Thread(() =>
            {
                while (State == 1)
                {
                    try
                    {
                        string s = kz.H_Recv();
                        Analysis(s);
                    }
                    catch (Exception e)
                    {
                        if (State == 0)
                        {
                            ErrMsg = "停止成功!";
                            ErrStop();
                        }
                        else
                        {
                            State = 0;
                            ErrMsg = e.Message;
                            ErrStop();
                        }
                    }
                }
            })
            { IsBackground = true }.Start();
            new Thread(() =>
            {
                while (State == 1)
                {
                    Thread.Sleep(1000);
                }
            })
            { IsBackground = true }.Start();
        }

        public void ErrStop()
        {
            foreach (ConnentPort connentPort in ConnentPorts) connentPort.ErrStop();
            OtherHelper.Dis(() => ConnentPorts.Clear());
        }

        private void Analysis(string s)
        {
            string[] ss = s.Split(',');
            if (ss[0] == "fail")
            {
                ErrMsg = ss[1];
                ErrStop();
            }
            else if (ss[0] == "s_PortOccupied")
            {
                ConnentPort connentPort = ConnentPorts.FirstOrDefault(l => l.ID == ss[1]);
                connentPort.ErrMsg = "远程端口被占用";
                connentPort.ErrStop();
            }
            else if (ss[0] == "s_PortAccess")
            {
                ConnentPort connentPort = ConnentPorts.FirstOrDefault(l => l.ID == ss[1]);
                connentPort.ErrMsg = null;
                connentPort.State = 1;
            }
            else if (ss[0] == "s_StartListen")
            {
                ConnentPort connentPort = ConnentPorts.FirstOrDefault(l => l.ID == ss[1]);
                connentPort.NewConn(ss[2], ss[3], ss[4]);
            }
        }

        public async void StartPort()
        {
            ConnentPort connentPort = new ConnentPort
            {
                connentIP = this,
                ID = Guid.NewGuid().ToString("N"),
                InIP = SInIP,
                InPort = SInPort,
                OutIP = OutIP,
                OutPort = SOutPort,
                Index = indexCount++,
            };
            ConnentPorts.Add(connentPort);
            string[] ss = {
                    "s_Start",
                    SOutPort.ToString(),
                    SInIP,
                    SInPort.ToString(),
                    connentPort.ID,
                };
            await kz.H_SendAsync(string.Join(",", ss));
        }
    }
}
