using System;
using System.Globalization;
using System.Windows.Data;
using Graphs.Sources.Models;
using Northwoods.GoXam;

namespace Graphs.Sources.Converters
{
    class LinkDirectionConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((bool)value)
            {
                return "Standard";
            }

            return "None";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
