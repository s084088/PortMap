using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Util;

namespace Server.Socket.Map
{
    public class ConnentMap
    {
        public ConnentPort ConnentPort;

        public TcpClient inClient;

        public TcpClient outClient;

        public int state = 1;

        internal void DisClientConn()
        {
            try
            {
                inClient.GetStream().Dispose();
                inClient.Close();
                inClient.Dispose();
            }
            catch (Exception ex)
            {
                LogHelper.Logger("转发模块内网回收资源出错 " + ex.Message);
            }
            try
            {
                outClient.GetStream().Dispose();
                outClient.Close();
                outClient.Dispose();
            }
            catch (Exception ex)
            {
                LogHelper.Logger("转发模块外网回收资源出错 " + ex.Message);
            }
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <param name="tc1"></param>
        /// <param name="tc2"></param>
        public void Lianjie()
        {
            inClient.SendTimeout = 86400000;
            inClient.ReceiveTimeout = 86400000;
            outClient.SendTimeout = 86400000;
            outClient.ReceiveTimeout = 86400000;
            ThreadPool.QueueUserWorkItem(new WaitCallback(Transfer));
            ThreadPool.QueueUserWorkItem(new WaitCallback(Transfer1));
        }

        /// <summary>
        /// 转发
        /// </summary>
        /// <param name="obj"></param>
        public void Transfer(object obj)
        {
            try
            {
                NetworkStream ns1 = inClient.GetStream();
                NetworkStream ns2 = outClient.GetStream();
                while (true)
                {
                    try
                    {
                        //这里必须try catch，否则连接一旦中断程序就崩溃了，要是弹出错误提示让机主看见那就囧了
                        //LogHelper.Logger("准备接收数据 ");
                        byte[] bt = new byte[10240];
                        int count = ns1.Read(bt, 0, bt.Length);
                        if (count == 0) throw new Exception("转发模组内网连接断开");
                        ns2.Write(bt, 0, count);
                        //LogHelper.Logger("接收到数据 " + BitConverter.ToString(bt, 0, count) + count.ToString());
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            ns1.Dispose();
                            inClient.Close();
                            inClient.Dispose();
                        }
                        catch { }
                        try
                        {
                            ns2.Dispose();
                            outClient.Close();
                            outClient.Dispose();
                        }
                        catch { }
                        LogHelper.Logger("转发模组内网连接关闭 " + ex.Message);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Logger("转发模组内网出错 " + ex.Message);
            }
        }

        /// <summary>
        /// 转发
        /// </summary>
        /// <param name="obj"></param>
        public void Transfer1(object obj)
        {
            try
            {
                NetworkStream ns1 = outClient.GetStream();
                NetworkStream ns2 = inClient.GetStream();
                while (true)
                {
                    try
                    {
                        //这里必须try catch，否则连接一旦中断程序就崩溃了，要是弹出错误提示让机主看见那就囧了
                        //LogHelper.Logger("准备接收数据 ");
                        byte[] bt = new byte[10240];
                        int count = ns1.Read(bt, 0, bt.Length);
                        if (count == 0) throw new Exception("转发模组外网连接断开");
                        ns2.Write(bt, 0, count);
                        //LogHelper.Logger("接收到数据 " + BitConverter.ToString(bt, 0, count) + count.ToString());
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            ns2.Dispose();
                            inClient.Close();
                            inClient.Dispose();
                        }
                        catch { }
                        try
                        {
                            ns1.Dispose();
                            outClient.Close();
                            outClient.Dispose();
                        }
                        catch { }
                        LogHelper.Logger("转发模组外网连接关闭 " + ex.Message);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Logger("转发模组外网出错 " + ex.Message);
            }
        }
    }
}
