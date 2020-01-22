using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Helper
{
    public class LogHelper
    {
        /// <summary>
        /// 记录消息Queue
        /// </summary>
        private readonly ConcurrentQueue<FlashLogMessage> _que = new ConcurrentQueue<FlashLogMessage>();

        /// <summary>
        /// 信号
        /// </summary>
        private readonly ManualResetEvent _mre = new ManualResetEvent(false);

        /// <summary>
        /// 记录线程
        /// </summary>
        private Thread _td;

        /// <summary>
        /// 默认文件名
        /// </summary>
        private string fileName;

        /// <summary>
        /// 开启线程记录日志，只在程序初始化时调用一次
        /// </summary>
        public LogHelper() : this("log.txt")
        {

        }

        /// <summary>
        /// 开启线程记录日志，只在程序初始化时调用一次
        /// </summary>
        public LogHelper(string fileName)
        {
            this.fileName = fileName;
            _td?.Abort();
            _td = new Thread(new ThreadStart(WriteLog)) { IsBackground = true };
            _td.Start();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            _td?.Abort();
        }

        /// <summary>
        /// 从队列中写日志至磁盘
        /// </summary>
        private void WriteLog()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            while (true)
            {
                // 等待信号通知
                _mre.WaitOne();

                // 判断是否有内容需要如磁盘 从列队中获取内容，并删除列队中的内容
                while (_que.Count > 0 && _que.TryDequeue(out FlashLogMessage msg))
                {
                    System.Diagnostics.Debug.WriteLine(msg.Message);
                    // 判断日志等级，然后写日志
                    string logContent = string.Empty;
                    switch (msg.Level)
                    {
                        case FlashLogLevel.Debug:
                            logContent = $"[Debug]:  {msg.Message}   { msg.Exception}\r\n";
                            break;
                        case FlashLogLevel.Info:
                            logContent = $"[Info]:  {msg.Message}   { msg.Exception}\r\n";
                            break;
                        case FlashLogLevel.Error:
                            logContent = $"[Error]:  {msg.Message}   { msg.Exception}\r\n";
                            break;
                        case FlashLogLevel.Warn:
                            logContent = $"[Warn]:  {msg.Message}   { msg.Exception}\r\n";
                            break;
                        case FlashLogLevel.Fatal:
                            logContent = $"[Fatal]:  {msg.Message}   { msg.Exception}\r\n";
                            break;
                    }
                    File.AppendAllText(filePath, logContent);
                }

                // 重新设置信号
                _mre.Reset();
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="message">日志文本</param>
        /// <param name="level">等级</param>
        /// <param name="ex">Exception</param>
        private void EnqueueMessage(string message, FlashLogLevel level, Exception ex = null)
        {

            _que.Enqueue(new FlashLogMessage
            {
                Message = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff") + "]:  " + message + (ex == null ? "" : "\r\n"),
                Level = level,
                Exception = ex
            });

            // 通知线程往磁盘中写日志
            _mre.Set();
        }

        public void Debug(string msg, Exception ex = null)
        {
            EnqueueMessage(msg, FlashLogLevel.Debug, ex);
        }

        public void Error(string msg, Exception ex = null)
        {
            EnqueueMessage(msg, FlashLogLevel.Error, ex);
        }

        public void Fatal(string msg, Exception ex = null)
        {
            EnqueueMessage(msg, FlashLogLevel.Fatal, ex);
        }

        public void Info(string msg, Exception ex = null)
        {
            EnqueueMessage(msg, FlashLogLevel.Info, ex);
        }

        public void Warn(string msg, Exception ex = null)
        {
            EnqueueMessage(msg, FlashLogLevel.Warn, ex);
        }

    }

    /// <summary>
    /// 日志等级
    /// </summary>
    internal enum FlashLogLevel
    {
        Debug,
        Info,
        Error,
        Warn,
        Fatal
    }


    /// <summary>
    /// 日志内容
    /// </summary>
    internal class FlashLogMessage
    {
        /// <summary>
        /// 内容
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 等级
        /// </summary>
        public FlashLogLevel Level { get; set; }
        /// <summary>
        /// 错误堆栈
        /// </summary>
        public Exception Exception { get; set; }

    }
}
