using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server.Socket.Models
{
    /// <summary>
    /// 单个服务模型
    /// </summary>
    public class ServerModel
    {
        //标识
        public string key;
        //连接状态
        public int state = 1;
        //对内主机
        public string ip;
        //对内端口
        public int inport;
        //对外端口
        public int outport;

        //主通信
        public TcpClient MainClient;

        public TcpListener lisenClient;
        public TcpListener lisenClient1;

        //候选连接
        public TcpClient waitClient;

        //连接池
        public List<ConnectMap> connectMaps = new List<ConnectMap>();
    }


    public class ConnectMap
    {
        public TcpClient inClient;

        public TcpClient outClient;

        public int state =1;
    }
}
