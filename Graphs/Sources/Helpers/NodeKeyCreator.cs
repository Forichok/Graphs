using System.Collections.Generic;
using System.Linq;

namespace Graphs.Sources.Helpers
{
    static class NodeKeyCreator
    {
        private static readonly char[] Alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        public static int _alphaPosition;
        

        public static string GetNodeName(IEnumerable<string> nodes)
        {
            var curent = "";

            while (nodes.Contains(curent) || string.IsNullOrEmpty(curent))
            {
                var a = _alphaPosition / (Alpha.Length);
                if (a >= 1)
                {
                    curent = Alpha[_alphaPosition++ - (Alpha.Length) * a].ToString() + (a - 1);
                }
                else
                {
                    curent = Alpha[_alphaPosition++].ToString();
                }
            }

            return curent;
        }

    }
}
