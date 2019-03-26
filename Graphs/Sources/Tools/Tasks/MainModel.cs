using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using DevExpress.Mvvm;
using Graphs.Models;
using Northwoods.GoXam.Model;

namespace Graph.sources.mvvm.models
{
    public class MainModel : ViewModelBase
    {
        public static KeyValuePair<IEnumerable<NodeModel>, IEnumerable<LinkModel>> LoadAdjencyMatrix(string filePath)
        {
            var resultWithoutComments = ReadWithoutComments(filePath);

            var nodesList = new List<NodeModel>();
            var linksList = new List<LinkModel>();

            var regex = new Regex(@"^Vertex{([\w]+)\(([-]*[\d]+),([-]*[\d]+)\)}$", RegexOptions.Compiled);

            var lines = resultWithoutComments.ToString().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var checkSum = -1;
            var linesCount = 0;
            var vertexCount = 0;
            foreach (var line in lines)
            {
                if (regex.IsMatch(line))
                {
                    var groups = regex.Match(line).Groups;
                    var name = groups[1].Value;
                    var x = int.Parse(groups[2].Value);
                    var y = int.Parse(groups[3].Value);

                    if (nodesList.Count - 1 < vertexCount)
                    {
                        var NodeModel = new NodeModel(vertexCount.ToString(), name) { Location = new Point(x, y) };
                        nodesList.Add(NodeModel);
                    }
                    else
                    {
                        nodesList[vertexCount].Location = new Point(x, y);
                        nodesList[vertexCount].Text = name;
                    }
                    vertexCount++;
                }
                else
                {
                    var splitedLine = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    linesCount++;
                    if (checkSum == -1)
                    {
                        checkSum = splitedLine.Length;
                        for (var i = vertexCount; i < splitedLine.Length && nodesList.Count < splitedLine.Length; i++)
                        {
                            var NodeModel = new NodeModel(i.ToString(), i.ToString());
                            nodesList.Add(NodeModel);
                        }
                    }
                    else if (checkSum != splitedLine.Length)
                    {
                        throw new Exception("#Error in AdjencyMatrix: Lines have different sizes");
                    }

                    for (int i = 0; i < splitedLine.Length; i++)
                    {
                        if (splitedLine[i] == "0")
                            continue;
                        var LinkModel = new LinkModel(nodesList[i].Key, nodesList[linesCount - 1].Key, splitedLine[i]);
                        linksList.Add(LinkModel);
                    }

                }
            }

            if (linesCount != checkSum)
                throw new Exception("#Error in AdjencyMatrix: Non-square matrix");

            return new KeyValuePair<IEnumerable<NodeModel>, IEnumerable<LinkModel>>(nodesList, linksList);
        }

        public static void SaveAdjencyMatrix(string filePath, GraphLinksModel<NodeModel, string, string, LinkModel> model)
        {
            using (var sw = new StreamWriter(filePath))
            {
                var nodes = (ObservableCollection<NodeModel>)model.NodesSource;
                foreach (var node in nodes)
                {
                    sw.WriteLine($"Vertex{{{node.Text}({(int)node.Location.X},{(int)node.Location.Y})}}");
                }

                var links = (ObservableCollection<LinkModel>)model.LinksSource;
                var matrix = CreateMatrix(links, nodes);

                for (int i = 0; i < nodes.Count; i++)
                {
                    for (int j = 0; j < nodes.Count; j++)
                    {
                        if (string.IsNullOrEmpty(matrix[i, j, 0]))
                            sw.Write($"0 ");
                        else
                            sw.Write($"{matrix[i, j, 0]} ");
                    }
                    sw.WriteLine();
                }


            }
        }

        public static string[,,] CreateMatrix(IEnumerable<LinkModel> links, IEnumerable<NodeModel> nodes)
        {
            var count = nodes.Count();
            var result = new string[count, count, 1];

            var nodesList = nodes.Select(i => i.Key).ToList();

            foreach (var LinkModel in links)
            {
                var fromIndex = nodesList.IndexOf(LinkModel.From);
                var toIndex = nodesList.IndexOf(LinkModel.To);
                var text = LinkModel.Text;

                if (!string.IsNullOrEmpty(result[fromIndex, toIndex, 0]))
                    throw new Exception("#Error in save adj: multigraph");

                result[fromIndex, toIndex, 0] = text;
            }

            return result;
        }

        public static KeyValuePair<IEnumerable<NodeModel>, IEnumerable<LinkModel>> LoadIncidenceMatrix(string filePath)
        {
            var resultWithoutComments = ReadWithoutComments(filePath);

            var nodesList = new List<NodeModel>();
            var linksList = new List<LinkModel>();

            var regex = new Regex(@"^Vertex{([\w]+)\(([-]*[\d]+),([-]*[\d]+)\)}$", RegexOptions.Compiled);

            var lines = resultWithoutComments.ToString().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            var vertexCount = 0;
            var checkSum = -1;

            var matrix = new List<List<string>>();

            foreach (var line in lines)
            {
                if (regex.IsMatch(line))
                {
                    var groups = regex.Match(line).Groups;
                    var name = groups[1].Value;
                    var x = int.Parse(groups[2].Value);
                    var y = int.Parse(groups[3].Value);

                    if (nodesList.Count - 1 < vertexCount)
                    {
                        var NodeModel = new NodeModel(vertexCount.ToString(), name) { Location = new Point(x, y) };
                        nodesList.Add(NodeModel);
                    }
                    else
                    {
                        nodesList[vertexCount].Location = new Point(x, y);
                        nodesList[vertexCount].Text = name;
                    }
                    vertexCount++;
                }
                else
                {
                    var splitedLine = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (checkSum == -1)
                    {
                        checkSum = splitedLine.Length;
                        for (int i = 0; i < splitedLine.Length; i++)
                        {
                            matrix.Add(new List<string>());
                        }
                    }
                    else if (checkSum != splitedLine.Length)
                        throw new Exception("#Error in AdjencyMatrix: Lines have different sizes");

                    for (int i = 0; i < splitedLine.Length; i++)
                    {
                        matrix[i].Add(splitedLine[i]);
                    }
                }

                //normalize list of nodes
                for (int i = vertexCount; i < matrix.First().Count; i++)
                {
                    var NodeModel = new NodeModel(i.ToString(), i.ToString());
                    nodesList.Add(NodeModel);
                }

                for (int i = 0; i < matrix.Count; i++)
                {
                    for (int j = 0; j < matrix[0].Count; j++)
                    {

                    }
                }

            }

            return new KeyValuePair<IEnumerable<NodeModel>, IEnumerable<LinkModel>>(nodesList, linksList);
        }


        public static void SaveIncidenceMatrix(string fileName, GraphLinksModel<NodeModel, string, string, LinkModel> model)
        {


        }

        private static StringBuilder ReadWithoutComments(string filePath)
        {
            var streamReader = new StreamReader(filePath);
            var sb = new StringBuilder();

            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                if (line == null || line.First() == '%')
                {
                    continue;
                }
                sb.AppendLine(line);
            }
            return sb;
        }


    }
}
