//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using AndroidMemoryMonitor.View;
using AndroidMemoryMonitor.ViewModel;
using Managed.Adb;

namespace AndroidMemoryMonitor
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private List<String> _knownAdbLocations = new List<string>()
                                                  {
                                                      @"C:\android\platform-tools\adb.exe",
                                                      @"D:\android\platform-tools\adb.exe",
                                                  };

        private AndroidDebugBridge _bridge;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            string adbLocation = null;

            foreach (var knownAdbLocation in _knownAdbLocations)
            {
                if (File.Exists(knownAdbLocation))
                {
                    adbLocation = knownAdbLocation;
                    break;
                }
            }

            if (String.IsNullOrEmpty(adbLocation))
            {
                MessageBox.Show("ADB not found");
                return;
            }

            _bridge = StartBridge(adbLocation);
    

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
