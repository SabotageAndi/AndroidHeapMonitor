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
using System.Collections.ObjectModel;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using AndroidMemoryMonitor.Logic;
using GalaSoft.MvvmLight.Command;
using Managed.Adb;

namespace AndroidMemoryMonitor.ViewModel
{
    public class MainViewModel : ViewModel
    {
        private AndroidDebugBridge _bridge;
        private DumpsysMemInfoParser _dumpsysMemInfoParser;

        private int _interval;
        private Timer _refreshTimer;
        private Device _selectedDevice;
        private DumpsysPackages _selectedPackage;
        private string _title;

        public MainViewModel(AndroidDebugBridge bridge)
        {
            _bridge = bridge;
            CloseCommand = new RelayCommand(OnClose);
            StartCommand = new RelayCommand(OnStart);
            StopCommand = new RelayCommand(OnStop);
            RefreshDevicesCommand = new RelayCommand(OnRefreshDevices);
            RefreshPackagesCommand = new RelayCommand(OnPackagesRefresh);

            PlotViewModel = new PlotViewModel();
            Devices = new ObservableCollection<Device>();
            Packages = new ObservableCollection<DumpsysPackages>();
        }


        public ICommand CloseCommand { get; private set; }
        public ICommand StartCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand RefreshPackagesCommand { get; set; }
        public ICommand RefreshDevicesCommand { get; private set; }

        public PlotViewModel PlotViewModel { get; set; }
        public ObservableCollection<Device> Devices { get; set; }
        public ObservableCollection<DumpsysPackages> Packages { get; set; }

        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        public int Interval
        {
            get { return _interval; }
            set
            {
                if (value == _interval) return;
                _interval = value;
                OnPropertyChanged();
                PlotViewModel.Interval = _interval;
            }
        }


        public Device SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                _selectedDevice = value;
                OnPropertyChanged();
                OnDeviceSelected();
                PlotViewModel.SelectedDevice = _selectedDevice;
                RaisePropertyChanged(() => DeviceSelected);
            }
        }

        public bool DeviceSelected
        {
            get { return SelectedDevice != null; }
        }

        public bool PackageSelected
        {
            get { return SelectedPackage != null; }
        }


        public DumpsysPackages SelectedPackage
        {
            get { return _selectedPackage; }
            set
            {
                if (Equals(value, _selectedPackage)) return;
                _selectedPackage = value;
                OnPropertyChanged();
                PlotViewModel.SelectedPackage = _selectedPackage;
                RaisePropertyChanged(() => PackageSelected);
            }
        }


        private void OnRefreshDevices()
        {
            SelectedDevice = null;
            Devices.Clear();
            foreach (Device device in _bridge.Devices)
            {
                Devices.Add(device);
            }
        }

        private void OnStop()
        {
            _refreshTimer.Stop();
        }

        private void OnStart()
        {
            _refreshTimer.Start();
        }

        private void OnClose()
        {
            _bridge = null;
            Application.Current.Shutdown();
        }

        public void Init()
        {
            OnRefreshDevices();

            Interval = 2500;
            _refreshTimer = new Timer(Interval);
            _refreshTimer.AutoReset = false;
            _refreshTimer.Elapsed += _refreshTimer_Elapsed;
            _dumpsysMemInfoParser = new DumpsysMemInfoParser();

            PlotViewModel.InitPlotModel();
        }

        private void _refreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PlotViewModel.Update();

            _refreshTimer.Start();
        }

        private void OnDeviceSelected()
        {
            if (_selectedDevice != null)
            {
                Title = String.Format("Connected to device: {0}-{1}", _selectedDevice.Model,
                    _selectedDevice.SerialNumber);

                OnPackagesRefresh();
            }
            else
            {
                Title = "Not connected";
                Packages.Clear();
            }
        }

        private void OnPackagesRefresh()
        {
            SelectedPackage = null;
            Packages.Clear();
            _dumpsysMemInfoParser.ParsePackages(Dumpsys.GetMeminfo(SelectedDevice)).ForEach(i => Packages.Add(i));
        }

    
    }
}