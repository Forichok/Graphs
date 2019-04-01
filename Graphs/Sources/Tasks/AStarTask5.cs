using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Graphs.Sources.Models;
using Utilities.DataTypes;

namespace Graphs.Sources.Tasks
{
    public class AStarTask5
    {

        //NOT WORKING
        public static KeyValuePair<IEnumerable<LinkModel>, string> StartAStar(IEnumerable<MappedNode> mapedEnumerable, string keyFrom, string keyTo)
        {
            var result = new List<LinkModel>();

            var queue = new PriorityQueue<string> { { 0, keyFrom } };

            var dataDict = new Dictionary<string, UniversalGraphNodeData>();
            var mapedList = mapedEnumerable.ToList();

            foreach (var mappedNode in mapedList)
            {
                dataDict.Add(mappedNode.Node.Key, new UniversalGraphNodeData { Node = mappedNode });
            }
            var toMapped = dataDict[keyTo].Node;
            dataDict[keyFrom].IsVisited = true;

            MappedNode nextMaped = null;
            var isOk = false;
            while (queue.Count != 0)
            {            // пока очередь не пуста
                var nodeName = queue.Pop();                 // извлечь первый элемент в очереди
                nextMaped = dataDict[nodeName].Node;

                if (nodeName == keyTo)
                {
                    isOk = true;
                    break;                      // проверить, не является ли текущий узел целевым
                }

                foreach (var link in nextMaped.Links)
                {    // все преемники текущего узла, ...
                    var to = link.GetTo(nodeName);

                    if (dataDict[to].IsVisited == false)
                    {
                        var toNodeCost = int.Parse(link.Text);
                        var selfCost = dataDict[nodeName].Cost;
                        var newTotalCost = toNodeCost + selfCost + GetEurastick(dataDict[nodeName].Node, toMapped);

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

            if (isOk && nextMaped != null)
            {
                //result.Add(dataDict[nextMaped.Node.Key].ParentLink);
                while (dataDict[nextMaped.Node.Key].ParentLink != null)
                {
                    result.Add(dataDict[nextMaped.Node.Key].ParentLink);//и дописываем к пути
                    //пока существует предыдущая вершина
                    nextMaped = dataDict[nextMaped.Node.Key].ParentMappedNode; //переходим в неё

                }
            }

            return new KeyValuePair<IEnumerable<LinkModel>, string>(result, UniversalGraphNodeData.GetVector(dataDict, keyTo));

        }

        private static int GetEurastick(MappedNode from, MappedNode to)
        {
            return (int) Math.Sqrt(Math.Pow(from.Node.Location.X - to.Node.Location.X, 2) +
                                   Math.Pow(from.Node.Location.Y - to.Node.Location.Y, 2));
        }

    }
}
