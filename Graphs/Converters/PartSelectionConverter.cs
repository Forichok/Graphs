using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Graphs.Converters
{
    class PartSelectionConverter:IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if((bool)values[1])
                return new SolidColorBrush(Colors.Red);
            if ((bool)values[0])
                return new SolidColorBrush(Colors.Blue);

            return new SolidColorBrush(Colors.Black);

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
