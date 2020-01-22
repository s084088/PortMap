using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Util;

namespace Server.Socket.Map
{
    public class ConnentPort
    {
        public ConnentIP connentIP; //主链接
        public string ID;           //标识ID
        public int InPort;          //对内端口
        public int OutPort;         //对外端口
        public string ClientIP;     //内网IP
        public int ClientPort;      //内网端口

        TcpClient waitClient;       //候选链接

        //连接池
        public List<ConnentMap> connectMaps = new List<ConnentMap>();


        internal void Start()
        {
            new Thread(StartLisen1) { IsBackground = true }.Start();
            new Thread(StartLisen2) { IsBackground = true }.Start();
        }

        /// <summary>
        /// 开启客户端口并监听
        /// </summary>
        public void StartLisen1()
        {
            TcpListener tl = new TcpListener(IPAddress.Any, InPort);
            tl.Start();

            while (true)
            {
                try
                {
                    TcpClient tc = tl.AcceptTcpClient();
                    //接收到连接
                    waitClient = tc;
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
            TcpListener tl = new TcpListener(IPAddress.Any, OutPort);
            tl.Start();
            while (true)
            {
                try
                {
                    TcpClient tc = tl.AcceptTcpClient();
                    IPEndPoint iPEndPoint = tc.Client.RemoteEndPoint as IPEndPoint;
                    string IP = iPEndPoint.Address.ToString();
                    int Port = iPEndPoint.Port;

                    waitClient = null;
                    connentIP.ns.H_Send($"s_StartListen,{ID},{InPort},{IP},{Port}");
                    int i = 0;
                    while (waitClient == null)
                    {
                        Thread.Sleep(1);
                        i++;
                        if (i > 1000) throw new Exception("等待客户端失败");
                    }
                    //接收到连接
                    PortHelper.Lianjie(tc, waitClient);
                    //检查连接情况
                    connectMaps.Add(new ConnentMap
                    {
                        inClient = tc,
                        outClient = waitClient,
                        ConnentPort = this,
                    });
                }
                catch
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 客户端退出,回收资源
        /// </summary>
        /// <param name="serverModel"></param>
        public void DisClientConn()
        {
            try
            {
                foreach (ConnentMap connentMap in connectMaps) connentMap.DisClientConn();
                connectMaps.Clear();
                try { connentIP.connentPorts.Remove(this); } catch { }
            }
            catch (Exception ex)
            {
                LogHelper.Logger("端口类回收资源出错 " + ex.Message);
            }
        }
    }
}
