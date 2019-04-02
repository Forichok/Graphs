using System;
using System.Globalization;
using System.Windows.Controls;

namespace Graphs.Sources.Helpers
{
    class NumberValidation : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int i;
            if (!Int32.TryParse(value.ToString(), out i)&&i<0)
            {
                return new ValidationResult(false, "Incorrect");
            }

            return new ValidationResult(true, null);
        }
    }
}
