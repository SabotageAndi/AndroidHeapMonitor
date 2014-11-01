using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AndroidHeapMonitor.Validator
{
    class MinIntValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string strValue = (string)value;

            int intValue;

            if (!Int32.TryParse(strValue, out intValue))
                return new ValidationResult(false, "Not parsable");

            if (intValue < MinValue)
                return new ValidationResult(false, "Value to low");

            return ValidationResult.ValidResult;
        }

        public int MinValue { get; set; }
    }
}
