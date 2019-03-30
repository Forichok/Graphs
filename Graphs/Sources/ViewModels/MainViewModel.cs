using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using Graphs.Sources.Helpers;
using Graphs.Sources.Models;
using Graphs.Sources.Tasks;
using Graphs.Sources.Tools;
using Northwoods.GoXam;
using Northwoods.GoXam.Model;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;


namespace Graphs.Sources.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        public GraphLinksModel<NodeModel, string, string, LinkModel> Model { get; set; }
        public CustomPartManager PartManager { get; set; }

        public MainViewModel()
        {
            Model = new GraphLinksModel<NodeModel, string, string, LinkModel>()
            {
                Modifiable = true,
                HasUndoManager = true
            };
            PartManager = new CustomPartManager();


            LoadAdjencyMatrixCommand = new DelegateCommand(LoadAdjencyMatrix);
            SaveAdjencyMatrixCommand = new DelegateCommand(SaveAdjencyMatrix);

            LoadIncidenceMatrixCommand = new DelegateCommand(LoadIncidenceMatrix);
            SaveIncidenceMatrixCommand = new DelegateCommand(SaveIncidenceMatrix);

            LoadByEdgesCommand = new DelegateCommand(LoadByEdges);
            SaveByEdgesCommand = new DelegateCommand(SaveByEdges);

            ExitCommand = new DelegateCommand(Exit);


            ReverseMenuCommand = new DelegateCommand<object>(ReverseMenu);
            ChangeLinkDirectionMenuCommand = new DelegateCommand<object>(ChangeLinkDirectionMenu);

            ChangeFigureMenuCommand = new DelegateCommand<object>(ChangeFigureMenu);
            AddNewNodeCommand = new DelegateCommand<PartManager.PartBinding>(AddNewNode);

            SaveAsImageCommand = new DelegateCommand<Diagram>(SaveAsImage);
            LoadCommand = new DelegateCommand(LoadUniversal);
            SaveCommand = new DelegateCommand<Diagram>(SaveUniversal);

            BfsCommand = new DelegateCommand(StartBfs);
            BestfsCommand = new DelegateCommand(StartBestfs);

            DijkstraMatrixCommand = new DelegateCommand(StartDijkstraMatrix);
        }


        #region link context menu commands

        public DelegateCommand<object> ReverseMenuCommand { get; }

        public DelegateCommand<object> ChangeLinkDirectionMenuCommand { get; }

        #endregion


        #region Links context menu actions

        private void ReverseMenu(object sender)
        {
            var link = (sender as PartManager.PartBinding).Data as LinkModel;
            var tmpStr = link.From;
            link.From = link.To;
            link.To = tmpStr;
        }

        private void ChangeLinkDirectionMenu(object sender)
        {
            var link = (sender as PartManager.PartBinding).Data as LinkModel;
            link.IsOriented = !link.IsOriented;
            Model.RemoveLink(link);
            Model.AddLink(link); //?? better way to update??
        }

        #endregion


        #region node context menu commands

        public DelegateCommand<object> ChangeFigureMenuCommand { get; }

        public DelegateCommand<PartManager.PartBinding> AddNewNodeCommand { get; }

        #endregion


        #region node context menu commands

        private void ChangeFigureMenu(object sender)
        {
            var b = (sender as PartManager.PartBinding).Data as NodeModel;
            b.ChangeFigure();
        }

        private void AddNewNode(PartManager.PartBinding sender)
        {
            var myDiagram = sender.Part.Diagram;
            var ad = Part.FindAncestor<Adornment>(sender.Part.SelectionElement); //e.OriginalSource as UIElement
            if (ad == null) return;
            // its AdornedPart should be a Node that is bound to a NodeModel object
            var from = ad.AdornedPart.Data as NodeModel;
            if (from == null) return;
            // make all changes to the model within a transaction
            myDiagram.StartTransaction("Add NodeModel");
            // create a new NodeModel, add it to the model, and create a link from
            // the selected node data to the new node data

            var list = MainModel.NodesModelToArr(myDiagram.Model.NodesSource.Cast<NodeModel>());
            var key = NodeKeyCreator.GetNodeName(list);
            var to = new NodeModel(key, key);
            //  to.Text = "new";
            var p = from.Location;
            //?? this isn't a very smart way to decide where to place the node
            to.Location = new Point(p.X + 200, p.Y);
            myDiagram.Model.AddNode(to);
            var newnode = myDiagram.PartManager.FindNodeForData(to, myDiagram.Model);
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

        #endregion


        #region Menu Commands 

        public DelegateCommand LoadAdjencyMatrixCommand { get; }
        public DelegateCommand SaveAdjencyMatrixCommand { get; }

        public DelegateCommand LoadIncidenceMatrixCommand { get; }
        public DelegateCommand SaveIncidenceMatrixCommand { get; }

        public DelegateCommand LoadByEdgesCommand { get; }
        public DelegateCommand SaveByEdgesCommand { get; }

        public DelegateCommand<Diagram> SaveAsImageCommand { get; }


        public DelegateCommand LoadCommand { get; }

        public DelegateCommand<Diagram> SaveCommand { get; }



        public DelegateCommand ExitCommand { get; }

        #endregion


        #region Menu Actions

        private void LoadAdjencyMatrix()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Adjency matrix (*.adm)|*.adm";

            if (openFileDialog.ShowDialog() != true) return;

            try
            {
                var filePath = openFileDialog.FileName;
                var result = MainModel.LoadAdjencyMatrix(filePath);
                UpdateMatrix(result.Key, result.Value);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveAdjencyMatrix()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Adjency matrix (*.adm)|*.adm";
            if (saveFileDialog.ShowDialog() == true)
                try
                {
                    MainModel.SaveAdjencyMatrix(saveFileDialog.FileName, Model);
                    Model.IsModified = false;

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                }

        }

        private void LoadIncidenceMatrix()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Incidence matrix (*.inm)|*.inm";

            if (openFileDialog.ShowDialog() != true) return;

            try
            {
                var filePath = openFileDialog.FileName;
                var result = MainModel.LoadIncidenceMatrix(filePath);
                UpdateMatrix(result.Key, result.Value);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveIncidenceMatrix()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Incidence matrix (*.inm)|*.inm";
            if (saveFileDialog.ShowDialog() == true)
                try
                {
                    MainModel.SaveIncidenceMatrix(saveFileDialog.FileName, Model);
                    Model.IsModified = false;

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                }

        }

        private void LoadByEdges()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Edges input (*.edg)|*.edg";

            if (openFileDialog.ShowDialog() != true) return;

            try
            {
                var filePath = openFileDialog.FileName;
                var result = MainModel.LoadByEdges(filePath);
                UpdateMatrix(result.Key, result.Value);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveByEdges()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Edges input (*.edg)|*.edg";
            if (saveFileDialog.ShowDialog() == true)
                try
                {
                    MainModel.SaveByEdges(saveFileDialog.FileName, Model);
                    Model.IsModified = false;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                }

        }


        private void SaveAsImage(Diagram diagram)
        {
            var b = diagram.Panel.DiagramBounds;

            var l_dialog = new SaveFileDialog();


            var dialogResult = l_dialog.ShowDialog();

            if (dialogResult == true)
            {
                var image = diagram.Panel.MakeBitmap(new Size(b.Width, b.Height), 96, new Point(b.X, b.Y), 1,
                    bmp =>
                    {
                        var pos = diagram.Panel.Position;
                        diagram.Panel.Position = new Point(pos.X, pos.Y + 1);
                        diagram.Panel.Position = pos;

                        var stream = new FileStream(l_dialog.FileName + ".png", FileMode.Create);
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bmp));
                        encoder.Save(stream);
                        stream.Close();
                    });
            }
        }

        private void LoadUniversal()
        {
            if (Model == null) return;
            try
            {
                var root = XElement.Parse(LoadFromFile());
                // set the Route.Points after nodes have been built and the layout has finished
                //myDiagram.LayoutCompleted += UpdateRoutes;
                // tell the CustomPartManager that we're loading
                //PartManager.UpdatesRouteDataPoints = false;
                Model.Load<NodeModel, LinkModel>(root, "NodeModel", "LinkModel");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            Model.IsModified = false;
        }

        private void SaveUniversal(Diagram myDiagram)
        {
            if (Model == null) return;
            // copy the Route.Points into each LinkModel data
            foreach (var link in myDiagram.Links)
            {
                var linkModel = link.Data as LinkModel;
                if (linkModel != null)
                {
                    linkModel.Points = new List<Point>(link.Route.Points);
                }
            }
            var root = Model.Save<NodeModel, LinkModel>("StateChart", "NodeModel", "LinkModel");
            var saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "Xml files (*.xml)|*.xml";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == true)
            {
                using (var sw = new StreamWriter(saveFileDialog1.FileName))
                {
                    sw.Write(root.ToString());
                }
            }

            Model.IsModified = false;
        }

        private void Exit()
        {
            if (Model.IsModified)
            {
                var result = MessageBox.Show("You have unsaved changes, do you want to save it in xml?",
                    "Are you sure?", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:

                        break;
                }

            }
            if (Application.Current.MainWindow != null) Application.Current.MainWindow.Close();
        }

        #endregion


        #region task 2

        public DelegateCommand BfsCommand { get; }

        private void StartBfs()
        {
            ClearGraph();
            var mappedList = MainModel.CreateMapedList(Model.NodesSource.Cast<NodeModel>(),
                Model.LinksSource.Cast<LinkModel>());

            var resBFS = BFSTask2.BreadthFirstSearch(mappedList, "A", "B");
            var cost = 0;
            resBFS.Key.ForEach(t =>
            {
                t.IsSelected = true;
                var parseResult = int.TryParse(t.Text, out var res);

                if (parseResult)
                    cost += res;
            });
            ShowWaySearchResult(cost, resBFS.Value);
        }

        #endregion


        #region task 3

        public DelegateCommand BestfsCommand { get; }

        private void StartBestfs()
        {
            ClearGraph();
            var checkRes = CheckGraphsLinksWithMsg();
            if (checkRes == false)
                return;
            var mappedList = MainModel.CreateMapedList(Model.NodesSource.Cast<NodeModel>(),
                Model.LinksSource.Cast<LinkModel>());

            var resBestFS = BestFSTask3.StartBestFs(mappedList, "A", "B");
            var cost = 0;
            resBestFS.Key.ForEach(t =>
            {
                t.IsSelected = true;
                var parseResult = int.TryParse(t.Text, out var res);

                if (parseResult)
                    cost += res;
            });
            ShowWaySearchResult(cost, resBestFS.Value);
        }

        #endregion


        #region task 4

        public DelegateCommand DijkstraMatrixCommand { get; }

        public void StartDijkstraMatrix()
        {

            ClearGraph();
            var checkRes = CheckGraphsLinksWithMsg(true);
            if (checkRes == false)
                return;
            var mappedList = MainModel.CreateMapedList(Model.NodesSource.Cast<NodeModel>(),
                Model.LinksSource.Cast<LinkModel>());

            var resDijkstra = DijkstraTask4.StartDijkstra(mappedList, "A");

            var resWindow = new DijkstraResultWindow((Dictionary<string, UniversalGraphNodeData>) resDijkstra, "A");
            resWindow.Show();

        }

        #endregion



        private void ClearGraph()
        {
            foreach (var o in Model.LinksSource)
            {
                ((LinkModel) o).IsSelected = false;
            }
        }

        private bool CheckGraphsLinksWithMsg(bool onlyPlus = false)
        {
            foreach (var o in Model.LinksSource)
            {
                var parseResult = int.TryParse(((LinkModel) o).Text, out var ignored);
                if (parseResult == false)
                {
                    MessageBox.Show($"Cannot start func because one of link has wrong cost [{(LinkModel) o}]", "Alert",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (onlyPlus && ignored < 0)
                    MessageBox.Show(
                        $"Cannot start func because one of link has wrong cost [{(LinkModel) o}] required > 0", "Alert",
                        MessageBoxButton.OK, MessageBoxImage.Error);

            }
            return true;
        }

        private void UpdateMatrix(IEnumerable<NodeModel> nodes, IEnumerable<LinkModel> links)
        {
            Model.NodesSource = new ObservableCollection<NodeModel>(nodes);
            Model.LinksSource = new ObservableCollection<LinkModel>(links);
        }

        private string LoadFromFile()
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            var openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.Filter = "Xml files (*.xml)|*.xml";
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

        private void ShowWaySearchResult(int cost, string vector)
        {
            var result = MessageBox.Show($"Path cost: {cost}, Do you want to save vector?",
                "Search result", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    SaveVector(vector);        
                    break;
            }
        }

        private void SaveVector(string vector)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Simple text (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var sw = new StreamWriter(saveFileDialog.FileName))
                    {
                        sw.WriteLine(vector);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
              
}
