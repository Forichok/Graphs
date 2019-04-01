using System;
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
            
            public string DegreeVector { get; set; }
        }

        private ResultData _result;

        public ResultData BeginTask6(IEnumerable<MappedNode> mapedEnumerable)
        {
            _result = new ResultData();
            var mappedList = mapedEnumerable.ToList();

            FindRadiusAndDim(mappedList);
            CreateDegreeVector(mappedList);

            return _result;
        }

        private void CreateDegreeVector(List<MappedNode> mappedList)
        {
            var dataDict = new Dictionary<string, int>();
            foreach (var mappedNode in mappedList)
            {
                dataDict.Add(mappedNode.Node.Key, 0);
            }

            foreach (var mappedNode in mappedList)
            {
                foreach (var link in mappedNode.Links)
                {
                    if (link.To == link.From)
                        dataDict[mappedNode.Node.Key] += 2;
                    else
                    {
                        dataDict[mappedNode.Node.Key] += 1;
                        if(link.IsOriented)
                            dataDict[link.GetTo(mappedNode.Node.Key)] += 1;
                    }
                }       
            }

            var degreeVector = dataDict.Values.Select((t)=>t).ToList();
            degreeVector.Sort((f, s) =>
            {

                if (f > s)
                    return -1;
                if (f == s)
                    return 0;

                return 1;
            });

            _result.DegreeVector = string.Join(", ", degreeVector);
        }

        private void FindRadiusAndDim(List<MappedNode> mapped)
        {
            var minCost = Int32.MaxValue;
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
                    if(universalGraphNodeData.Node == mappedNode)
                        continue;

                    if (universalGraphNodeData.Cost > maxCost)
                    {
                        maxCost = universalGraphNodeData.Cost;
                        maxName = universalGraphNodeData.Node.Node.Key;
                        maxVector = djResult.Values.ToList();
                    }

                    if (universalGraphNodeData.Cost < minCost & universalGraphNodeData.Cost != -1)
                    {
                        minCost = universalGraphNodeData.Cost;
                        minName = universalGraphNodeData.Node.Node.Key;
                        minVector = djResult.Values.ToList();
                    }
                }
            }   

            if(minVector != null)
                _result.RadiusData = new KeyValuePair<int, string>(minCost, UniversalGraphNodeData.GetVector(minVector, minName));
            else
                _result.RadiusData = new KeyValuePair<int, string>(-1, "none");

            if(maxVector != null)
                _result.DiametrData = new KeyValuePair<int, string>(maxCost, UniversalGraphNodeData.GetVector(maxVector, maxName));
            else
                _result.DiametrData = new KeyValuePair<int, string>(-1, "none");

        }

       
    }
}
