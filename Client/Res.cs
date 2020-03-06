using Client.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public static class Res
    {
        public static Mutex mutex;

        public static string version = "1.0.1";

        public static LogHelper LogHelper;

        public static void RunUIAction(Action action)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                SynchronizationContext.Current.Post(pl => action.Invoke(), null);
            });
        }
    }
}
