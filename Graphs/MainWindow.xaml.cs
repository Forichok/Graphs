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

            // because we cannot data-bind the Route.Points property,
            // we use a custom PartManager to read/write the LinkModel.Points data property
            myDiagram.PartManager = new CustomPartManager();

            // create the diagram's data model
            


            // initialize it from data in an XML file that is an embedded resource
            //     String xml = Demo.MainPage.Instance.LoadText("StateChart", "xml");
            // set the Route.Points after nodes have been built and the layout has finished
            myDiagram.LayoutCompleted += UpdateRoutes;
            //       model.Load<NodeModel, LinkModel>(XElement.Parse(xml), "NodeModel", "LinkModel");

            

            // add a tool that lets the user shift the position of the link labels
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

        // This event handler is called from the Button that is in the Adornment
        // shown for a node when it is selected
        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //  find the Adornment "parent" for the Button

            Adornment ad = Part.FindAncestor<Adornment>(e.OriginalSource as UIElement);
            if (ad == null) return;
            // its AdornedPart should be a Node that is bound to a NodeModel object
            NodeModel from = ad.AdornedPart.Data as NodeModel;
            if (from == null) return;
            // make all changes to the model within a transaction
            myDiagram.StartTransaction("Add NodeModel");
            // create a new NodeModel, add it to the model, and create a link from
            // the selected node data to the new node data
            NodeModel to = new NodeModel();
            //  to.Text = "new";
            Point p = from.Location;
            //?? this isn't a very smart way to decide where to place the node
            to.Location = new Point(p.X + 200, p.Y);
            myDiagram.Model.AddNode(to);
            Node newnode = myDiagram.PartManager.FindNodeForData(to, myDiagram.Model);
            myDiagram.Select(newnode);
            EventHandler<DiagramEventArgs> show = null;
            show = (snd, evt) =>
            {
                myDiagram.Panel.MakeVisible(newnode, Rect.Empty);
                myDiagram.LayoutCompleted -= show;
            };
            myDiagram.LayoutCompleted += show;
            myDiagram.Model.AddLink(from, null, to, null);

            myDiagram.CommitTransaction("Add NodeModel");
        }

        // save and load the model data as XML, visible in the "Saved" tab of the Demo
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var model = myDiagram.Model as GraphLinksModel<NodeModel, String, String, LinkModel>;
            if (model == null) return;
            // copy the Route.Points into each LinkModel data
            foreach (Link link in myDiagram.Links)
            {
                LinkModel linkModel = link.Data as LinkModel;
                if (linkModel != null)
                {
                    linkModel.Points = new List<Point>(link.Route.Points);
                }
            }
            XElement root = model.Save<NodeModel, LinkModel>("StateChart", "NodeModel", "LinkModel");
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "All files (*.*)|*.*|xml files (*.xml)|*.xml";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == true)
            {
                using (var sw = new StreamWriter(saveFileDialog1.FileName))
                {
                    sw.Write(root.ToString());
                }
            }
            LoadButton.IsEnabled = true;
            model.IsModified = false;
        }

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

        private void ChangeFigureClick(object sender, RoutedEventArgs e)
        {
            var a = sender as System.Windows.Controls.MenuItem;
            var b = (a.DataContext as PartManager.PartBinding).Data as NodeModel;
            b.ChangeFigure();
            

        }

        private void ReverseMenuClick(object sender, RoutedEventArgs e)
        {
            var a = sender as System.Windows.Controls.MenuItem;
            var link = (a.DataContext as PartManager.PartBinding).Data as LinkModel;
            var tmpStr = link.From;
            link.From = link.To;
            link.To = tmpStr;
        }

        private void ChangeLinkTypeMenuClick(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as System.Windows.Controls.MenuItem;
            var link = (menuItem.DataContext as PartManager.PartBinding).Data as LinkModel;
            link.IsOriented = !link.IsOriented;
            GraphLinksModel<NodeModel, String, String, LinkModel> model = (GraphLinksModel<NodeModel, string, string, LinkModel>) myDiagram.Model;
            model.RemoveLink(link);
            model.AddLink(link); //?? better way to update??
        }

        private void SaveAsImageClick(object sender, RoutedEventArgs e)
        {
            Rect b = myDiagram.Panel.DiagramBounds;

            SaveFileDialog l_dialog = new SaveFileDialog();


            bool? dialogResult = l_dialog.ShowDialog();

            if (dialogResult == true)
            {
                var image = myDiagram.Panel.MakeBitmap(new Size(b.Width, b.Height), 96, new Point(b.X, b.Y), 1, bmp =>
                {
                    var pos = myDiagram.Panel.Position;
                    myDiagram.Panel.Position = new Point(pos.X, pos.Y + 1);
                    myDiagram.Panel.Position = pos;

                    FileStream stream = new FileStream(l_dialog.FileName, FileMode.Create);
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bmp));
                    encoder.Save(stream);
                    stream.Close();
                });
            }
        }
    }
}
