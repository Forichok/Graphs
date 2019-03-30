using System.Collections.Generic;
using System.Linq;
using Graphs.Sources.Models;

namespace Graphs.Sources.Tasks
{
    public static class BFSTask2
    {
        public static KeyValuePair<List<LinkModel>, string> BreadthFirstSearch(IEnumerable<MappedNode> mapedEnumerable, string keyFrom, string keyTo)
        {
            var result = new List<LinkModel>();

            var queue = new Queue<string>();
            queue.Enqueue(keyFrom);

            var dataDict = new Dictionary<string, UniversalGraphNodeData>();
            var mapedList = mapedEnumerable.ToList();

            foreach (var mappedNode in mapedList)
            {
                dataDict.Add(mappedNode.Node.Key, new UniversalGraphNodeData{Node = mappedNode});
            }

            dataDict[keyFrom].IsVisited = true;

            MappedNode nextMaped = null;
            var isOk = false;
            while (queue.Count != 0)
            {            // пока очередь не пуста

                var node = queue.Dequeue();                 // извлечь первый элемент в очереди

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
                        queue.Enqueue(to);                // ... добавить в конец очереди...
                        dataDict[to].IsVisited = true;            // ... и пометить как посещённые
                        dataDict[to].ParentMappedNode = nextMaped;
                        dataDict[to].ParentLink = link;
                    }
                }
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

            return new KeyValuePair<List<LinkModel>, string>(result, UniversalGraphNodeData.GetVector(dataDict, keyTo));
        }

    }

    
}
