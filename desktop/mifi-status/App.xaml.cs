using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace mifi_status
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow mainWindow;
        void App_Startup(object sender, StartupEventArgs e)
        {
            this.mainWindow = new MainWindow();
            mainWindow.Show();
            mainWindow.refreshData();

            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
            dispatcherTimer.Start();
            dispatcherTimer.Tick += (sd, ea) => { HandleTimer(sender, e); };
        }

        private void HandleTimer(object sender, EventArgs e)
        {
            mainWindow.refreshData();
        }
    }
}
