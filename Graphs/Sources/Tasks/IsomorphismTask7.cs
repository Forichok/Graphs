using Graphs.Sources.Models;
using Northwoods.GoXam.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Graphs.Sources.Helpers;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using Northwoods.GoXam;
using Utilities.DataTypes;


namespace Graphs.Sources.Tasks
{ 
    class IsomorphismTask7
    {
        private GraphLinksModel<NodeModel, string, string, LinkModel> model1;
        private GraphLinksModel<NodeModel, string, string, LinkModel> model2;

        public IsomorphismTask7(GraphLinksModel<NodeModel, string, string, LinkModel> model1, GraphLinksModel<NodeModel, string, string, LinkModel> model2)
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
            var values2 = model1.GetAllValues();

            Matrix<Single> matrix1 = new DenseMatrix(nodes1.Count,nodes1.Count,values1.Select(Convert.ToSingle).ToArray());
            Matrix<Single> matrix2 = new DenseMatrix(nodes2.Count, nodes2.Count, values1.Select(Convert.ToSingle).ToArray());

            var tmpMatrix = new DenseMatrix(nodes2.Count, nodes2.Count, values1.Select(Convert.ToSingle).ToArray());
            foreach (var row in matrix2.EnumerateRows())
            {
                foreach (var column in matrix2.EnumerateColumns())
                {
                    if (tmpMatrix.Equals(matrix1))
                        return true;
                }
            }
            return matrix2.Equals(matrix1);


            return false;
        }
    }
}
