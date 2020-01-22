using Server.Socket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Util;

namespace Server.Socket.Map
{
    public class Client
    {
        public TcpClient tc;     //主链接
        private NetworkStream ns;//主链接
        string key;
        ServerModel serverModel;

        /// <summary>
        /// 接收到客户端连接
        /// </summary>
        /// <param name="tcpClient"></param>
        public void ClientConn()
        {
            ns = tc.GetStream();
            key = KeyHelper.GenerateKey();
            serverModel = new ServerModel
            {
                key = key,
                MainClient = tc,
            };
            Hosts.serverModels.Add(serverModel);
            LogHelper.Logger(key + "  :连接  " + (tc.Client.RemoteEndPoint as IPEndPoint).Address.ToString());
            while (true)
            {
                try
                {
                    string s = ns.H_Recv();
                    LogHelper.Logger(key + "  :接收  " + s);
                    Analysis(s);
                }
                catch
                {
                    DisClientConn();
                    LogHelper.Logger(key + "  :断开");
                    break;
                }
            }
        }

        /// <summary>
        /// 解析数据
        /// 0,申请关闭服务
        /// 1,申请开启服务,带端口
        /// </summary>
        /// <param name="s"></param>
        public void Analysis(string s)
        {
            string[] r = s.Split(',');
            if (r[0] == "check")    //验证
            {
                if (r[1] != "1.0.0")
                {
                    ns.H_Send("0,客户端版本过低,请更新版本");
                }
                else
                {
                    ns.H_Send("pass");
                }
            }
            else if (r[0] == "1")    //申请开启服务
            {
                int i = Convert.ToInt32(r[1]);
                bool b = PortHelper.PortInUse(i);
                if (b) //如果申请的端口正在使用
                {
                    ns.H_Send("0");   //拒绝
                }
                else
                {
                    ns.H_Send("10," + (tc.Client.RemoteEndPoint as IPEndPoint).Address.ToString());   //同意
                    int p = PortHelper.GetFirstAvailablePort();
                    serverModel.inport = p;
                    serverModel.outport = i;
                    new Thread(StartLisen1) { IsBackground = true }.Start();
                    new Thread(StartLisen2) { IsBackground = true }.Start();
                }
            }
            else if (r[0] == "0")    //申请关闭服务
            {
                DisClientConn();
            }
        }

        /// <summary>
        /// 开启客户端口并监听
        /// </summary>
        public void StartLisen1()
        {
            TcpListener tl = new TcpListener(IPAddress.Any, serverModel.inport);
            serverModel.lisenClient = tl;
            tl.Start();

            while (true)
            {
                try
                {
                    TcpClient tc = tl.AcceptTcpClient();
                    //接收到连接
                    serverModel.waitClient = tc;
                }
                catch
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 开启服务端口并监听
        /// </summary>
        public void StartLisen2()
        {
            TcpListener tl = new TcpListener(IPAddress.Any, serverModel.outport);
            serverModel.lisenClient1 = tl;
            tl.Start();
            while (true)
            {
                try
                {
                    TcpClient tc = tl.AcceptTcpClient();

                    serverModel.waitClient = null;
                    ns.H_Send($"1,{serverModel.inport}");
                    int i = 0;
                    while (serverModel.waitClient == null)
                    {
                        Thread.Sleep(1);
                        i++;
                        if (i > 1000) throw new Exception("等待客户端失败");
                    }
                    //接收到连接
                    PortHelper.Lianjie(tc, serverModel.waitClient);
                    //检查连接情况
                    CheckConnect();
                    serverModel.connectMaps.Add(new ConnectMap
                    {
                        inClient = tc,
                        outClient = serverModel.waitClient
                    });

                }
                catch
                {
                    break;
                }
            }
        }

        private void CheckConnect()
        {
            serverModel.connectMaps.ForEach(l =>
            {
                if (l.inClient.Connected && l.outClient.Connected) { }
                else
                {
                    l.inClient.Close();
                    l.inClient.Dispose();
                    l.outClient.Close();
                    l.outClient.Dispose();
                    l.state = 0;
                }
            });
            List<ConnectMap> ss = serverModel.connectMaps.Where(l => l.state == 0).ToList();
            ss.ForEach(l =>
            {
                serverModel.connectMaps.Remove(l);
            });
        }

        /// <summary>
        /// 客户端退出,回收资源
        /// </summary>
        /// <param name="serverModel"></param>
        public void DisClientConn()
        {
            try
            {
                if (serverModel.state == 0) return;
                if (serverModel == null) return;
                serverModel.state = 0;
                serverModel.connectMaps?.ForEach(l =>
                {
                    l.inClient.Close();
                    l.inClient.Dispose();
                    l.outClient.Close();
                    l.outClient.Dispose();
                });
                serverModel.waitClient?.Close();
                serverModel.waitClient?.Dispose();
                serverModel.MainClient?.Close();
                serverModel.MainClient?.Dispose();
                serverModel.lisenClient?.Stop();
                serverModel.lisenClient1?.Stop();
                Hosts.serverModels.Remove(serverModel);
            }
            catch (Exception ex)
            {
                LogHelper.Logger("客户端退出,回收资源出错 " + ex.Message);
            }
        }
    }
}
