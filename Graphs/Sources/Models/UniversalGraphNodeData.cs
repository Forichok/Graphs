using System.Collections.Generic;
using System.Text;

namespace Graphs.Sources.Models
{
    public class UniversalGraphNodeData
    {
        public MappedNode Node = null;
        public bool IsVisited = false;
        public LinkModel ParentLink = null;
        public MappedNode ParentMappedNode = null;

        public int Cost = -1;

        public static string GetVector(Dictionary<string, UniversalGraphNodeData> dataDict, string to)
        {
            var sb = new StringBuilder();
            var nextMaped = dataDict[to].Node;

            while (dataDict[nextMaped.Node.Key].ParentLink != null)
            {
                sb.Insert(0, "->" + dataDict[nextMaped.Node.Key].Node.Node.Text + $" [{dataDict[nextMaped.Node.Key].Node.Node.Key}]");//и дописываем к пути

                nextMaped = dataDict[nextMaped.Node.Key].ParentMappedNode; //переходим в неё

            }

            sb.Insert(0, nextMaped.Node.Text + $" [{nextMaped.Node.Key}]");

            return sb.ToString();
        }

        public static string GetVector(IEnumerable<UniversalGraphNodeData> dataList, string to)
        {
            var dataDict = new Dictionary<string, UniversalGraphNodeData>();

            foreach (var data in dataList)
            {
                dataDict.Add(data.Node.Node.Key, data);
            }
            return GetVector(dataDict, to);
        }
    }
}
