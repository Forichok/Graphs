using DevExpress.Mvvm.Native;
using Graphs.Sources.Helpers;
using Graphs.Sources.Models;
using Graphs.Sources.ViewModels;
using Northwoods.GoXam;
using Northwoods.GoXam.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;

namespace Graphs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Line> matrixData;
        
        private CancellationTokenSource source;
        private CancellationToken token;
        private Task updateTask;
        public MainWindow()
        {

            InitializeComponent();
            
            source = new CancellationTokenSource();

            token = source.Token;
            token.ThrowIfCancellationRequested();

            var model = myDiagram.Model as GraphLinksModel<NodeModel, string, string, LinkModel>;
            updateTask =new Task(() =>
            {
                UpdateMatrix(model,myDiagram);
            },token);
            

            matrixData = new ObservableCollection<Line>();

            myDiagram.LayoutCompleted += UpdateRoutes;

            (myDiagram.DataContext as MainViewModel).FileLoaded += MainWindow_FileLoaded;

            //var tool = new SimpleLabelDraggingTool();
            //tool.Diagram = myDiagram;

            //myDiagram.MouseMoveTools.Insert(0, tool);

            myDiagram.NodeCreated += MyDiagram_NodeCreated;

            myDiagram.LinkDrawn += MyDiagram_LinkDrawn;

            //myDiagram.LinkReshaped += MyDiagram_LinkDrawn;

            //myDiagram.LinkRelinked += MyDiagram_LinkDrawn;

            MatrixControl.ItemsSource = matrixData;
        }

        private void MainWindow_FileLoaded(object sender, EventArgs e)
        {
            UpdateMatrix();
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
                        linkModel = new LinkModel(from, to, "") {model = model, DiagramModel = diagram};
                    }

                    routes.Add(linkModel);


                }

                lines.Add(new Line() {Heading = from, Values = routes});
            }
            App.Current.Dispatcher.Invoke(()=>MatrixControl.ItemsSource=matrixData = lines);
        }


        private void MyDiagram_LinkDrawn(object sender, DiagramEventArgs e)
        {
            try
            {

                var linkModel = e.Part.Data as LinkModel;
                linkModel.Weight=((int) (GetNode(linkModel.From).Location - GetNode(linkModel.To).Location).Length / 100).ToString();

                linkModel.IsOriented = (bool) IsOrientedCheckBox.IsChecked;

                int from = getNodeIndex(linkModel.From, myDiagram.Model.NodesSource);
                int to = getNodeIndex(linkModel.To, myDiagram.Model.NodesSource);

                matrixData[from].Values[to] = linkModel;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
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

        private NodeModel GetNode(string key)
        {
            foreach (NodeModel node in myDiagram.Model.NodesSource)
                if (node.Key == key)
                    return node;

            return null;
        }

        private void MyDiagram_NodeCreated(object sender, DiagramEventArgs e)
        {
            var nodeModel = e.Part.Data as NodeModel;
            var key = NodeKeyCreator.GetNodeName(myDiagram.Model.NodesSource.Cast<NodeModel>().Select(i => i.Key));


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
                line.Values.Add(new LinkModel(line.Heading, key, "")
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

        // only use the saved route points after the layout has completed,
        // because links will get the default routing
        private void UpdateRoutes(object sender, DiagramEventArgs e)
        {
            // just set the Route points once per Load
            myDiagram.LayoutCompleted -= UpdateRoutes;
            foreach (var link in myDiagram.Links)
            {
                if (link.Data is LinkModel linkModel && linkModel.Points != null && linkModel.Points.Count() > 1)
                    link.Route.Points = (IList<Point>) linkModel.Points;
            }

            myDiagram.PartManager.UpdatesRouteDataPoints =
                true; // OK for CustomPartManager to update LinkModel.Points automatically
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {

            if ((Keyboard.Modifiers == ModifierKeys.Control && (e.Key == Key.Z || e.Key == Key.Y)) ||
                e.Key == Key.Delete)
            {
                UpdateMatrix();
            }


        }

        private void ChangeOrientationClick(object sender, RoutedEventArgs e)
        {
            var linkModel = ((PartManager.PartBinding) (sender as MenuItem).DataContext).Part.Data as LinkModel;
            int from = getNodeIndex(linkModel.From, myDiagram.Model.NodesSource);
            int to = getNodeIndex(linkModel.To, myDiagram.Model.NodesSource);

            if (linkModel.IsOriented)
            {
                matrixData[from].Values[to] = linkModel;
                matrixData[to].Values[from] = linkModel;
            }
            else
            {
                matrixData[to].Values[from] = new LinkModel(linkModel.From, linkModel.To, "")
                {
                    model = myDiagram.Model as GraphLinksModel<NodeModel, string, string, LinkModel>,
                    DiagramModel = myDiagram
                };
            }

        }

        private void ReverseClick(object sender, RoutedEventArgs e)
        {
            var linkModel = ((PartManager.PartBinding) (sender as MenuItem).DataContext).Part.Data as LinkModel;
            if (linkModel.IsOriented)
            {
                int from = getNodeIndex(linkModel.From, myDiagram.Model.NodesSource);
                int to = getNodeIndex(linkModel.To, myDiagram.Model.NodesSource);
                matrixData[from].Values[to] = new LinkModel(linkModel.From, linkModel.To, "")
                {
                    model = myDiagram.Model as GraphLinksModel<NodeModel, string, string, LinkModel>,
                    DiagramModel = myDiagram
                };
                matrixData[to].Values[from] = linkModel;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            UpdateMatrix();
        }

        private void DeleteNodeInMatrix_Click(object sender, RoutedEventArgs e)
        {
            var line = ((sender as MenuItem).DataContext) as Line;

            myDiagram.StartTransaction("Remove Node");

            myDiagram.Model.RemoveNode(GetNode(line.Heading));

            myDiagram.CommitTransaction("Remove Node");
            UpdateMatrix();
        }

        private void UpdateMatrix()
        {
            if (updateTask.Status != TaskStatus.Running) { 
            var model = myDiagram.Model as GraphLinksModel<NodeModel, string, string, LinkModel>;

          
            updateTask = new Task(() =>
            {
                UpdateMatrix(model, myDiagram);
            }, token);
            updateTask.Start();
                }
        }
    }

    public class Line
    {
        public string Heading { get; set; }

        public ObservableCollection<LinkModel> Values { get; set; }
        public int Position { get; set; }
    }
}