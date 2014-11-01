using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AndroidHeapMonitor.Validator
{
    class MaxIntValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string strValue = (string) value;

            int intValue;

            if (!Int32.TryParse(strValue, out intValue))
                return new ValidationResult(false, "Not parsable");

            if (intValue > MaxValue)
                return new ValidationResult(false, "Value to high");

            return ValidationResult.ValidResult;
        }

        public int MaxValue { get; set; }
    }
}
