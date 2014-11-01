using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidHeapMonitor.Logic
{
    public class DumpsysMemInfo
    {
        public MeminfoHeap NativeHeap { get; set; }
        public MeminfoHeap DalvikHeap { get; set; }
        public MeminfoHeap Total { get; set; }
        public Meminfo DalvikOther { get; set; }
        public Meminfo Stack { get; set; }
        public Meminfo OtherDev { get; set; }
        public Meminfo SoMMAP { get; set; }
        public Meminfo ApkMMAP { get; set; }
        public Meminfo TtfMMAP { get; set; }
        public Meminfo DexMMAP { get; set; }
        public Meminfo OtherMMAP { get; set; }
        public Meminfo Graphics { get; set; }
        public Meminfo GL { get; set; }
        public Meminfo Unknown { get; set; }
    }

    public class Meminfo
    {
        public int PssTotal { get; set; }
        public int PrivateDirty { get; set; }
        public int PrivateClean { get; set; }
        public int SwappedDirty { get; set; }
    }

    public class MeminfoHeap : Meminfo
    {
        public int HeapSize { get; set; }
        public int HeapAlloc { get; set; }
        public int HeapFree { get; set; }
    }

    class DumpsysMemInfoParser
    {
        public DumpsysMemInfo Parse(string output)
        {
            var dumpsysMemInfo = new DumpsysMemInfo();
            var lines = output.Split(new string[]{Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);


            foreach (var line in lines)
            {
                var resultitem = ParseLines(line);

                if (resultitem == null)
                    continue;

                switch (resultitem.Item1)
                {
                    case "Native Heap":
                        dumpsysMemInfo.NativeHeap = (MeminfoHeap)resultitem.Item2;
                        break;
                    case "TOTAL":
                        dumpsysMemInfo.Total = (MeminfoHeap)resultitem.Item2;
                        break;
                    case "Dalvik Heap":
                        dumpsysMemInfo.DalvikHeap = (MeminfoHeap)resultitem.Item2;
                        break;
                    case "Dalvik Other":
                        dumpsysMemInfo.DalvikOther = resultitem.Item2;
                        break;
                    case "Stack":
                        dumpsysMemInfo.Stack = resultitem.Item2;
                        break;
                    case "Other dev":
                        dumpsysMemInfo.OtherDev = resultitem.Item2;
                        break;
                    case ".so mmap":
                        dumpsysMemInfo.SoMMAP = resultitem.Item2;
                        break;
                    case ".apk mmap":
                        dumpsysMemInfo.ApkMMAP = resultitem.Item2;
                        break;
                    case ".ttf mmap":
                        dumpsysMemInfo.TtfMMAP = resultitem.Item2;
                        break;
                    case ".dex mmap":
                        dumpsysMemInfo.DexMMAP = resultitem.Item2;
                        break;
                    case "Other mmap":
                        dumpsysMemInfo.OtherMMAP = resultitem.Item2;
                        break;
                    case "Graphics":
                        dumpsysMemInfo.Graphics = resultitem.Item2;
                        break;
                    case "GL":
                        dumpsysMemInfo.GL = resultitem.Item2;
                        break;
                    case "Unknown":
                        dumpsysMemInfo.Unknown = resultitem.Item2;
                        break;
                }
            }



            return dumpsysMemInfo;
        }

        private Tuple<string, Meminfo> ParseLines(string line)
        {
            string[] columnArray = line.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);

            if (columnArray.Length == 0)
                return null;

            int startColumn = 1;
            string meminfoName = columnArray[0];


            if (columnArray.Length >= 2)
            {
                int column2;
                if (!int.TryParse(columnArray[1], out column2))
                {
                    startColumn = 2;
                    meminfoName += " " + columnArray[1];
                }
            }

            var dataColumns = columnArray.Skip(startColumn).ToArray();

            Meminfo meminfo;

            switch (meminfoName)
            {
                case "Native Heap":
                case "TOTAL":
                case "Dalvik Heap":
                    meminfo = ParseMeminfoHeap(dataColumns);
                    break;
                case "Dalvik Other":
                case "Stack":
                case "Other dev":
                case ".so mmap":
                case ".apk mmap":
                case ".ttf mmap":
                case ".dex mmap":
                case "Other mmap":
                case "Graphics":
                case "GL":
                case "Unknown":
                    meminfo = ParseMeminfo(dataColumns);
                    break;
                default:
                    return null;
                    break;
            }


            return new Tuple<string, Meminfo>(meminfoName, meminfo);
        }

        private Meminfo ParseMeminfo(string[] columnArray)
        {
            var result = new Meminfo();

            FillMeminfo(result, columnArray);

            return result;
        }

        private void FillMeminfo(Meminfo result, string[] columnArray)
        {
            result.PssTotal = ParseInt(columnArray, 0);
            result.PrivateDirty = ParseInt(columnArray, 1);
            result.PrivateClean = ParseInt(columnArray, 2);
            result.SwappedDirty = ParseInt(columnArray, 3);
        }

        private Meminfo ParseMeminfoHeap(string[] columnArray)
        {
            var meminfoHeap = new MeminfoHeap();
            FillMeminfo(meminfoHeap, columnArray);

            meminfoHeap.HeapSize = ParseInt(columnArray, 4);
            meminfoHeap.HeapAlloc = ParseInt(columnArray, 5);
            meminfoHeap.HeapFree = ParseInt(columnArray, 6);

            return meminfoHeap;
        }

        private int ParseInt(string[] columnArray, int index)
        {
            int pssTotal;
            Int32.TryParse(columnArray[index], out pssTotal);
            return pssTotal;
        }
    }
}
