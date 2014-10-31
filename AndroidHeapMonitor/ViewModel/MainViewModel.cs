using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using AndroidHeapMonitor.Logic;
using GalaSoft.MvvmLight.Command;
using Managed.Adb;

namespace AndroidHeapMonitor.ViewModel
{
    public class MainViewModel : ViewModel
    {
        private AndroidDebugBridge _bridge;
        private string _title;
        private Device _device;
        private string _packageName;
        private string _output;

        private Timer _refreshTimer;
        private DumpsysMeminfo _dumpsysMeminfo;

        public MainViewModel(AndroidDebugBridge bridge)
        {
            _bridge = bridge;
            CloseCommand = new RelayCommand(OnClose);
            StartCommand = new RelayCommand(OnStart);
            StopCommand = new RelayCommand(OnStop);
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
            _device = null;
            _bridge = null;
            Application.Current.Shutdown();
        }

        public void Init()
        {
            var devices = _bridge.Devices;
            _device = devices.FirstOrDefault();

            if (_device != null)
            {
                Title = String.Format("Connected to device: {0}-{1}", _device.Model, _device.SerialNumber);
            }
            else
            {
                Title = "Not connected";
            }

            _refreshTimer = new Timer(2500);
            _refreshTimer.AutoReset = true;
            _refreshTimer.Elapsed += _refreshTimer_Elapsed;
            _dumpsysMeminfo = new DumpsysMeminfo(_device);
        }

        void _refreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var output = _dumpsysMeminfo.GetInfo(PackageName);

            Output = output;
        }


        public ICommand CloseCommand { get; private set; }
        public ICommand StartCommand { get; private set; }
        public ICommand StopCommand { get; private set; }

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

        public string PackageName
        {
            get { return _packageName; }
            set
            {
                if (value == _packageName) return;
                _packageName = value;
                OnPropertyChanged();
            }
        }

        public string Output
        {
            get { return _output; }
            set
            {
                if (value == _output) return;
                _output = value;
                OnPropertyChanged();
            }
        }

    }
}
