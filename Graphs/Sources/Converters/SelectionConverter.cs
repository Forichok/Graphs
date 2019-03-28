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
            try
            {
                if (parameter.ToString() == "Node")
                {
                    var node = values[1] as NodeModel;

                    if (node.IsStartNode)
                        return new SolidColorBrush(Colors.Orange);

                    if (node.IsFinishNode)
                        return new SolidColorBrush(Colors.Green);

                    if (node.IsSelected)
                        return new SolidColorBrush(Colors.Red);
                }

                if (parameter.ToString() == "Link")
                {
                    var link = values[1] as LinkModel;

                    if (link.IsSelected)
                        return new SolidColorBrush(Colors.Red);
                }

                if ((bool)values[0])
                    return new SolidColorBrush(Colors.Blue);
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
