using System;
using System.Diagnostics;
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
        private DispatcherTimer dispatcherTimer;
        private TimeSpan shortTimer = new TimeSpan(0, 0, 1);
        private TimeSpan longTimer = new TimeSpan(0, 0, 8);
        
        private void initTimer(object sender, EventArgs e)
        {
            if (dispatcherTimer == null)
            {
                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Interval = shortTimer;
                dispatcherTimer.Start();
                dispatcherTimer.Tick += (sd, ea) => { HandleTimer(sender, e); };
            }
        }
        void App_Startup(object sender, StartupEventArgs e)
        {
            initTimer(sender,e);
            this.mainWindow = new MainWindow();
            mainWindow.Show();
            mainWindow.refreshData();
        }

        private void HandleTimer(object sender, EventArgs e)
        {
            mainWindow.refreshData();
        }

        private void Application_Activated(object sender, EventArgs e)
        {
            initTimer(sender, e);
            mainWindow.refreshData();
            dispatcherTimer.Interval = shortTimer;
        }

        private void Application_Deactivated(object sender, EventArgs e)
        {
            initTimer(sender, e);
            dispatcherTimer.Interval = longTimer;
        }
    }
}
