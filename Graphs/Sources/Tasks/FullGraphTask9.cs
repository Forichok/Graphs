using System;
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

                    var existFrom = mappedNode.Links.Exists((t) =>
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

                    var existIn = mappedNode.LinksIn.Exists((t) =>
                    {
                        if (!t.IsOriented)
                        {
                            var from = t.GetFrom(mappedNode.Node.Key);
                            if (from != otherNode.Node.Key)
                                return false;
                        }
                        else
                        {
                            if (t.From != otherNode.Node.Key)
                                return false;
                        }
                        return true;
                    });

                    if (!existIn || !existFrom)
                        return false;
                }
            }

            return true;
        }

        public static List<LinkModel> GetFull(IEnumerable<MappedNode> mapedEnumerable)
        {
            var result = new List<LinkModel>();

            foreach (var mappedNode in mapedEnumerable)
            {
                foreach (var otherNode in mapedEnumerable)
                {
                    if (mappedNode == otherNode)
                    {
                        continue;
                    }
                    
                    var existFrom = mappedNode.Links.Exists((t) =>
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

                    var existIn = mappedNode.LinksIn.Exists((t) =>
                    {
                        if (!t.IsOriented)
                        {
                            var from = t.GetFrom(mappedNode.Node.Key);
                            if (from != otherNode.Node.Key)
                                return false;
                        }
                        else
                        {
                            if (t.From != otherNode.Node.Key)
                                return false;
                        }
                        return true;
                    });

                    if (!existIn && !existFrom)
                    {
                        var link = new LinkModel(mappedNode.Node.Key, otherNode.Node.Key,
                            ((int)(mappedNode.Node.Location - otherNode.Node.Location).Length / 100).ToString())
                        {
                            IsOriented = false
                        };
                        result.Add(link);
                        mappedNode.Links.Add(link);
                        mappedNode.LinksIn.Add(link);
                        otherNode.Links.Add(link);
                        otherNode.LinksIn.Add(link);

                    }
                    else if (!existIn)
                    {
                        var link = new LinkModel(otherNode.Node.Key, mappedNode.Node.Key,
                            ((int) (mappedNode.Node.Location - otherNode.Node.Location).Length / 100).ToString());
                        result.Add(link);
                    
                        mappedNode.LinksIn.Add(link);
                        otherNode.Links.Add(link);
                    }
                    else if (!existFrom)
                    {
                        var link = new LinkModel(mappedNode.Node.Key, otherNode.Node.Key,
                            ((int) (mappedNode.Node.Location - otherNode.Node.Location).Length / 100).ToString());
                        result.Add(link);

                        mappedNode.Links.Add(link);
                        otherNode.LinksIn.Add(link);
                    }
                }
            }


            return result;
        }
    }
}
