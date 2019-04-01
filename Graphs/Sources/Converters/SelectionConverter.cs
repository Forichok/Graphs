using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Graphs.Sources.Models;

namespace Graphs.Sources.Converters
{
    class SelectionConverter:IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)values[0])
                return new SolidColorBrush(Colors.Blue);
            try
            {
                SolidColorBrush color = values[4] as SolidColorBrush;
                if (color != null)
                {
                    return color;

                }
                if (parameter.ToString() == "Node")
                {
                    if((bool)values[1])
                        return new SolidColorBrush(Colors.Orange);

                    if((bool)values[2])
                        return new SolidColorBrush(Colors.Green);

                    if ((bool)values[3])
                        return new SolidColorBrush(Colors.Red);
                }

                if (parameter.ToString() == "Link")
                {
                    if ((bool)values[1])
                        return new SolidColorBrush(Colors.Red);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return new SolidColorBrush(Colors.Black);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
