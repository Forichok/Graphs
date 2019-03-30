using System.Collections.Generic;
using System.Linq;
using Graphs.Sources.Models;
using Utilities.DataTypes;

namespace Graphs.Sources.Tasks
{
    public static class BestFSTask3
    {
        public static KeyValuePair<IEnumerable<LinkModel>, string> StartBestFs(IEnumerable<MappedNode> mapedEnumerable, string keyFrom, string keyTo)
        {
            var result = new List<LinkModel>();

            var queue = new PriorityQueue<string> {{0, keyFrom}};

            var dataDict = new Dictionary<string, UniversalGraphNodeData>();
            var mapedList = mapedEnumerable.ToList();

            foreach (var mappedNode in mapedList)
            {
                dataDict.Add(mappedNode.Node.Key, new UniversalGraphNodeData { Node = mappedNode });
            }

            dataDict[keyFrom].IsVisited = true;

            MappedNode nextMaped = null;
            var isOk = false;
            while (queue.Count != 0)
            {            // пока очередь не пуста

                var node = queue.Pop();                 // извлечь первый элемент в очереди

                nextMaped = dataDict[node].Node;

                if (node == keyTo)
                {
                    isOk = true;
                    break;                      // проверить, не является ли текущий узел целевым
                }


                foreach (var link in nextMaped.Links)
                {    // все преемники текущего узла, ...
                    var to = link.GetTo(node);
                    if (dataDict[to].IsVisited == false)
                    {      // ... которые ещё не были посещены ...
                        var cost = int.Parse(link.Text) * -1;
                        queue.Add(cost,to);                // ... добавить в конец очереди...
                                                                // ... и пометить как посещённые
                        dataDict[to].ParentMappedNode = nextMaped;
                        dataDict[to].ParentLink = link;
                    }
                }
                dataDict[node].IsVisited = true;

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
    }
}
