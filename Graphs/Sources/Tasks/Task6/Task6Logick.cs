using System.Collections.Generic;
using System.Linq;
using Graphs.Sources.Models;

namespace Graphs.Sources.Tasks.Task6
{
    public class Task6Logick
    {
        public class ResultData
        {
            public KeyValuePair<int, string> RadiusData { get; set; }
            public KeyValuePair<int, string> DiametrData { get; set; }
        }

        private ResultData _result;

        public ResultData BeginTask6(IEnumerable<MappedNode> mapedEnumerable)
        {
            _result = new ResultData();
            var mappedList = mapedEnumerable.ToList();

            FindRadiusAndDim(mappedList);

            return _result;
        }

        private void FindRadiusAndDim(List<MappedNode> mapped)
        {
            var minCost = 0;
            var minName = "";
            List<UniversalGraphNodeData> minVector = null;

            var maxCost = 0;
            var maxName = "";
            List<UniversalGraphNodeData> maxVector = null;

            foreach (var mappedNode in mapped)
            {
                var djResult = DijkstraTask4.StartDijkstra(mapped, mappedNode.Node.Key);
                foreach (var universalGraphNodeData in djResult.Values)
                {
                    if (universalGraphNodeData.Cost > maxCost)
                    {
                        maxCost = universalGraphNodeData.Cost;
                        maxName = universalGraphNodeData.Node.Node.Key;
                        maxVector = djResult.Values.ToList();
                    }
                    else if (universalGraphNodeData.Cost < minCost)
                    {
                        minCost = universalGraphNodeData.Cost;
                        minName = universalGraphNodeData.Node.Node.Key;
                        minVector = djResult.Values.ToList();
                    }
                }
            }   

            _result.RadiusData = new KeyValuePair<int, string>(minCost, UniversalGraphNodeData.GetVector(minVector, minName));
            _result.DiametrData = new KeyValuePair<int, string>(maxCost, UniversalGraphNodeData.GetVector(maxVector, maxName));

        }
    }
}
