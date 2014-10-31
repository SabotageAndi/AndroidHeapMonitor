using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AndroidHeapMonitor.View;
using AndroidHeapMonitor.ViewModel;
using Managed.Adb;

namespace AndroidHeapMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private AndroidDebugBridge _bridge;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            _bridge = StartBridge(@"C:\android\platform-tools\adb.exe");
    

            var mainWindow = new MainWindow();
            var mainViewModel = new MainViewModel(_bridge);

            mainViewModel.Init();

            mainWindow.DataContext = mainViewModel;

            mainWindow.Show();

        }

        private AndroidDebugBridge StartBridge(String location)
        {
            AndroidDebugBridge adb = AndroidDebugBridge.CreateBridge(location, false);
            return adb;
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            _bridge.Stop();
            _bridge = null;
        }
    }
}
