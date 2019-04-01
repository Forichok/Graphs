using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Navigation;
using Graphs.Sources.Helpers;
using Graphs.Sources.Models;
using Graphs.Sources.Tasks.Task8;
using Northwoods.GoXam.Model;

namespace Graphs.Sources.Tasks
{
    class ConnectivityTask8
    {
        private GraphLinksModel<NodeModel, string, string, LinkModel> model;
        private Graph<String> graph;

        public ConnectivityTask8(GraphLinksModel<NodeModel, string, string, LinkModel> model)
        {
            this.model = model;
        }

        public String CheckConnectivity()
        {
            StringBuilder result = new StringBuilder();

            var connectedComponents = GetConnectivityComponents();

            var bridges = GetBridges();

            result.Append(connectedComponents.Count == 1 ? "1. Graph is connected\n\n" : "1. Graph isn't connected\n\n");

            result.Append(connectedComponents.Count == 1 ? "2. Graph is strongly connected" : (IsWeaklyConnected() ? "2. Graph is weakly connected" : "2. Graph isn't connected"));

            result.Append("\n\n3. Connectivity components:");
            for (int i = 0; i < connectedComponents.Count; i++)
            {
                result.Append($"\n{i + 1}: ");
                foreach (var node in connectedComponents[i])
                {
                    result.Append(node + " ");
                }
            }


            if (bridges == null || bridges.Count == 0)
            {
                result.Append("\n\n4. No bridges");
            }
            else
            {
                result.Append("\n\n4. Bridges:\n");
                foreach (var bridge in bridges)
                {
                    result.Append($"{bridge.vertexA}==>{bridge.vertexB} \n");
                }
            }


            var articulationPoints = TarjansArticulationFinder<String>.FindArticulationPoints(graph);
            result.Append("\n\n5. Articulation points: ");

            foreach (var point in articulationPoints)
            {
                result.Append(point + " ");
            }


            return result.ToString();
        }

        private List<Bridge<string>> GetBridges()
        {


            graph = new Graph<String>();

            foreach (NodeModel node in model.NodesSource)
            {
                graph.AddVertex(node.Key);
            }

            foreach (LinkModel link in model.LinksSource)
            {
                if (link.IsOriented)
                {
                    // MessageBox.Show("Bridges: Error! Remove oriented nodes","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    return null;
                }


                if (!link.IsOriented)
                    graph.AddEdge(link.From, link.To);
            }

            var bridgeFinder = new BridgeFinder<string>();
            return bridgeFinder.FindBridges(graph);
        }

        private bool IsWeaklyConnected() // Ориентированный граф называется слабо-связным, если является связным неориентированный граф, полученный из него заменой ориентированных рёбер неориентированными.
        {
            var graph = new DiGraph<String>();

            foreach (NodeModel node in model.NodesSource)
            {
                graph.AddVertex(node.Key);
            }

            foreach (LinkModel link in model.LinksSource)
            {
                graph.AddEdge(link.From, link.To);
                graph.AddEdge(link.To, link.From);
            }

            var connectivityComponentsFinder = new KosarajuStronglyConnected<String>();
            return connectivityComponentsFinder.FindStronglyConnectedComponents(graph).Count==1;
        }

        private List<List<string>> GetConnectivityComponents()
        {
            var graph = new DiGraph<String>();

            foreach (NodeModel node in model.NodesSource)
            {
                graph.AddVertex(node.Key);
            }

            foreach (LinkModel link in model.LinksSource)
            {
                graph.AddEdge(link.From, link.To);
                if (!link.IsOriented)
                    graph.AddEdge(link.To, link.From);
            }

            var connectivityComponentsFinder = new KosarajuStronglyConnected<String>();
            return connectivityComponentsFinder.FindStronglyConnectedComponents(graph);
        }
    }



    /// <summary>
    /// A Kosaraju Strong Connected Component Algorithm Implementation.
    /// </summary>
    public class KosarajuStronglyConnected<T> //Ориентированный граф называется сильно-связным, если в нём существует(ориентированный) путь из любой вершины в любую другую
    {
        /// <summary>
        /// Returns all Connected Components using Kosaraju's Algorithm.
        /// </summary>
        public List<List<T>>
            FindStronglyConnectedComponents(DiGraph<T> graph)
        {
            var visited = new HashSet<T>();
            var finishStack = new Stack<T>();

            //step one - create DFS finish visit stack
            foreach (var vertex in graph.Vertices)
            {
                if (!visited.Contains(vertex.Value.Value))
                {
                    kosarajuStep1(vertex.Value, visited, finishStack);
                }
            }

            //reverse edges
            var reverseGraph = reverseEdges(graph);

            visited.Clear();

            var result = new List<List<T>>();

            //now pop finish stack and gather the components
            while (finishStack.Count > 0)
            {
                var currentVertex = reverseGraph.FindVertex(finishStack.Pop());

                if (!visited.Contains(currentVertex.Value))
                {
                    result.Add(kosarajuStep2(currentVertex, visited,
                        finishStack, new List<T>()));
                }
            }

            return result;
        }

        /// <summary>
        /// Just do a DFS keeping track on finish Stack of Vertices.
        /// </summary>
        private void kosarajuStep1(DiGraphVertex<T> currentVertex,
            HashSet<T> visited,
            Stack<T> finishStack)
        {
            visited.Add(currentVertex.Value);

            foreach (var edge in currentVertex.OutEdges)
            {
                if (!visited.Contains(edge.Value))
                {
                    kosarajuStep1(edge, visited, finishStack);
                }
            }

            //finished visiting, so add to stack
            finishStack.Push(currentVertex.Value);
        }

        /// <summary>
        /// In step two we just add all reachable nodes to result (connected componant).
        /// </summary>
        private List<T> kosarajuStep2(DiGraphVertex<T> currentVertex,
            HashSet<T> visited, Stack<T> finishStack,
            List<T> result)
        {
            visited.Add(currentVertex.Value);
            result.Add(currentVertex.Value);

            foreach (var edge in currentVertex.OutEdges)
            {
                if (!visited.Contains(edge.Value))
                {
                    kosarajuStep2(edge, visited, finishStack, result);
                }
            }

            return result;
        }

        /// <summary>
        /// Create a clone graph with reverse edge directions.
        /// </summary>
        private DiGraph<T> reverseEdges(DiGraph<T> graph)
        {
            var newGraph = new DiGraph<T>();

            foreach (var vertex in graph.Vertices)
            {
                newGraph.AddVertex(vertex.Key);
            }

            foreach (var vertex in graph.Vertices)
            {
                foreach (var edge in vertex.Value.OutEdges)
                {
                    //reverse edge
                    newGraph.AddEdge(edge.Value, vertex.Value.Value);
                }
            }

            return newGraph;
        }
    }
}
