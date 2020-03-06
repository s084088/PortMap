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

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <param name="tc1"></param>
        /// <param name="tc2"></param>
        public static void Lianjie(TcpClient tc1, TcpClient tc2)
        {
            tc1.SendTimeout = 300000;
            tc1.ReceiveTimeout = 300000;
            tc2.SendTimeout = 300000;
            tc2.ReceiveTimeout = 300000;
            object obj1 = (object)(new TcpClient[] { tc1, tc2 });
            object obj2 = (object)(new TcpClient[] { tc2, tc1 });
            ThreadPool.QueueUserWorkItem(new WaitCallback(Transfer), obj1);
            ThreadPool.QueueUserWorkItem(new WaitCallback(Transfer), obj2);
        }

        /// <summary>
        /// 转发
        /// </summary>
        /// <param name="obj"></param>
        public static void Transfer(object obj)
        {
            try
            {
                TcpClient tc1 = ((TcpClient[])obj)[0];
                TcpClient tc2 = ((TcpClient[])obj)[1];
                NetworkStream ns1 = tc1.GetStream();
                NetworkStream ns2 = tc2.GetStream();
                while (true)
                {
                    try
                    {
                        //这里必须try catch，否则连接一旦中断程序就崩溃了，要是弹出错误提示让机主看见那就囧了
                        //LogHelper.Logger("准备接收数据 ");
                        byte[] bt = new byte[10240];
                        int count = ns1.Read(bt, 0, bt.Length);
                        if (count == 0) throw new Exception("连接断开");
                        ns2.Write(bt, 0, count);
                        //LogHelper.Logger("接收到数据 " + BitConverter.ToString(bt, 0, count) + count.ToString());
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            ns1.Dispose();
                            tc1.Close();
                            tc1.Dispose();
                        }
                        catch { }
                        try
                        {
                            ns2.Dispose();
                            tc2.Close();
                            tc2.Dispose();
                        }
                        catch { }
                        LogHelper.Logger("连接关闭 " + ex.Message);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Logger("转发方法出错 " + ex.Message);
            }
        }
    }
}
