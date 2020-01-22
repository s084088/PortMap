using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FrameClient.Util
{
    public static class LogHelper
    {
        public static void Logger(string s)
        {
            
            try
            {
                string path = Path.Combine("./logs");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                string logFileName = Environment.CurrentDirectory + "\\logs\\"  + DateTime.Now.ToString("yyyy_MM_dd") + ".log";//生成日志文件
                if (!File.Exists(logFileName)) File.Create(logFileName).Close(); //判断日志文件是否为当天
                StreamWriter writer = File.AppendText(logFileName);//文件中添加文件流
                writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + s);
                writer.Flush();
                writer.Close();
            }
            catch (Exception e)
            {
                string path = Path.Combine("./log");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                string logFileName = path + "\\pt" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                if (!File.Exists(logFileName)) File.Create(logFileName);
                StreamWriter writer = File.AppendText(logFileName);
                writer.WriteLine(DateTime.Now.ToString("日志记录错误HH:mm:ss") + " " + e.Message + " " + s);
                writer.Flush();
                writer.Close();
            }

        }
    }
}
