using System.Collections.Generic;
using System.Linq;
using Graphs.Sources.Models;
using Utilities.DataTypes;

namespace Graphs.Sources.Tasks
{
    public static class DijkstraTask4
    {
        public static IDictionary<string, UniversalGraphNodeData> StartDijkstra(IEnumerable<MappedNode> mapedEnumerable, string keyFrom)
        {

            var queue = new PriorityQueue<string> { { 0, keyFrom } };

            var dataDict = new Dictionary<string, UniversalGraphNodeData>();
            var mapedList = mapedEnumerable.ToList();

            foreach (var mappedNode in mapedList)
            {
                dataDict.Add(mappedNode.Node.Key, new UniversalGraphNodeData { Node = mappedNode });
            }

            dataDict[keyFrom].IsVisited = true;
            dataDict[keyFrom].Cost = 0;

            while (queue.Count != 0)
            {            // пока очередь не пуста

                var nodeName = queue.Pop();                 // извлечь первый элемент в очереди
                var nextMaped = dataDict[nodeName].Node;

                foreach (var link in nextMaped.Links)
                {    // все преемники текущего узла, ...
                    var to = link.GetTo(nodeName);

                    if (dataDict[to].IsVisited == false)
                    { 
                        var toNodeCost = int.Parse(link.Text);
                        var selfCost = dataDict[nodeName].Cost;
                        var newTotalCost = toNodeCost + selfCost;

                        if (dataDict[to].Cost == -1)
                        {
                            queue.Add(newTotalCost * -1, to);
                            dataDict[to].Cost = newTotalCost;
                        }
                        else if (dataDict[to].Cost > newTotalCost)
                        {
                            queue.Remove(dataDict[to].Cost * -1);
                            dataDict[to].Cost = newTotalCost;
                            queue.Add(newTotalCost * -1, to);
                        }
                        else
                        {
                            continue;
                        }                        

                        dataDict[to].ParentMappedNode = nextMaped;
                        dataDict[to].ParentLink = link;
                    }
                }
                dataDict[nodeName].IsVisited = true;
            }

            return dataDict;
        }
    }
}
