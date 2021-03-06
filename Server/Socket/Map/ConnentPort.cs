﻿using System;
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
        TcpListener t1;
        TcpListener t2;

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
            t1 = new TcpListener(IPAddress.Any, InPort);
            t1.Start();

            while (true)
            {
                try
                {
                    TcpClient tc = t1.AcceptTcpClient();
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
            t2 = new TcpListener(IPAddress.Any, OutPort);
            t2.Start();
            while (true)
            {
                try
                {
                    TcpClient tc = t2.AcceptTcpClient();
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
                    //检查连接情况
                    ConnentMap connentMap = new ConnentMap
                    {
                        inClient = tc,
                        outClient = waitClient,
                        ConnentPort = this,
                    };
                    connentMap.Lianjie();
                    connectMaps.Add(connentMap);
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
            foreach (ConnentMap connentMap in connectMaps) connentMap.DisClientConn();
            try
            {
                t1.Stop();
            }
            catch (Exception ex)
            {
                LogHelper.Logger("内网端口类回收资源出错 " + ex.Message);
            }
            try
            {
                t2.Stop();
            }
            catch (Exception ex)
            {
                LogHelper.Logger("外网端口类回收资源出错 " + ex.Message);
            }
            connectMaps.Clear();
        }
    }
}
