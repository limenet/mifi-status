using System;
using System.Windows;
using System.Windows.Threading;

namespace mifi_status
{
    /// <inheritdoc />
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private MainWindow _mainWindow;
        private DispatcherTimer _dispatcherTimer;
        private readonly TimeSpan _shortTimer = new TimeSpan(0, 0, 1);
        private readonly TimeSpan _longTimer = new TimeSpan(0, 0, 8);
        
        private void InitTimer()
        {
            if (_dispatcherTimer != null) return;

            _dispatcherTimer = new DispatcherTimer {Interval = _shortTimer};
            _dispatcherTimer.Start();
            _dispatcherTimer.Tick += (sd, ea) => { HandleTimer(); };
        }
        void App_Startup(object sender, StartupEventArgs e)
        {
            InitTimer();
            _mainWindow = new MainWindow();
            _mainWindow.Show();
            _mainWindow.RefreshData();
        }

        private void HandleTimer()
        {
            _mainWindow.RefreshData();
        }

        private void Application_Activated(object sender, EventArgs e)
        {
            InitTimer();
            _mainWindow.RefreshData();
            _dispatcherTimer.Interval = _shortTimer;
        }

        private void Application_Deactivated(object sender, EventArgs e)
        {
            InitTimer();
            _dispatcherTimer.Interval = _longTimer;
        }
    }
}
