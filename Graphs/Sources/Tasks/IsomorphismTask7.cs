using Graphs.Sources.Models;
using Northwoods.GoXam.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Graphs.Sources.Helpers;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using vflibcs;


namespace Graphs.Sources.Tasks
{
    class IsomorphismTask7
    {
        private GraphLinksModel<NodeModel, string, string, LinkModel> model1;
        private GraphLinksModel<NodeModel, string, string, LinkModel> model2;

        public IsomorphismTask7(GraphLinksModel<NodeModel, string, string, LinkModel> model1,
            GraphLinksModel<NodeModel, string, string, LinkModel> model2)
        {
            this.model2 = model2;
            this.model1 = model1;
        }

        public bool IsIsomorphy()
        {
            var nodes1 = new List<NodeModel>((model1.NodesSource).Cast<NodeModel>());
            var nodes2 = new List<NodeModel>((model2.NodesSource).Cast<NodeModel>());

            var m1 = model1.GetMatrix();
            var m = model2.GetMatrix();

            var values1 = model1.GetAllValues();
            var values2 = model2.GetAllValues();

            Matrix<Single> matrix1 =
                new DenseMatrix(nodes1.Count, nodes1.Count, values1.Select(Convert.ToSingle).ToArray());
            Matrix<Single> matrix2 =
                new DenseMatrix(nodes2.Count, nodes2.Count, values2.Select(Convert.ToSingle).ToArray());

            var tmpMatrix = new DenseMatrix(nodes1.Count, nodes1.Count, values1.Select(Convert.ToSingle).ToArray());

            
            var rows = matrix1.EnumerateRows().ToList();

            var rowsPermutation = PermutationExtension.Permute(rows);


            Graph<String, String> graph1 = new Graph<string, string>();
            Graph<String, String> graph2 = new Graph<string, string>();

            // The first nodes is always ID 0 and the rest
            // follow so we have nodes 0, 1, 2 and 3
            var nodesDictionary1 = new Dictionary<string, int>();
            var nodesDictionary2 = new Dictionary<string, int>();
            int id = 0;
            foreach (NodeModel node in model1.NodesSource)
            {
                graph1.InsertVertex(node.Key);
                nodesDictionary1.Add(node.Key, id++);
            }

            id = 0;
            foreach (NodeModel node in model2.NodesSource)
            {
                graph2.InsertVertex(GetIntHash(node.Key).ToString());
                nodesDictionary2.Add(node.Key, id++);
            }

            foreach (LinkModel link in model1.LinksSource)
            {
                graph1.AddEdge(nodesDictionary1[link.From],nodesDictionary1[link.To],link.Weight);
                
            }

            foreach (LinkModel link in model2.LinksSource)
            {
                graph2.AddEdge(nodesDictionary2[link.From], nodesDictionary2[link.To], link.Weight);
            }

            var vfs = new VfState<String,String>(graph1, graph2,true,true);
            FullMapping fIsomorphic = vfs.Match();

            if (fIsomorphic == null)
                return false;

            return true;
            

            foreach (var permutedRows in rowsPermutation)
            {

                tmpMatrix.Clear();
                int rowId = 0;
                foreach (var row in permutedRows)
                {
                    tmpMatrix.SetRow(rowId++, row);
                }

                if (AreEquals(tmpMatrix, matrix2))
                    return true;

                var columns = matrix1.EnumerateColumns().ToList();
                var columnsPermutation = PermutationExtension.Permute(columns);

                foreach (var permutedColumns in columnsPermutation)
                {
                    tmpMatrix.Clear();
                    int columnId = 0;
                    foreach (var column in permutedColumns)
                    {
                        tmpMatrix.SetColumn(columnId++, column);
                    }

                    if (AreEquals(tmpMatrix, matrix2))
                       return true;

                }
            }

            //for (var row = 0; row < rows.Count; row++)
            //{
            //    for (int k = 0; k < rows.Count; k++)
            //    {
            //        for (var column = 0; column < columns.Count; column++)
            //        {
            //            for (int i = 0; i < columns.Count; i++)
            //            {
            //                if (AreEquals(tmpMatrix, matrix2))
            //                    return true;

            //                if (i == column) continue;

            //                tmpMatrix.SetColumn(column, columns[i]);
            //                tmpMatrix.SetColumn(i, columns[column]);
            //            }
            //        }

            //        if (k == row) continue;
            //        tmpMatrix.SetRow(row, rows[k]);
            //        tmpMatrix.SetRow(k, rows[row]);
            //    }
            //}

            return false;
        }

        private bool AreEquals(Matrix<Single> matrix1, Matrix<Single> matrix2)
        {

            //return matrix2.Equals(matrix1);
            var a = matrix1.Enumerate().ToList();
            var b = matrix2.Enumerate().ToList();

            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i]) return false;
            }

            return true;
            
        }

        private int GetIntHash(String str)
        {
            MD5 md5Hasher = MD5.Create();
            var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(str));
            var ivalue = BitConverter.ToInt32(hashed, 0);
            return ivalue;
        }




    }
}