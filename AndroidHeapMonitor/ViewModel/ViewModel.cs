using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AndroidHeapMonitor.Annotations;
using GalaSoft.MvvmLight;

namespace AndroidHeapMonitor.ViewModel
{
    public class ViewModel : ViewModelBase
    {
        
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            RaisePropertyChanged(propertyName);
        }
    }
}
