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
