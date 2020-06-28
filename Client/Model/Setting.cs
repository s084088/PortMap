using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Model
{
    public class Setting
    {
        public string ServerIP { get; set; }

        public List<SettingPort> SettingPorts { get; set; }
    }

    public class SettingPort
    {
        public string ClientIP { get; set; }

        public int ClientPort { get; set; }

        public int ServerPort { get; set; }

    }
}
