using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AndroidHeapMonitor.Logic;
using GalaSoft.MvvmLight.Command;
using Managed.Adb;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

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
        private DumpsysMemInfoParser _dumpsysMemInfoParser;
        private ObservableCollection<DataItemViewModel> _items;
        private LineSeries _series;

        public MainViewModel(AndroidDebugBridge bridge)
        {
            _bridge = bridge;
            CloseCommand = new RelayCommand(OnClose);
            StartCommand = new RelayCommand(OnStart);
            StopCommand = new RelayCommand(OnStop);
            Items = new ObservableCollection<DataItemViewModel>();
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
            _refreshTimer.AutoReset = false;
            _refreshTimer.Elapsed += _refreshTimer_Elapsed;
            _dumpsysMeminfo = new DumpsysMeminfo(_device);
            _dumpsysMemInfoParser = new DumpsysMemInfoParser();

            InitPlotModel();


            PackageName = "at.oebb.ikt.greenpoints";
        }

        private void InitPlotModel()
        {
            PlotModel = new PlotModel();
            PlotModel.Axes.Add(new LinearAxis(AxisPosition.Bottom));
            PlotModel.Axes.Add(new LinearAxis(AxisPosition.Left, minimum: 0, maximum: 100000));

            _series = new LineSeries("Heap Size") {};
            PlotModel.Series.Add(_series);
        }

        public ObservableCollection<DataItemViewModel> Items
        {
            get { return _items; }
            set
            {
                if (Equals(value, _items)) return;
                _items = value;
                OnPropertyChanged();
            }
        }

        void _refreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var output = _dumpsysMeminfo.GetInfo(PackageName);

            var dumpsysMemInfo = _dumpsysMemInfoParser.Parse(output);

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Items.Add(new DataItemViewModel()
                {
                    Timestamp = DateTime.Now,
                    MemInfo = dumpsysMemInfo
                });

                _series.Points.Add(new DataPoint(Items.Count - 1, dumpsysMemInfo.NativeHeap.HeapSize));
              
                PlotModel.InvalidatePlot(false);

                _refreshTimer.Start();
            }));
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

        public PlotModel PlotModel { get; set; }
    }

    public class DataItemViewModel : ViewModel
    {
        public DateTime Timestamp { get; set; }

        public DumpsysMemInfo MemInfo { get; set; }
    }
}
