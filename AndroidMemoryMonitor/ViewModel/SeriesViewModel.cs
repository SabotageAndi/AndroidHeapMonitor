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
using AndroidMemoryMonitor.Logic;
using OxyPlot.Series;

namespace AndroidMemoryMonitor.ViewModel
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