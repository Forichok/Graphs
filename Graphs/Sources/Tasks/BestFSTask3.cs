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
            {            // ���� ������� �� �����

                var node = queue.Pop();                 // ������� ������ ������� � �������

                nextMaped = dataDict[node].Node;

                if (node == keyTo)
                {
                    isOk = true;
                    break;                      // ���������, �� �������� �� ������� ���� �������
                }


                foreach (var link in nextMaped.Links)
                {    // ��� ��������� �������� ����, ...
                    var to = link.GetTo(node);
                    if (dataDict[to].IsVisited == false)
                    {      // ... ������� ��� �� ���� �������� ...
                        var cost = int.Parse(link.Text) * -1;
                        queue.Add(cost,to);                // ... �������� � ����� �������...
                                                                // ... � �������� ��� ����������
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
                    result.Add(dataDict[nextMaped.Node.Key].ParentLink);//� ���������� � ����
                    //���� ���������� ���������� �������
                    nextMaped = dataDict[nextMaped.Node.Key].ParentMappedNode; //��������� � ��

                }
            }

            return new KeyValuePair<IEnumerable<LinkModel>, string>(result, UniversalGraphNodeData.GetVector(dataDict, keyTo));

        }
    }
}
