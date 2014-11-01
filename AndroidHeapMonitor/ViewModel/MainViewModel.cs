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
        private LineSeries _series;
        private LinearAxis _timeAxis;
        private LinearAxis _valueAxis;
        private int _interval;
        private int _currentX;

        public MainViewModel(AndroidDebugBridge bridge)
        {
            _bridge = bridge;
            CloseCommand = new RelayCommand(OnClose);
            StartCommand = new RelayCommand(OnStart);
            StopCommand = new RelayCommand(OnStop);
            Items = new ObservableCollection<DataItemViewModel>();
            AvailableValues = new ObservableCollection<SeriesViewModel>();
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


            Interval = 2500;
            _refreshTimer = new Timer(Interval);
            _refreshTimer.AutoReset = false;
            _refreshTimer.Elapsed += _refreshTimer_Elapsed;
            _dumpsysMeminfo = new DumpsysMeminfo(_device);
            _dumpsysMemInfoParser = new DumpsysMemInfoParser();

            InitPlotModel();
         
            AddMeminfoHeapColumns("Native Heap", info => info.NativeHeap, true);
            AddMeminfoHeapColumns("Total", info => info.Total);
            AddMeminfoHeapColumns("Dalvik Heap", info => info.DalvikHeap);
            AddMemInfoClumns("Dalvik Other", info => info.DalvikOther);
            AddMemInfoClumns("Stack", info => info.Stack);
            AddMemInfoClumns("Other dev", info => info.OtherDev);
            AddMemInfoClumns(".so mmap", info => info.SoMMAP);
            AddMemInfoClumns(".apk mmap", info => info.ApkMMAP);
            AddMemInfoClumns(".ttf mmap", info => info.TtfMMAP);
            AddMemInfoClumns(".dex mmap", info => info.DexMMAP);
            AddMemInfoClumns("Graphics", info => info.Graphics);
            AddMemInfoClumns("GL", info => info.GL);
            AddMemInfoClumns("Unknown", info => info.Unknown);

            foreach (var seriesViewModel in AvailableValues)
            {
                seriesViewModel.CheckedChanged += SeriesViewModelOnCheckedChanged;
            }

            PackageName = "at.oebb.ikt.greenpoints";
        }


        private void AddMeminfoHeapColumns(string name, Func<DumpsysMemInfo, MeminfoHeap> getMeminfo, bool areHeapColumnsChecked = false)
        {
            AddSeries(new SeriesViewModel(name + " Size", info => getMeminfo(info).HeapSize, areHeapColumnsChecked));
            AddSeries(new SeriesViewModel(name + " Free", info => getMeminfo(info).HeapFree, areHeapColumnsChecked));
            AddSeries(new SeriesViewModel(name + " Alloc", info => getMeminfo(info).HeapAlloc, areHeapColumnsChecked));

            AddMemInfoClumns(name, getMeminfo);
        }

        private void AddMemInfoClumns(string name, Func<DumpsysMemInfo, Meminfo> getMemInfo)
        {
            AddSeries(new SeriesViewModel(name + " PSS Total", info => getMemInfo(info).PssTotal));
            AddSeries(new SeriesViewModel(name + " Private Dirty", info => getMemInfo(info).PrivateDirty));
            AddSeries(new SeriesViewModel(name + " Swapped Dirty", info => getMemInfo(info).SwappedDirty));
            AddSeries(new SeriesViewModel(name + " Private Clean", info => getMemInfo(info).PrivateClean));
        }

        private void AddSeries(SeriesViewModel seriesViewModel)
        {
            AvailableValues.Add(seriesViewModel);
            if (seriesViewModel.IsChecked)
            {
                PlotModel.Series.Add(seriesViewModel.Series);
            }
        }

        private void SeriesViewModelOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            var seriesViewModel = (SeriesViewModel)sender;

            if (seriesViewModel.IsChecked)
            {
                PlotModel.Series.Add(seriesViewModel.Series);
            }
            else
            {
                PlotModel.Series.Remove(seriesViewModel.Series);
            }

            PlotModel.InvalidatePlot(true);
        }

        private void InitPlotModel()
        {
            PlotModel = new PlotModel();
            _timeAxis = new LinearAxis(AxisPosition.Bottom, minimum:0, maximum:120);
            _valueAxis = new LinearAxis(AxisPosition.Left);
            PlotModel.Axes.Add(_timeAxis);
            PlotModel.Axes.Add(_valueAxis);

        }

       
        void _refreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var output = _dumpsysMeminfo.GetInfo(PackageName);

            var dumpsysMemInfo = _dumpsysMemInfoParser.Parse(output);

            var dataItemViewModel = new DataItemViewModel()
            {
                Timestamp = DateTime.Now,
                MemInfo = dumpsysMemInfo
            };


            foreach (var seriesViewModel in AvailableValues)
            {
                if (seriesViewModel.IsChecked)
                {
                    double currentY = seriesViewModel.GetValue(dumpsysMemInfo);

                    if (Double.IsNaN(_valueAxis.Maximum))
                    {
                        _valueAxis.Maximum = currentY*1.2;
                    }

                    if (_valueAxis.Maximum <= currentY)
                    {
                        _valueAxis.Maximum *= 1.2;
                    }

                    seriesViewModel.Series.Points.Add(new DataPoint(_currentX, currentY));
                }
            }


            _currentX = _currentX + (Interval / 1000);
            if (_timeAxis.Maximum <= _currentX)
            {
                _timeAxis.Maximum += _timeAxis.Maximum;
            }

 
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Items.Add(dataItemViewModel);
                
                PlotModel.InvalidatePlot(true);

                _refreshTimer.Start();
            }));
        }



        public ICommand CloseCommand { get; private set; }
        public ICommand StartCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public PlotModel PlotModel { get; set; }
        public ObservableCollection<SeriesViewModel> AvailableValues { get; set; }
        public ObservableCollection<DataItemViewModel> Items { get; set; }

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


        public int Interval
        {
            get { return _interval; }
            set
            {
                if (value == _interval) return;
                _interval = value;
                OnPropertyChanged();
            }
        }
    }

    public class SeriesViewModel : ViewModel
    {
        private string _name;
        private readonly Func<DumpsysMemInfo, int> _getValue;
        private bool _isChecked;
        private readonly LineSeries _series;

        public event EventHandler CheckedChanged;

        protected virtual void OnCheckedChanged()
        {
            var handler = CheckedChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public SeriesViewModel(string name, Func<DumpsysMemInfo, int> getValue, bool isChecked = false)
        {
            _name = name;
            IsChecked = isChecked;
            _getValue = getValue;
            _series = new LineSeries(_name);
        }

        public int GetValue(DumpsysMemInfo dumpsysMemInfo)
        {
            return _getValue(dumpsysMemInfo);
        }

        public LineSeries Series
        {
            get { return _series; }
        }

        public string Name
        {
            get { return _name; }
            private set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (value.Equals(_isChecked)) return;
                _isChecked = value;
                OnPropertyChanged();
                OnCheckedChanged();
            }
        }
    }

    public class DataItemViewModel : ViewModel
    {
        public DateTime Timestamp { get; set; }

        public DumpsysMemInfo MemInfo { get; set; }
    }
}
