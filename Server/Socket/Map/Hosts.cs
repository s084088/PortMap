using Server.Socket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Util;

namespace Server.Socket.Map
{
    public class Hosts
    {
        /// <summary>
        /// 主机列表
        /// </summary>
        public static List<ServerModel> serverModels = new List<ServerModel>();

        /// <summary>
        /// 启动服务
        /// </summary>
        public static void StartServer()
        {
            TcpListener tl = new TcpListener(IPAddress.Any, 4999);
            tl.Start();
            while (true)
            {
                TcpClient tc = tl.AcceptTcpClient();
                Client client = new Client { tc = tc };
                new Thread(client.ClientConn) { IsBackground = true }.Start();
            }
        }
    }
}