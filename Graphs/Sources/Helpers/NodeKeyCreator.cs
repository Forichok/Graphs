using System;
using System.Collections.Generic;
using System.Linq;

namespace Graphs.Sources.Helpers
{
    static class NodeKeyCreator
    {
        private static readonly char[] Alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        public static string GetNodeName(IEnumerable<string> nodes,IEnumerable<string> nodeKeys)
        {
            var current = "";
            var _alphaPosition = 0;

            while (nodes.Contains(current) || string.IsNullOrEmpty(current))
            {
                var a = _alphaPosition / (Alpha.Length);
                if (a >= 1)
                {
                    current = Alpha[_alphaPosition++ - (Alpha.Length) * a].ToString() + (a - 1);
                    if(!nodeKeys.Contains(current))
                        break;
                }
                else
                {
                    current = Alpha[_alphaPosition++].ToString();
                    if (!nodeKeys.Contains(current))
                        break;
                }
            }
            return current;
        }
    }
}
