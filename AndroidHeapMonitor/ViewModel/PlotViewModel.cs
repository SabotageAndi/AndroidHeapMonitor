using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AndroidHeapMonitor.Logic;
using Managed.Adb;
using OxyPlot;
using OxyPlot.Axes;

namespace AndroidHeapMonitor.ViewModel
{
    public class PlotViewModel : ViewModel
    {
        public PlotModel PlotModel { get; set; }

        private LinearAxis _timeAxis;
        private LinearAxis _valueAxis;
        private int _currentX;
        private readonly DumpsysMemInfoParser _dumpsysMemInfoParser;


        public ObservableCollection<SeriesViewModel> AvailableValues { get; set; }
        public ObservableCollection<DataItemViewModel> Items { get; set; }

        public PlotViewModel()
        {
            Items = new ObservableCollection<DataItemViewModel>();
            AvailableValues = new ObservableCollection<SeriesViewModel>();
            _dumpsysMemInfoParser = new DumpsysMemInfoParser();
        }

        public void InitPlotModel()
        {
            PlotModel = new PlotModel();
            _timeAxis = new LinearAxis(AxisPosition.Bottom, minimum: 0, maximum: 120);
            _valueAxis = new LinearAxis(AxisPosition.Left);
            PlotModel.Axes.Add(_timeAxis);
            PlotModel.Axes.Add(_valueAxis);

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


        public void Update()
        {
            var output = Dumpsys.GetMeminfoOfPackage(SelectedDevice, SelectedPackage.Name);

            var dumpsysMemInfo = _dumpsysMemInfoParser.ParseMeminfo(output);

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
                        _valueAxis.Maximum = currentY * 1.2;
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

            }));
        }

        public Device SelectedDevice { get; set; }

        public int Interval { get; set; }

        public DumpsysPackages SelectedPackage { get; set; }
    }
}
