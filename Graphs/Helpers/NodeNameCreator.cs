using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphs.Helpers
{
    static class NodeNameCreator
    {
        private static readonly char[] Alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static int alphaPosition;
        private static readonly char[] Nums = "1234567890".ToCharArray();
        

        public static String GetNodeName()
        {
            var a = alphaPosition / (Alpha.Length);
            if (a>=1)
            {
                return Alpha[alphaPosition++ - (Alpha.Length) * a].ToString() + Nums[a-1];
            }
            return Alpha[alphaPosition++].ToString();
        }

    }
}
