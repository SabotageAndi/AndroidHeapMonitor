using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Managed.Adb;

namespace AndroidHeapMonitor.ViewModel
{
    public class MainViewModel : ViewModel
    {
        private readonly AndroidDebugBridge _bridge;
        private string _title;
        private Device _device;

        public MainViewModel(AndroidDebugBridge bridge)
        {
            _bridge = bridge;
            CloseCommand = new RelayCommand(() =>
            {
                _device = null;
                Application.Current.Shutdown();
            });
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

        }


        public ICommand CloseCommand { get; private set; }

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
    }
}
