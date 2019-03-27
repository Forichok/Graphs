using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Graphs.Sources.Helpers;
using Graphs.Sources.Models;
using Graphs.Sources.Tasks;
using Northwoods.GoXam;
using Northwoods.GoXam.Model;

namespace Graphs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            myDiagram.LayoutCompleted += UpdateRoutes;

            //var tool = new SimpleLabelDraggingTool();
            //tool.Diagram = myDiagram;

            //myDiagram.MouseMoveTools.Insert(0, tool);

            myDiagram.NodeCreated += MyDiagram_NodeCreated;

            myDiagram.LinkDrawn += MyDiagram_LinkDrawn;

            //myDiagram.LinkReshaped += MyDiagram_LinkDrawn;

            //myDiagram.LinkRelinked += MyDiagram_LinkDrawn;
        }

        private void MyDiagram_LinkDrawn(object sender, DiagramEventArgs e)
        {
            var linkModel = e.Part.Data as LinkModel;

            linkModel.Text = ((int) (GetNode(linkModel.From).Location - GetNode(linkModel.To).Location).Length / 100).ToString();
        }

        private GraphLinksModelNodeData<string> GetNode(string key)
        {
            foreach (NodeModel node in myDiagram.Model.NodesSource)
                if (node.Key == key)
                    return node;

            return null;
        }

        private void MyDiagram_NodeCreated(object sender, DiagramEventArgs e)
        {
            var nodeModel = e.Part.Data as NodeModel;
            var list = MainModel.NodesModelToArr(myDiagram.Model.NodesSource.Cast<NodeModel>());
            var key = NodeKeyCreator.GetNodeName(list);
        
            nodeModel.Text = key;
            nodeModel.Key = key;
        }

        // only use the saved route points after the layout has completed,
        // because links will get the default routing
        private void UpdateRoutes(object sender, DiagramEventArgs e)
        {
            // just set the Route points once per Load
            myDiagram.LayoutCompleted -= UpdateRoutes;
            foreach (var link in myDiagram.Links)
            {
                var linkModel = link.Data as LinkModel;
                if (linkModel != null && linkModel.Points != null && linkModel.Points.Count() > 1)
                    link.Route.Points = (IList<Point>) linkModel.Points;
            }

            myDiagram.PartManager.UpdatesRouteDataPoints =
                true; // OK for CustomPartManager to update LinkModel.Points automatically
        }
    }
}