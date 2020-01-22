using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Client.Socket.Models
{
    public class ClientModel
    {
        public List<ConnectMap> connectMaps;
    }

    public class ConnectMap
    {
        public TcpClient inClient;

        public TcpClient outClient;

        internal int state = 1;
    }
}
