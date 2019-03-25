using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Graphs.Helpers;
using Microsoft.Win32;
using Northwoods.GoXam;
using Northwoods.GoXam.Model;
using Northwoods.GoXam.Tool;

namespace Graphs
{
    /// <summary>
    /// Interaction logic for StateChart.xaml
    /// </summary>

    public partial class StateChart : UserControl
    {


        private GraphLinksModel<NodeModel, String, String, LinkModel> model;

        public StateChart()
        {
            InitializeComponent();

            // because we cannot data-bind the Route.Points property,
            // we use a custom PartManager to read/write the LinkModel.Points data property
            myDiagram.PartManager = new CustomPartManager();

            // create the diagram's data model
            model = new GraphLinksModel<NodeModel, String, String, LinkModel>();
            

            // initialize it from data in an XML file that is an embedded resource
       //     String xml = Demo.MainPage.Instance.LoadText("StateChart", "xml");
            // set the Route.Points after nodes have been built and the layout has finished
            myDiagram.LayoutCompleted += UpdateRoutes;
     //       model.Load<NodeModel, LinkModel>(XElement.Parse(xml), "NodeModel", "LinkModel");
            model.Modifiable = true;  // let the user modify the graph
            model.HasUndoManager = true;  // support undo/redo
            myDiagram.Model = model;

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


            linkModel.Text = ((int)(GetNode(linkModel.From).Location - GetNode(linkModel.To).Location).Length/100).ToString();
        }

