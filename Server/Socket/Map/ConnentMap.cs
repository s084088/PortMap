using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
                outClient.GetStream().Dispose();
                inClient.Close();
                outClient.Close();
                inClient.Dispose();
                outClient.Dispose();
                try
                {
                    ConnentPort.connectMaps.Remove(this);
                }
                catch { }
            }
            catch (Exception ex)
            {
                LogHelper.Logger("转发模块回收资源出错 " + ex.Message);
            }
        }
    }
}
