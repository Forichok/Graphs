using System.Collections.Generic;
using System.Linq;
using Graphs.Sources.Models;

namespace Graphs.Sources.Tasks
{
    public static class BFSTask2
    {
        public static List<LinkModel> BreadthFirstSearch(IEnumerable<MappedNode> mapedEnumerable, string keyFrom, string keyTo)
        {
            var result = new List<LinkModel>();

            var queue = new Queue<string>();
            queue.Enqueue(keyFrom);

            var dataDict = new Dictionary<string, BfsData>();
            var mapedList = mapedEnumerable.ToList();

            foreach (var mappedNode in mapedList)
            {
                dataDict.Add(mappedNode.Node.Key, new BfsData{Node = mappedNode});
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
                    if (dataDict[link.To].IsVisited == false)
                    {      // ... которые ещё не были посещены ...
                        queue.Enqueue(link.To);                // ... добавить в конец очереди...
                        dataDict[link.To].IsVisited = true;            // ... и пометить как посещённые
                        dataDict[link.To].ParentMappedNode = nextMaped;
                        dataDict[link.To].ParentLink = link;
                    }

                    if (!link.IsOriented && dataDict[link.From].IsVisited == false)
                    {
                        queue.Enqueue(link.From);                // ... добавить в конец очереди...
                        dataDict[link.From].IsVisited = true;
                        dataDict[link.From].ParentMappedNode = nextMaped;
                        dataDict[link.From].ParentLink = link;
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

            return result;
        }

    }

    
}
