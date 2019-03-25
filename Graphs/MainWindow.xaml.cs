using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Graphs.Helpers;
using Graphs.Models;
using Graphs.Tools;
using Microsoft.Win32;
using Northwoods.GoXam;
using Northwoods.GoXam.Model;

namespace Graphs
{

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            myDiagram.LayoutCompleted += UpdateRoutes;

            var tool = new SimpleLabelDraggingTool();
            tool.Diagram = myDiagram;

            myDiagram.MouseMoveTools.Insert(0, tool);

            myDiagram.NodeCreated += MyDiagram_NodeCreated;

            myDiagram.LinkDrawn += MyDiagram_LinkDrawn;

            myDiagram.LinkReshaped += MyDiagram_LinkDrawn;

            myDiagram.LinkRelinked += MyDiagram_LinkDrawn;
        }

        private void MyDiagram_LinkDrawn(object sender, DiagramEventArgs e)
        {
            var linkModel = e.Part.Data as LinkModel;


            linkModel.Text = ((int) (GetNode(linkModel.From).Location - GetNode(linkModel.To).Location).Length / 100)
                .ToString();
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
            var key = NodeNameCreator.GetNodeName();

            nodeModel.Text = key;
            nodeModel.Key = key;
        }

        // save and load the model data as XML, visible in the "Saved" tab of the Demo       

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var model = myDiagram.Model as GraphLinksModel<NodeModel, string, string, LinkModel>;
            if (model == null) return;
            try
            {
                var root = XElement.Parse(LoadFromFile());
                // set the Route.Points after nodes have been built and the layout has finished
                myDiagram.LayoutCompleted += UpdateRoutes;
                // tell the CustomPartManager that we're loading
                myDiagram.PartManager.UpdatesRouteDataPoints = false;
                model.Load<NodeModel, LinkModel>(root, "NodeModel", "LinkModel");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            model.IsModified = false;
        }

        private string LoadFromFile()
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            var openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.Filter = "All files (*.*)|*.*|xml files (*.xml)|*.xml";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                //Get the path of specified file
                filePath = openFileDialog.FileName;

                //Read the contents of the file into a stream
                var fileStream = openFileDialog.OpenFile();

                using (var reader = new StreamReader(fileStream))
                {
                    fileContent = reader.ReadToEnd();
                }
            }

            return fileContent;
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