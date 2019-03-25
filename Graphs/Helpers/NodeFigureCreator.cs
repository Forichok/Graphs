using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphs.Helpers
{
    static class NodeFigureCreator
    {
        private static List<String> Figures = new List<string>(){"Circle","Cube","Triangle"};
        private static int pos = 0;

        public static String GetFigure()
        {
            if (pos >= Figures.Count)
                pos = 0;
            return Figures[pos++];
        }

    }
}
