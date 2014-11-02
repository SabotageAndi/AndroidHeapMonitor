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
using System.Windows;
using AndroidMemoryMonitor.Logic;
using Managed.Adb;
using OxyPlot;
using OxyPlot.Axes;

namespace AndroidMemoryMonitor.ViewModel
{
    public class PlotViewModel : ViewModel
    {
        private readonly DumpsysMemInfoParser _dumpsysMemInfoParser;
        private int _currentX;
        private LinearAxis _timeAxis;
        private LinearAxis _valueAxis;

        public PlotViewModel()
        {
            Items = new ObservableCollection<DataItemViewModel>();
            AvailableValues = new ObservableCollection<SeriesViewModel>();
            _dumpsysMemInfoParser = new DumpsysMemInfoParser();
        }

        public PlotModel PlotModel { get; set; }


        public ObservableCollection<SeriesViewModel> AvailableValues { get; set; }
        public ObservableCollection<DataItemViewModel> Items { get; set; }
        public Device SelectedDevice { get; set; }

        public int Interval { get; set; }

        public DumpsysPackages SelectedPackage { get; set; }

        public void InitPlotModel()
        {
            PlotModel = new PlotModel();
            _timeAxis = new LinearAxis(AxisPosition.Bottom, 0, 120);
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

            foreach (SeriesViewModel seriesViewModel in AvailableValues)
            {
                seriesViewModel.CheckedChanged += SeriesViewModelOnCheckedChanged;
            }
        }

        private void AddMeminfoHeapColumns(string name, Func<DumpsysMemInfo, MeminfoHeap> getMeminfo,
            bool areHeapColumnsChecked = false)
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
            var seriesViewModel = (SeriesViewModel) sender;

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
            string output = Dumpsys.GetMeminfoOfPackage(SelectedDevice, SelectedPackage.Name);

            DumpsysMemInfo dumpsysMemInfo = _dumpsysMemInfoParser.ParseMeminfo(output);

            var dataItemViewModel = new DataItemViewModel
            {
                Timestamp = DateTime.Now,
                MemInfo = dumpsysMemInfo
            };

            foreach (SeriesViewModel seriesViewModel in AvailableValues)
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


            _currentX = _currentX + (Interval/1000);
            if (_timeAxis.Maximum <= _currentX)
            {
                _timeAxis.Maximum += _timeAxis.Maximum;
            }


            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Items.Add(dataItemViewModel);

                PlotModel.InvalidatePlot(true);
            }));
        }
    }
}