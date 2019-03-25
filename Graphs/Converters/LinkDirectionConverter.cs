using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Graphs.Models;
using Northwoods.GoXam;

namespace Graphs
{
    class LinkDirectionConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = (value as PartManager.PartBinding).Data as LinkModel;
            if(b.IsOriented)
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
