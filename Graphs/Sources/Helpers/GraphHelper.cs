using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphs.Sources.Models;
using Northwoods.GoXam;
using Northwoods.GoXam.Model;

namespace Graphs.Sources.Helpers
{
    static class GraphHelper
    {
        public static IEnumerable<IEnumerable<int>> GetMatrix(this GraphLinksModel<NodeModel, string, string, LinkModel> model)
        {
            List<List<int>> matrix = new List<List<int>>();


            var dictionary = new Dictionary<string, Dictionary<string, LinkModel>>();
            foreach (NodeModel node in model.NodesSource)
            {
                if (!dictionary.ContainsKey(node.Key))
                {
                    var a = new Dictionary<String, LinkModel>();
                    dictionary.Add(node.Key, a);

                }
            }

            foreach (LinkModel link in model.LinksSource)
            {
                dictionary[link.From][link.To] = link;
            }

            foreach (var from in dictionary.Keys)
            {
                var line = new List<int>();
                foreach (var to in dictionary.Keys)
                {
                    int weight;

                    if (dictionary[to].ContainsKey(from) && !dictionary[to][from].IsOriented)
                    {
                        int.TryParse(dictionary[to][from].Weight, out weight);
                    }

                    else if (dictionary[from].ContainsKey(to))
                    {
                        int.TryParse(dictionary[from][to].Weight, out weight);
                    }
                    else
                    {
                        weight = -1;
                    }

                    line.Add(weight);
                }

                matrix.Add(line);
            }

            return matrix;
        }

        public static List<int> GetAllValues( this GraphLinksModel<NodeModel, string, string, LinkModel> model)
        {
            var matrix = model.GetMatrix();

            var values = new List<int>();

            foreach (var row in matrix)
            {
                foreach (var cell in row)
                {
                   values.Add(cell);
                }
            }

            return values;
        }
    }
}
