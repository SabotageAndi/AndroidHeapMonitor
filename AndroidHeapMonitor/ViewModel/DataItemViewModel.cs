using System;
using AndroidHeapMonitor.Logic;

namespace AndroidHeapMonitor.ViewModel
{
    public class DataItemViewModel : ViewModel
    {
        public DateTime Timestamp { get; set; }

        public DumpsysMemInfo MemInfo { get; set; }
    }
}