using System;

namespace Graphs.Helpers
{
    static class NodeNameCreator
    {
        private static readonly char[] Alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static int _alphaPosition;
        private static readonly char[] Nums = "1234567890".ToCharArray();
        

        public static string GetNodeName()
        {
            var a = _alphaPosition / (Alpha.Length);
            if (a>=1)
            {
                return Alpha[_alphaPosition++ - (Alpha.Length) * a].ToString() + Nums[a-1];
            }
            return Alpha[_alphaPosition++].ToString();
        }

    }
}
