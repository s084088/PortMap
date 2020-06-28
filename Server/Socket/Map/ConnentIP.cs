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
    public class ConnentIP
    {
        public ConnentHost connentHost;
        public TcpClient tc;     //主链接
        public NetworkStream ns;//主链接
        public string IP;        //对应客户端IP
        public int Port;         //对应客户端端口
        public string machineCode;//机器码
        public List<ConnentPort> connentPorts = new List<ConnentPort>();
        public string key = KeyHelper.GenerateKey();

        /// <summary>
        /// 接收到客户端连接
        /// </summary>
        /// <param name="tcpClient"></param>
        public void ClientConn()
        {
            ns = tc.GetStream();
            IPEndPoint iPEndPoint = tc.Client.RemoteEndPoint as IPEndPoint;
            IP = iPEndPoint.Address.ToString();
            Port = iPEndPoint.Port;
            LogHelper.Logger(key + "  :连接  " + IP + Port);
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
                if (r[1] != "1.0.1")
                {
                    ns.H_Send("fail,客户端版本过低 请更新版本");
                    connentHost.serverModels.FirstOrDefault(l => l.machineCode == r[5])?.DisClientConn();
                    machineCode = r[5];
                }
                else
                {
                    ns.H_Send("pass");
                }
            }
            else if (r[0] == "stop")    //申请关闭服务
            {
                DisClientConn();
            }
            else if (r[0] == "s_Start")    //申请开启服务
            {
                int outPort = Convert.ToInt32(r[1]);
                bool b = PortHelper.PortInUse(outPort);
                if (b) //如果申请的端口正在使用
                {
                    ns.H_Send("s_PortOccupied," + r[4]);   //拒绝
                }
                else
                {
                    ns.H_Send("s_PortAccess," + r[4]);   //同意
                    ConnentPort connentPort = new ConnentPort()
                    {
                        connentIP = this,
                        InPort = PortHelper.GetFirstAvailablePort(),
                        OutPort = outPort,
                        ClientIP = r[2],
                        ClientPort = Convert.ToInt32(r[3]),
                        ID = r[4],
                    };
                    connentPort.Start();
                    connentPorts.Add(connentPort);
                }
            }

            else if (r[0] == "s_Stop")    //申请关闭服务
            {
                ConnentPort connentPort = connentPorts.FirstOrDefault(l => l.ID == r[1]);
                if (connentPort != null)
                {
                    connentPort.DisClientConn();
                    connentPorts.Remove(connentPort);
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
                foreach (ConnentPort connentPort in connentPorts) connentPort.DisClientConn();
                ns.Dispose();
                tc.Close();
                tc.Dispose();
                connentPorts.Clear();
                connentHost.serverModels.Remove(this);
            }
            catch (Exception ex)
            {
                LogHelper.Logger("IP类回收资源出错 " + ex.Message);
            }
        }
    }
}
