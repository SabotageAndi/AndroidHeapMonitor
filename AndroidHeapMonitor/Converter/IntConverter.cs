using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AndroidHeapMonitor.Converter
{
    class IntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            int intValue = (int) value;

            return intValue.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            string strValue = (String) value;

            int intValue;

            if (Int32.TryParse(strValue, out intValue))
                return intValue;

            return null;
        }
    }
}
