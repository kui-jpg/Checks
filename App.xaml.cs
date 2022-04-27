using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Checks
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static Mutex mutex;
        public App()
        {
            this.Startup += new StartupEventHandler(App_Startup);
        }

        void App_Startup(object snder, StartupEventArgs e)
        {
            bool ret;
            mutex = new Mutex(true, "Checks", out ret);
            if (!ret)
            {
                Environment.Exit(0);
            }
        }
    }
}
