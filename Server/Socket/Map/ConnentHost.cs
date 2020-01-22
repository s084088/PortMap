using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Socket.Map
{
    public class ConnentHost
    {
        /// <summary>
        /// 主机列表
        /// </summary>
        public List<ConnentIP> serverModels = new List<ConnentIP>();

        /// <summary>
        /// 启动服务
        /// </summary>
        public void StartServer()
        {
            TcpListener tl = new TcpListener(IPAddress.Any, 4999);
            tl.Start();
            while (true)
            {
                TcpClient tc = tl.AcceptTcpClient();
                ConnentIP client = new ConnentIP { tc = tc, connentHost = this };
                serverModels.Add(client);
                new Thread(client.ClientConn) { IsBackground = true }.Start();
            }
        }
    }
}