        private GraphLinksModelNodeData<string> GetNode(String key)
        {
            foreach (NodeModel node in model.NodesSource)
            {
                if (node.Key== key)
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
            myDiagram.Model.AddLink(from, "AAA", to, "BBB");

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
                model.Load<NodeModel, LinkModel>(root,"NodeModel", "LinkModel");
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
            openFileDialog.InitialDirectory = "c:\\";
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

        private void NodeMenuClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void LinkMenuClick(object sender, RoutedEventArgs e)
        {
            
        }
    }


    public class CustomPartManager : PartManager
    {
        public CustomPartManager()
        {
            this.UpdatesRouteDataPoints = true;  // call UpdateRouteDataPoints when Link.Route.Points has changed
        }

        // this supports undo/redo of link route reshaping
        protected override void UpdateRouteDataPoints(Link link)
        {
            if (!this.UpdatesRouteDataPoints) return;   // in coordination with Load_Click and UpdateRoutes, above
            var data = link.Data as LinkModel;
            if (data != null)
            {
                data.Points = new List<Point>(link.Route.Points);
            }
        }
    }


    // the data for each node; the predefined data class is enough

    [Serializable]
    public class NodeModel : GraphLinksModelNodeData<String>
    {
        private static Boolean isExampleCreated;

        public NodeModel()
        {
            
            if (isExampleCreated)
            {
                String key = NodeNameCreator.GetNodeName();
                this.Key = key;  // be sure to provide an initial non-null value for the Key
                this.Text = key;
            }
            else
                isExampleCreated = true;
        }

        public override void ChangeDataValue(ModelChangedEventArgs e, bool undo)
        {
            base.ChangeDataValue(e, undo);
        }

        // note that adding properties here means also overriding MakeXElement and LoadFromXElement
    }


    // the data for each link
#if !SILVERLIGHT
    [Serializable]
#endif
    public class LinkModel : GraphLinksModelLinkData<String, String>
    {
        public LinkModel()
        {         
            this.Text = "0";
        }

        // this property remembers the curviness;
        // Double.NaN means let it use a default calculated value
        public double Curviness
        {
            get { return _Curviness; }
            set
            {
                if (_Curviness != value)
                {
                    double old = _Curviness;
                    _Curviness = value;
                    RaisePropertyChanged("Curviness", old, value);
                }
            }
        }
        // default value of NaN causes Route to calculate it
        private double _Curviness = Double.NaN;

        public Point Offset
        {
            get { return _Offset; }
            set
            {
                if (_Offset != value)
                {
                    Point old = _Offset;
                    _Offset = value;
                    RaisePropertyChanged("Offset", old, value);
                }
            }
        }
        private Point _Offset = new Point(0, 0);

        // write the extra property on the link data
        public override XElement MakeXElement(XName n)
        {
            XElement e = base.MakeXElement(n);
            e.Add(XHelper.Attribute("Curviness", this.Curviness, Double.NaN));
            e.Add(XHelper.Attribute("Offset", this.Offset, new Point(0, 0)));
            return e;
        }

        // read the extra property on the link data
        public override void LoadFromXElement(XElement e)
        {
            base.LoadFromXElement(e);
            this.Curviness = XHelper.Read("Curviness", e, Double.NaN);
            this.Offset = XHelper.Read("Offset", e, new Point(0, 0));
        }
    }


    // This tool only works when a Link has a LinkPanel with a single child element
    // that is positioned at the Route.MidPoint plus some Offset.
    public class SimpleLabelDraggingTool : DiagramTool
    {
        public override bool CanStart()
        {
            if (!base.CanStart()) return false;
            Diagram diagram = this.Diagram;
            if (diagram == null) return false;
            // require left button & that it has moved far enough away from the mouse down point, so it isn't a click
            if (!IsLeftButtonDown()) return false;
            if (!IsBeyondDragSize()) return false;
            return FindLabel() != null;
        }

        private FrameworkElement FindLabel()
        {
            var elt = this.Diagram.Panel.FindElementAt<System.Windows.Media.Visual>(this.Diagram.LastMousePointInModel, e => e, null, SearchLayers.Links);
            if (elt == null) return null;
            Link link = Part.FindAncestor<Link>(elt);
            if (link == null) return null;
            var parent = System.Windows.Media.VisualTreeHelper.GetParent(elt) as System.Windows.Media.Visual;
            while (parent != null && parent != link && !(parent is LinkPanel))
            {
                elt = parent;
                parent = System.Windows.Media.VisualTreeHelper.GetParent(elt) as System.Windows.Media.Visual;
            }
            if (parent is LinkPanel)
            {
                FrameworkElement lab = elt as FrameworkElement;
                if (lab == null) return null;
                // needs to be positioned relative to the MidPoint
                if (LinkPanel.GetIndex(lab) != Int32.MinValue) return null;
                // also check for movable-ness?
                return lab;
            }
            return null;
        }

        public override void DoActivate()
        {
            StartTransaction("Shifted Label");
            this.Label = FindLabel();
            if (this.Label != null)
            {
                this.OriginalOffset = LinkPanel.GetOffset(this.Label);
            }
            base.DoActivate();
        }

        public override void DoDeactivate()
        {
            base.DoDeactivate();
            StopTransaction();
        }

        private FrameworkElement Label { get; set; }
        private Point OriginalOffset { get; set; }

        public override void DoStop()
        {
            this.Label = null;
            base.DoStop();
        }

        public override void DoCancel()
        {
            if (this.Label != null)
            {
                LinkPanel.SetOffset(this.Label, this.OriginalOffset);
            }
            base.DoCancel();
        }

        public override void DoMouseMove()
        {
            if (!this.Active) return;
            UpdateLinkPanelProperties();
        }

        public override void DoMouseUp()
        {
            if (!this.Active) return;
            UpdateLinkPanelProperties();
            this.TransactionResult = "Shifted Label";
            StopTool();
        }

        private void UpdateLinkPanelProperties()
        {
            if (this.Label == null) return;
            Link link = Part.FindAncestor<Link>(this.Label);
            if (link == null) return;
            Point last = this.Diagram.LastMousePointInModel;
            Point mid = link.Route.MidPoint;
            // need to rotate this point to account for angle of middle segment
            Point p = new Point(last.X - mid.X, last.Y - mid.Y);
            LinkPanel.SetOffset(this.Label, RotatePoint(p, -link.Route.MidAngle));
        }

        private static Point RotatePoint(Point p, double angle)
        {
            if (angle == 0 || (p.X == 0 && p.Y == 0))
                return p;
            double rad = angle * Math.PI / 180;
            double cosine = Math.Cos(rad);
            double sine = Math.Sin(rad);
            return new Point((cosine * p.X - sine * p.Y),
                             (sine * p.X + cosine * p.Y));
        }
    }
}

