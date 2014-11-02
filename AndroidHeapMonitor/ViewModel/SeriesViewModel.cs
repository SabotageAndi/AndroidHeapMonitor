using System;
using AndroidHeapMonitor.Logic;
using OxyPlot.Series;

namespace AndroidHeapMonitor.ViewModel
{
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
}