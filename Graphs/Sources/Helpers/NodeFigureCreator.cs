using System.Collections.Generic;

namespace Graphs.Sources.Helpers
{
    static class NodeFigureCreator
    {
        private static List<string> Figures = new List<string>(){ "RoundedRectangle","Circle","Cube","Triangle"};
        private static int pos = 0;

        public static string GetFigure()
        {
            if (pos >= Figures.Count)
                pos = 0;
            return Figures[pos++];
        }

        public static string GetFigure(ref int id)
        {
            if (id >= Figures.Count)
                id = 0;
            return Figures[id++];
            
        }

    }
}
