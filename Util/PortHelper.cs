using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Util
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

        
    }
}
