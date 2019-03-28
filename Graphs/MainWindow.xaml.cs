using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.POCO;
using Graphs.Sources.Helpers;
using Graphs.Sources.Models;
using Northwoods.GoXam;
using Northwoods.GoXam.Model;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Graphs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Line> matrixData;
        public MainWindow()
        {
            InitializeComponent();

            matrixData = new ObservableCollection<Line>();

            myDiagram.LayoutCompleted += UpdateRoutes;

            //var tool = new SimpleLabelDraggingTool();
            //tool.Diagram = myDiagram;

            //myDiagram.MouseMoveTools.Insert(0, tool);

            myDiagram.NodeCreated += MyDiagram_NodeCreated;

            myDiagram.LinkDrawn += MyDiagram_LinkDrawn;

            //myDiagram.LinkReshaped += MyDiagram_LinkDrawn;

            //myDiagram.LinkRelinked += MyDiagram_LinkDrawn;

            MatrixControl.ItemsSource = matrixData;
        }



        public void UpdateMatrix(GraphLinksModel<NodeModel, string, string, LinkModel> model, Diagram diagram)
        {
            ObservableCollection<Line> lines = new ObservableCollection<Line>();


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
                ObservableCollection<LinkModel> routes = new ObservableCollection<LinkModel>();
                foreach (var to in dictionary.Keys)
                {
                    LinkModel linkModel;

                    if (dictionary[to].ContainsKey(from) && !dictionary[to][from].IsOriented)
                    {
                        linkModel = dictionary[to][from];
                    }

                    else if (dictionary[from].ContainsKey(to))
                    {
                        linkModel = dictionary[from][to];
                    }
                    else
                    {
                        linkModel = new LinkModel(from, to, "") { model = model, DiagramModel = diagram };
                    }
                    routes.Add(linkModel);


                }
                lines.Add(new Line() { Heading = from, Values = routes });
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                matrixData.Clear();
                foreach (var line in lines)
                {
                    matrixData.Add(line);
                }
            });
        }


        private void MyDiagram_LinkDrawn(object sender, DiagramEventArgs e)
        {
            var linkModel = e.Part.Data as LinkModel;

            linkModel.Weight = ((int)(GetNode(linkModel.From).Location - GetNode(linkModel.To).Location).Length / 100).ToString();

            int from = getNodeIndex(linkModel.From, myDiagram.Model.NodesSource);
            int to = getNodeIndex(linkModel.To, myDiagram.Model.NodesSource);

            matrixData[from].Values[to] = linkModel;
            //linkModel.LinkChangedHandler += LinkChanged;
            //var model = myDiagram.Model as GraphLinksModel<NodeModel, string, string, LinkModel>;
            //Task.Factory.StartNew(()=>UpdateMatrix(model,myDiagram));
        }

        private int getNodeIndex(String key, IEnumerable nodes)
        {
            int index = 0;
            foreach (NodeModel node in nodes)
            {
                if (node.Key == key)
                    return index;
                index++;
            }

            return -1;
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
            var key = NodeKeyCreator.GetNodeName(myDiagram.Model.NodesSource.Cast<NodeModel>().Select(i=>i.Key));
            

            nodeModel.Text = key;
            nodeModel.Key = key;

            AddNodeToMatrix(key);
        }

        private void AddNodeToMatrix(string key)
        {
            var values = new ObservableCollection<LinkModel>();
            foreach (NodeModel node in myDiagram.Model.NodesSource)
            {
                var linkModel = new LinkModel(key, node.Key, "")
                {
                    model = myDiagram.Model as GraphLinksModel<NodeModel, string, string, LinkModel>,
                    DiagramModel = myDiagram
                };
                //linkModel.LinkChangedHandler += LinkChanged;
                values.Add(linkModel);
            }

            foreach (var line in matrixData)
            {
                line.Values.Add(new LinkModel(key, line.Heading, "")
                {
                    model = myDiagram.Model as GraphLinksModel<NodeModel, string, string, LinkModel>,
                    DiagramModel = myDiagram
                });
            }

            matrixData.Add(new Line()
            {
                Heading = key,
                Values = values,
                Position = getNodeIndex(key, myDiagram.Model.NodesSource)
            });
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
                if (link.Data is LinkModel linkModel && linkModel.Points != null && linkModel.Points.Count() > 1)
                    link.Route.Points = (IList<Point>)linkModel.Points;
            }

            myDiagram.PartManager.UpdatesRouteDataPoints =
                true; // OK for CustomPartManager to update LinkModel.Points automatically
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {

            if ((Keyboard.Modifiers == ModifierKeys.Control && (e.Key == Key.Z || e.Key == Key.Y)) || e.Key == Key.Delete)
            {
                var model = myDiagram.Model as GraphLinksModel<NodeModel, string, string, LinkModel>;
                Task.Factory.StartNew(() => UpdateMatrix(model, myDiagram));
            }


        }

        private void ChangeOrientationClick(object sender, RoutedEventArgs e)
        {
            var linkModel = ((PartManager.PartBinding)(sender as MenuItem).DataContext).Part.Data as LinkModel;
            int from = getNodeIndex(linkModel.From, myDiagram.Model.NodesSource);
            int to = getNodeIndex(linkModel.To, myDiagram.Model.NodesSource);

            if (linkModel.IsOriented)
            {
                matrixData[from].Values[to] = linkModel;
                matrixData[to].Values[from] = linkModel;
            }
            else
            {
                matrixData[to].Values[from] = new LinkModel(linkModel.From, linkModel.To, "") { model = myDiagram.Model as GraphLinksModel<NodeModel, string, string, LinkModel>, DiagramModel = myDiagram };
            }

        }

        private void ReverseClick(object sender, RoutedEventArgs e)
        {
            var linkModel = ((PartManager.PartBinding)(sender as MenuItem).DataContext).Part.Data as LinkModel;
            if (linkModel.IsOriented)
            {
                int from = getNodeIndex(linkModel.From, myDiagram.Model.NodesSource);
                int to = getNodeIndex(linkModel.To, myDiagram.Model.NodesSource);
                matrixData[from].Values[to] = new LinkModel(linkModel.From, linkModel.To, "") { model = myDiagram.Model as GraphLinksModel<NodeModel, string, string, LinkModel>, DiagramModel = myDiagram };
                matrixData[to].Values[from] = linkModel;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var model = myDiagram.Model as GraphLinksModel<NodeModel, string, string, LinkModel>;
            Task.Factory.StartNew(() => UpdateMatrix(model, myDiagram));
        }
    }

    public class Line
    {
        public string Heading
        {
            get;
            set;
        }

        public ObservableCollection<LinkModel> Values
        {
            get;
            set;
        }
        public int Position { get; set; }
    }
}