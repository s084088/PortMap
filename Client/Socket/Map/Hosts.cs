using Client.Socket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Util;

namespace Client.Socket.Map
{
    public static class Hosts
    {
        public static List<ConnectMap> connectMaps = new List<ConnectMap>();
        public static NetworkStream kongzhins = null;

        public static int serverport = 4999;   //外网主服务端口
        public static string outip = "jiyiwm.cn";     //外网IP

        public static int outport;   //外网端口
        public static int inport;    //内网端口
        public static string inip;      //内网IP

        public static TcpClient tc1;
        public static TcpClient tc2;

        public static TcpClient tc;
        public static void Start()
        {
            tc = new TcpClient(outip, serverport);
            kongzhins = tc.GetStream();
        }

        /// <summary>
        /// 打开端口
        /// </summary>
        /// <param name="ip">内网IP</param>
        /// <param name="port1">内网端口</param>
        /// <param name="port2">外网对外端口</param>
        /// <returns></returns>
        public static void Open(string ip, string port1, string port2)
        {
            try
            {
                inport = Convert.ToInt32(port1);
                inip = ip;

                kongzhins.H_Send($"1,{port2}");
                string s = kongzhins.H_Recv();
                string[] r = s.Split(',');
                if (r[0] == "0")
                {
                    throw new Exception("此服务器端口已经被占用");
                }
                else if (r[0] == "1")
                {
                    outport = Convert.ToInt32(r[1]);
                    tc1 = new TcpClient(inip, inport);
                    tc2 = new TcpClient(outip, outport);

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

        private static void CheckConnect()
        {
            connectMaps.ForEach(l =>
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
            List<ConnectMap> ss = connectMaps.Where(l => l.state == 0).ToList();
            ss.ForEach(l =>
            {
                connectMaps.Remove(l);
            });
        }

        /// <summary>
        /// 关闭服务
        /// </summary>
        public static void Close()
        {
            kongzhins.H_Send($"0");
            kongzhins.Dispose();
            tc.Close();
        }

        /// <summary>
        /// 开启监听
        /// </summary>
        private static void StartLisen()
        {
            try
            {
                connectMaps.Add(new ConnectMap
                {
                    inClient = tc1,
                    outClient = tc2,
                });
                PortHelper.Lianjie(tc1, tc2);
                while (true)
                {
                    string s = kongzhins.H_Recv();
                    string[] r = s.Split(',');
                    if (r[0] == "1")
                    {
                        outport = Convert.ToInt32(r[1]);
                        TcpClient tc1 = new TcpClient(inip, inport);
                        TcpClient tc2 = new TcpClient(outip, outport);
                        connectMaps.Add(new ConnectMap
                        {
                            inClient = tc1,
                            outClient = tc2,
                        });
                        PortHelper.Lianjie(tc1, tc2);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Logger("客户端开启监听出错 " + ex.Message);
                kongzhins.Dispose();
                tc.Close();
            }
        }
    }
}
