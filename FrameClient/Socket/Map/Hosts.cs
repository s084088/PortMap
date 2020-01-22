using FrameClient.Socket.Models;
using FrameClient.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FrameClient.Socket.Map
{
    public class Hosts
    {
        public ObservableCollection<ConnectMap> connectMaps = new ObservableCollection<ConnectMap>();
        public NetworkStream kz = null;

        public int serverport = 4999;   //外网主服务端口
        public string outip = "port.jiyiwm.cn";     //外网IP
        public string outip1;   //连接外网的IP

        public int outport;   //外网端口
        public int inport;    //内网端口
        public string inip;      //内网IP
        public TcpClient tc;   //控制器

        public int state = 0;  //运行状态

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="ip">外网IP</param>
        public async Task Start(string ip)
        {
            outip = ip;
            tc = new TcpClient();
            try { await tc.ConnectAsync(outip, serverport); }
            catch { throw new Exception("无法连接到" + ip); }
            kz = tc.GetStream();
            string[] s = {
                "check",
                Res.version,
                Environment.OSVersion.ToString(),
                Environment.MachineName,
                Environment.UserName,
                OtherHelper.GetInfo(),
            };
            await kz.H_Send(string.Join(",", s));
            string[] ss = (await kz.H_RecvAsync()).Split(',');
            if (ss[0] == "0")
            {
                throw new Exception(ss[1]);
            }

        }

        /// <summary>
        /// 打开端口
        /// </summary>
        /// <param name="ip">内网IP</param>
        /// <param name="port1">内网端口</param>
        /// <param name="port2">外网对外端口</param>
        /// <returns></returns>
        public async Task Open(string ip, string port1, string port2)
        {
            try
            {
                inport = Convert.ToInt32(port1);
                inip = ip;
                TcpClient tc1 = new TcpClient(inip, inport);
                tc1.Close();

                string[] ss = {
                    "1",
                    port2,
                    inip,
                    inport.ToString(),
                };
                await kz.H_Send(string.Join(",", ss));
                string s = await kz.H_RecvAsync();
                string[] r = s.Split(',');
                if (r[0] == "0")
                {
                    throw new Exception($"{outip}服务器的{port2}端口已经被占用");
                }
                else if (r[0] == "10")
                {
                    outip1 = r[1];
                    new Thread(StartLisen) { IsBackground = true }.Start();
                }
                else
                {
                    throw new Exception("返回数据未知");
                }
            }
            catch (Exception e)
            {
                throw new Exception("连接发生错误" + e.Message);
            }
        }

        public void CheckConnect()
        {
            foreach (var l in connectMaps)
            {
                if (!l.InClient.Connected || !l.OutClient.Connected)
                {
                    l.InClient.Close();
                    l.InClient.Dispose();
                    l.OutClient.Close();
                    l.OutClient.Dispose();
                    l.State = 0;
                }
            }

            List<ConnectMap> ss = connectMaps.Where(l => l.State == 0).ToList();
            ss.ForEach(l => Dis(() =>
            {
                connectMaps.Remove(l);
                SetIndex();
            }));
        }

        /// <summary>
        /// 关闭服务
        /// </summary>
        public async Task Close()
        {
            await kz.H_Send($"0");
            kz.Dispose();
            tc.Close();
        }

        /// <summary>
        /// 开启监听
        /// </summary>
        private void StartLisen()
        {
            try
            {
                while (true)
                {
                    string s = kz.H_Recv();
                    string[] r = s.Split(',');
                    if (r[0] == "1")
                    {
                        outport = Convert.ToInt32(r[1]);
                        TcpClient tc1 = new TcpClient(inip, inport);
                        TcpClient tc2 = new TcpClient(outip, outport);
                        ConnectMap connectMap = new ConnectMap { InClient = tc1, OutClient = tc2, Outip = outip1 };
                        Dis(() =>
                        {
                            connectMaps.Add(connectMap);
                            SetIndex();
                        });

                        PortHelper.Lianjie(connectMap);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Logger("客户端开启监听出错 " + ex.Message);
                kz.Dispose();
                tc.Close();
            }
        }


        private void SetIndex()
        {
            int i = 1;
            foreach (var l in connectMaps)
            {
                l.Index = i++;
            }
        }

        private void Dis(Action action)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                SynchronizationContext.SetSynchronizationContext(new
                        System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                SynchronizationContext.Current.Post(pl =>
                {
                    action();
                }, null);
            });
        }
    }
}
