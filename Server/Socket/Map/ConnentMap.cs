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
                inClient.Close();
                inClient.Dispose();
            }
            catch (Exception ex)
            {
                LogHelper.Logger("内网转发模块回收资源出错 " + ex.Message);
            }
            try
            {
                outClient.GetStream().Dispose();
                outClient.Close();
                outClient.Dispose();
            }
            catch (Exception ex)
            {
                LogHelper.Logger("外网转发模块回收资源出错 " + ex.Message);
            }
        }
    }
}
