using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Managed.Adb;

namespace AndroidHeapMonitor.Logic
{
    class DumpsysMeminfo
    {
        private readonly Device _device;

        public DumpsysMeminfo(Device device)
        {
            _device = device;
        }

        public string GetInfo(string packageName)
        {
            var commandResultReceiver = new CommandResultReceiver();
            _device.ExecuteShellCommand(string.Format("dumpsys meminfo {0}", packageName), commandResultReceiver );

            return commandResultReceiver.Result;
        }
    }
}
