using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Helper
{
    public static class OtherHelper
    {
        /// <summary>
        /// 获取本计算机唯一的机器码
        /// </summary>
        /// <returns>字符串形式的机器码</returns>
        public static string GetInfo()
        {
            string unique = "";
            // 获取处理器信息
            ManagementClass cimobject = new ManagementClass("Win32_Processor");
            ManagementObjectCollection moc = cimobject.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                unique += mo.Properties["ProcessorId"].Value.ToString();
            }
            // 获取硬盘ID  
            ManagementClass cimobject1 = new ManagementClass("Win32_DiskDrive");
            ManagementObjectCollection moc1 = cimobject1.GetInstances();
            foreach (ManagementObject mo in moc1)
            {
                unique += (string)mo.Properties["Model"].Value;
                break;
            }

            // 获取BIOS
            ManagementObjectSearcher searcher =
                new ManagementObjectSearcher("Select SerialNumber From Win32_BIOS");
            ManagementObjectCollection moc2 = searcher.Get();

            if (moc2.Count > 0)
            {
                foreach (ManagementObject share in moc2)
                {
                    unique += share["SerialNumber"].ToString();
                }
            }

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return ByteToHexString(md5.ComputeHash(Encoding.Unicode.GetBytes(unique)), (char)0).Substring(0, 25);
        }


        public static string ByteToHexString(byte[] InBytes, char segment)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte InByte in InBytes)
            {
                if (segment == 0) sb.Append(string.Format("{0:X2}", InByte));
                else sb.Append(string.Format("{0:X2}{1}", InByte, segment));
            }

            if (segment != 0 && sb.Length > 1 && sb[sb.Length - 1] == segment)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        public static void Dis(Action action)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                SynchronizationContext.SetSynchronizationContext(new
                        System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                SynchronizationContext.Current.Post(pl =>
                {
                    action();
                }, null);
            });
        }
    }
}
