using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
    /// Interaction logic for MainWindow.xaml
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
            LinkModel linkModel = e.Part.Data as LinkModel;


            linkModel.Text = ((int)(GetNode(linkModel.From).Location - GetNode(linkModel.To).Location).Length / 100).ToString();
        }

        private GraphLinksModelNodeData<string> GetNode(String key)
        {
            foreach (NodeModel node in myDiagram.Model.NodesSource)
            {
                if (node.Key == key)
                    return node;
            }

            return null;
        }

        private void MyDiagram_NodeCreated(object sender, DiagramEventArgs e)
        {
            NodeModel nodeModel = e.Part.Data as NodeModel;
            String key = NodeNameCreator.GetNodeName();

            nodeModel.Text = key;
            nodeModel.Key = key;
        }

        // save and load the model data as XML, visible in the "Saved" tab of the Demo       

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var model = myDiagram.Model as GraphLinksModel<NodeModel, String, String, LinkModel>;
            if (model == null) return;
            try
            {
                XElement root = XElement.Parse(LoadFromFile());
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

        private String LoadFromFile()
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

                using (StreamReader reader = new StreamReader(fileStream))
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
            foreach (Link link in myDiagram.Links)
            {
                LinkModel linkModel = link.Data as LinkModel;
                if (linkModel != null && linkModel.Points != null && linkModel.Points.Count() > 1)
                {
                    link.Route.Points = (IList<Point>)linkModel.Points;
                }
            }
            myDiagram.PartManager.UpdatesRouteDataPoints = true;  // OK for CustomPartManager to update LinkModel.Points automatically
        }

    }
}
