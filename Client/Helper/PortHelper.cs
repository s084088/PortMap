using Client.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Helper
{
    public static class PortHelper
    {
        /// <summary>
        /// 检测端口是否使用
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool PortInUse(int port)
        {
            bool inUse = false;

            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }

            return inUse;
        }

        /// <summary>
        /// 获取第一个可用的端口号
        /// </summary>
        /// <returns></returns>
        public static int GetFirstAvailablePort()
        {
            int MAX_PORT = 6000; //系统tcp/udp端口数最大是65535            
            int BEGIN_PORT = 5000;//从这个端口开始检测

            for (int i = BEGIN_PORT; i < MAX_PORT; i++)
            {
                if (!PortInUse(i)) return i;
            }

            return -1;
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="s"></param>
        public static void H_Send(this NetworkStream ns, string s)
        {
            byte[] bt = Encoding.UTF8.GetBytes(s);//这里发送一个连接提示
            ns.Write(bt, 0, bt.Length);
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="s"></param>
        public static async Task H_SendAsync(this NetworkStream ns, string s)
        {
            byte[] bt = Encoding.UTF8.GetBytes(s);//这里发送一个连接提示
            await ns.WriteAsync(bt, 0, bt.Length);
        }

        /// <summary>
        /// 接收
        /// </summary>
        /// <param name="ns"></param>
        /// <returns></returns>
        public static string H_Recv(this NetworkStream ns)
        {
            byte[] bt = new byte[10240];
            int count = ns.Read(bt, 0, bt.Length);
            if (count == 0) throw new Exception("连接断开");
            return Encoding.UTF8.GetString(bt, 0, count);
        }

        /// <summary>
        /// 接收
        /// </summary>
        /// <param name="ns"></param>
        /// <returns></returns>
        public static async Task<string> H_RecvAsync(this NetworkStream ns)
        {
            byte[] bt = new byte[10240];
            int count = await ns.ReadAsync(bt, 0, bt.Length);
            if (count == 0) throw new Exception("连接断开");
            return Encoding.UTF8.GetString(bt, 0, count);
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <param name="tc1"></param>
        /// <param name="tc2"></param>
        public static void Lianjie(ConnectMap connectMap)
        {
            connectMap.InClient.SendTimeout = 300000;
            connectMap.InClient.ReceiveTimeout = 300000;
            connectMap.OutClient.SendTimeout = 300000;
            connectMap.OutClient.ReceiveTimeout = 300000;
            ThreadPool.QueueUserWorkItem(new WaitCallback(Transfer), connectMap);
            ThreadPool.QueueUserWorkItem(new WaitCallback(Transfer1), connectMap);
        }

        /// <summary>
        /// 转发
        /// </summary>
        /// <param name="obj"></param>
        public static void Transfer(object obj)
        {
            try
            {
                TcpClient tc1 = ((ConnectMap)obj).InClient;
                TcpClient tc2 = ((ConnectMap)obj).OutClient;
                NetworkStream ns1 = tc1.GetStream();
                NetworkStream ns2 = tc2.GetStream();
                while (true)
                {
                    try
                    {
                        byte[] bt = new byte[10240];
                        int count = ns1.Read(bt, 0, bt.Length);
                        if (count == 0) throw new Exception("连接断开");
                        ns2.Write(bt, 0, count);
                        ((ConnectMap)obj).SetIn(count);
                        //LogHelper.Logger("发送 " + BitConverter.ToString(bt, 0, count) + count.ToString());
                    }
                    catch (Exception ex)
                    {
                        ns1.Dispose();
                        ns2.Dispose();
                        tc1.Close();
                        tc2.Close();
                        tc1.Dispose();
                        tc2.Dispose();
                        Res.LogHelper.Info("连接关闭 " + ex.Message);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Res.LogHelper.Info("内网主动断开 " + ex.Message);
            }
        }

        /// <summary>
        /// 转发
        /// </summary>
        /// <param name="obj"></param>
        public static void Transfer1(object obj)
        {
            try
            {
                TcpClient tc2 = ((ConnectMap)obj).InClient;
                TcpClient tc1 = ((ConnectMap)obj).OutClient;
                NetworkStream ns1 = tc1.GetStream();
                NetworkStream ns2 = tc2.GetStream();
                while (true)
                {
                    try
                    {
                        byte[] bt = new byte[10240];
                        int count = ns1.Read(bt, 0, bt.Length);
                        if (count == 0) throw new Exception("连接断开");
                        ns2.Write(bt, 0, count);
                        ((ConnectMap)obj).SetOut(count);
                        //LogHelper.Logger("接收 " + BitConverter.ToString(bt, 0, count) + count.ToString());
                    }
                    catch (Exception ex)
                    {
                        ns1.Dispose();
                        ns2.Dispose();
                        tc1.Close();
                        tc2.Close();
                        tc1.Dispose();
                        tc2.Dispose();
                        Res.LogHelper.Info("外网主动断开 " + ex.Message);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Res.LogHelper.Info("转发方法出错 " + ex.Message);
            }
        }
    }
}
