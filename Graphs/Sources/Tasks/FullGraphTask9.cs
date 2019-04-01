using System.Collections.Generic;
using Graphs.Sources.Models;

namespace Graphs.Sources.Tasks
{
    public static class FullGraphTask9
    {
        public static bool Check(IEnumerable<MappedNode> mapedEnumerable)
        {
            foreach (var mappedNode in mapedEnumerable)
            {
                foreach (var otherNode in mapedEnumerable)
                {
                    if (mappedNode == otherNode)
                    {
                        continue;
                    }

                    var existAllFrom = mappedNode.Links.Exists((t) =>
                    {
                        if (!t.IsOriented)
                        {
                            var to = t.GetTo(mappedNode.Node.Key);
                            if (to != otherNode.Node.Key)
                                return false;
                        }
                        else
                        {
                            if (t.To != otherNode.Node.Key)
                                return false;
                        }

                        return true;
                    });

                    var existAllIn = mappedNode.LinksIn.Exists((t) =>
                    {
                        if (!t.IsOriented)
                        {
                            var to = t.GetTo(otherNode.Node.Key);
                            if (to != mappedNode.Node.Key)
                                return false;
                        }
                        else
                        {
                            if (t.To != mappedNode.Node.Key)
                                return false;
                        }
                        return true;
                    });

                    if (!existAllIn || !existAllFrom)
                        return false;
                }
            }

            return true;
        }

        public static List<LinkModel> GetFull(IEnumerable<MappedNode> mapedEnumerable)
        {
            var result = new List<LinkModel>();

            return result;
        }
    }
}
