﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using Graphs.Sources.Advanced.Algorithms.DataStructures.Graph.AdjacencyList;
using Graphs.Sources.Helpers;
using Graphs.Sources.Models;
using Graphs.Sources.Tasks;
using Graphs.Sources.Tasks.Task14;
using Graphs.Sources.Tasks.Task15;
using Graphs.Sources.Tasks.Task6;
using Graphs.Sources.Tasks.Task8;
using Graphs.Sources.Tools;
using Graphs.Views;
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

        private GraphLinksModel<NodeModel, string, string, LinkModel> model2;
        public NodeModel StartNode { get; set; }

        public bool MenuIsActive { get; set; } = true;

        public NodeModel FinishNode { get; set; }
        public CustomPartManager PartManager { get; set; }

        public event EventHandler FileLoaded;

        public MainViewModel()
        {
            Model = new GraphLinksModel<NodeModel, string, string, LinkModel>()
            {
                Modifiable = true,
                HasUndoManager = true
            };
            model2 = new GraphLinksModel<NodeModel, string, string, LinkModel>()
            {
                Modifiable = true,
                HasUndoManager = true
            };
            PartManager = new CustomPartManager();

            SwitchGraphCommand = new DelegateCommand(SwitchGraph);

            LoadAdjencyMatrixCommand = new DelegateCommand(LoadAdjencyMatrix);
            SaveAdjencyMatrixCommand = new DelegateCommand(SaveAdjencyMatrix);

            LoadIncidenceMatrixCommand = new DelegateCommand(LoadIncidenceMatrix);
            SaveIncidenceMatrixCommand = new DelegateCommand(SaveIncidenceMatrix);

            LoadByEdgesCommand = new DelegateCommand(LoadByEdges);
            SaveByEdgesCommand = new DelegateCommand(SaveByEdges);

            ExitCommand = new DelegateCommand(Exit);

            ResetGraphCommand = new DelegateCommand(ResetGraph);

            ReverseMenuCommand = new DelegateCommand<object>(ReverseMenu);
            ChangeLinkDirectionMenuCommand = new DelegateCommand<object>(ChangeLinkDirectionMenu);

            ChangeFigureMenuCommand = new DelegateCommand<object>(ChangeFigureMenu);
            AddNewNodeCommand = new DelegateCommand<PartManager.PartBinding>(AddNewNode);

            SetFinishNodeCommand = new DelegateCommand<object>(SetFinishNodeMenu);
            SetStartNodeCommand = new DelegateCommand<object>(SetStartNodeMenu);

            SaveAsImageCommand = new DelegateCommand<Diagram>(SaveAsImage);
            LoadCommand = new DelegateCommand<Diagram>(LoadUniversal);
            SaveCommand = new DelegateCommand<Diagram>(SaveUniversal);

            BfsCommand = new DelegateCommand(TaskStarter(StartBfs));
            BestfsCommand = new DelegateCommand(TaskStarter(StartBestfs));
            IsomorphismCommand = new DelegateCommand(TaskStarter(StartIsomorphism));
            ConnectivityCommand = new DelegateCommand(TaskStarter(StartConnectivity));
            ColorerCommand = new DelegateCommand(TaskStarter(StartColorer));

            DijkstraMatrixCommand = new DelegateCommand(TaskStarter(StartDijkstraMatrix));

            AStarCommand = new DelegateCommand(TaskStarter(StartAStar));
            Task6Command = new DelegateCommand(TaskStarter(StartTask6));

            CheckToFullCommand = new DelegateCommand(TaskStarter(StartCheckToFull));
            CreateFullCommand = new DelegateCommand(TaskStarter(StartCreateFull));

            KruskalCommand = new DelegateCommand(TaskStarter(StartKruskal));

            HelpCommand = new DelegateCommand(Help);
            AboutCommand = new DelegateCommand(About);

            CheckCycleCommand = new DelegateCommand(TaskStarter(CheckCycle));
        }


        #region link context menu commands

        public DelegateCommand<object> ReverseMenuCommand { get; }

        public DelegateCommand<object> ChangeLinkDirectionMenuCommand { get; }

        #endregion


        #region Links context menu actions

        private void ReverseMenu(object sender)
        {
            try
            {
                var link = (sender as PartManager.PartBinding).Data as LinkModel;
                var tmpStr = link.From;
                link.From = link.To;
                link.To = tmpStr;
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private void ChangeLinkDirectionMenu(object sender)
        {
            try
            {
                var link = (sender as PartManager.PartBinding).Data as LinkModel;
                link.IsOriented = !link.IsOriented;
                Model.RemoveLink(link);
                Model.AddLink(link); //?? better way to update??
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
        

        #region node context menu commands

        public DelegateCommand<object> ChangeFigureMenuCommand { get; }

        public DelegateCommand<object> SetStartNodeCommand { get; }

        public DelegateCommand<object> SetFinishNodeCommand { get; }

        public DelegateCommand<PartManager.PartBinding> AddNewNodeCommand { get; }

        #endregion


        #region node context menu commands

        private void ChangeFigureMenu(object sender)
        {
            try
            {
                var b = (sender as PartManager.PartBinding).Data as NodeModel;
                b.ChangeFigure();
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetStartNodeMenu(object sender)
        {
            try
            {
                if (StartNode != null)
                {
                    StartNode.IsFinishNode = false;
                    StartNode.IsStartNode = false;
                    StartNode.IsSelected = false;
                }

                StartNode = (sender as PartManager.PartBinding).Data as NodeModel;

                StartNode.IsStartNode = true;
                StartNode.IsFinishNode = false;
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetFinishNodeMenu(object sender)
        {
            try
            {
                if (FinishNode != null)
                {
                    FinishNode.IsFinishNode = false;
                    FinishNode.IsStartNode = false;
                    FinishNode.IsSelected = false;
                }

                FinishNode = (sender as PartManager.PartBinding).Data as NodeModel;

                FinishNode.IsFinishNode = true;
                FinishNode.IsStartNode = false;
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddNewNode(PartManager.PartBinding sender)
        {
            try
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
                var key = NodeKeyCreator.GetNodeName(list,Model.NodesSource.Cast<NodeModel>().Select(i=>i.Key));
                var to = new NodeModel(key);
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
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        #region background commands

        public DelegateCommand ResetGraphCommand { get; }

        private void ResetGraph()
        {
            try
            {
                foreach (LinkModel link in Model.LinksSource)
                {
                    link.IsSelected = false;
                }

                foreach (NodeModel node in Model.NodesSource)
                {
                    node.IsFinishNode = false;
                    node.IsStartNode = false;
                    node.IsSelected = false;
                    node.Color = null;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        public DelegateCommand<Diagram> LoadCommand { get; }

        public DelegateCommand SwitchGraphCommand { get; }

        public DelegateCommand<Diagram> SaveCommand { get; }

        public DelegateCommand ExitCommand { get; }

        public DelegateCommand HelpCommand { get; }

        public DelegateCommand AboutCommand { get; }


        #endregion


        #region Menu Actions

        private void SwitchGraph()
        {
            try
            {
                var tmp = Model;
                Model = model2;
                model2 = tmp;
                OnFileLoaded();
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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

            OnFileLoaded();
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

            OnFileLoaded();
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

            OnFileLoaded();
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
            try
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
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUniversal(Diagram diagram)
        {
            if (Model == null) return;
            try
            {

                // set the Route.Points after nodes have been built and the layout has finished
                // tell the CustomPartManager that we're loading

                var root = XElement.Parse(LoadFromFile());
                // set the Route.Points after nodes have been built and the layout has finished
                diagram.LayoutCompleted += UpdateRoutes;
                // tell the CustomPartManager that we're loading
                PartManager.UpdatesRouteDataPoints = false;
                Model.Load<NodeModel, LinkModel>(root, "NodeModel", "LinkModel");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            Model.IsModified = false;
            OnFileLoaded();
        }

        private void SaveUniversal(Diagram myDiagram)
        {
            try
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
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Exit()
        {
            try
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
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Help()
        {
            var help = new Help();

            help.Show();
        }

        private void About()
        {
            var help = new AboutWindow();

            help.Show();
        }

        #endregion


        #region task 2

        public DelegateCommand BfsCommand { get; }

        private void StartBfs()
        {
            try
            {
                if (StartNode == null || FinishNode == null)
                {
                    MessageBox.Show("Please select start and finish nodes.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                ClearGraph();
                var mappedList = MainModel.CreateMapedList(Model.NodesSource.Cast<NodeModel>(),
                    Model.LinksSource.Cast<LinkModel>());


                var resBFS = BFSTask2.BreadthFirstSearch(mappedList, StartNode.Key, FinishNode.Key);
                var cost = 0;
                resBFS.Key.ForEach(t =>
                {
                    var fromNode = Model.GetFromNodeForLink(t);
                    if (!fromNode.IsFinishNode && !fromNode.IsStartNode)
                        fromNode.IsSelected = true;
                    t.IsSelected = true;

                    var parseResult = int.TryParse(t.Text, out var res);

                    if (parseResult)
                        cost += res;
                });
                ShowWaySearchResult(cost, resBFS.Value);
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        #region task 3

        public DelegateCommand BestfsCommand { get; }

        private void StartBestfs()
        {
            try
            {
                if (StartNode == null || FinishNode == null)
                {
                    MessageBox.Show("Please select start and finish nodes.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                ClearGraph();

                var checkRes = CheckGraphsLinksWithMsg();
                if (checkRes == false)
                    return;
                var mappedList = MainModel.CreateMapedList(Model.NodesSource.Cast<NodeModel>(),
                    Model.LinksSource.Cast<LinkModel>());

                var resBestFS = BestFSTask3.StartBestFs(mappedList, StartNode.Key, FinishNode.Key);
                var cost = 0;
                resBestFS.Key.ForEach(t =>
                {
                    var fromNode = Model.GetFromNodeForLink(t);
                    if (!fromNode.IsFinishNode && !fromNode.IsStartNode)
                        fromNode.IsSelected = true;

                    t.IsSelected = true;
                    var parseResult = int.TryParse(t.Text, out var res);

                    if (parseResult)
                        cost += res;
                });
                ShowWaySearchResult(cost, resBestFS.Value);
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        #region task 4

        public DelegateCommand DijkstraMatrixCommand { get; }

        public void StartDijkstraMatrix()
        {
            try
            {
                if (StartNode == null)
                {
                    MessageBox.Show("Please select start node.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                ClearGraph();
                var checkRes = CheckGraphsLinksWithMsg(true);
                if (checkRes == false)
                    return;
                var mappedList = MainModel.CreateMapedList(Model.NodesSource.Cast<NodeModel>(),
                    Model.LinksSource.Cast<LinkModel>());

                var resDijkstra = DijkstraTask4.StartDijkstra(mappedList, StartNode.Key);

                var resWindow = new Views.DijkstraResultWindow((Dictionary<string, UniversalGraphNodeData>) resDijkstra,
                    StartNode.Key);
                resWindow.Show();
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        #endregion


        #region task 5

        public DelegateCommand AStarCommand { get; }

        public void StartAStar()
        {
            try
            {
                if (StartNode == null)
                {
                    MessageBox.Show("Please select start node.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                ClearGraph();
                var checkRes = CheckGraphsLinksWithMsg(true);
                if (checkRes == false)
                    return;
                var mappedList = MainModel.CreateMapedList(Model.NodesSource.Cast<NodeModel>(),
                    Model.LinksSource.Cast<LinkModel>());

                var resAStar = AStarTask5.StartAStar(mappedList, StartNode.Key, FinishNode.Key);
                var cost = 0;
                resAStar.Key.ForEach(t =>
                {
                    var fromNode = Model.GetFromNodeForLink(t);
                    if (!fromNode.IsFinishNode && !fromNode.IsStartNode)
                        fromNode.IsSelected = true;

                    t.IsSelected = true;
                    var parseResult = int.TryParse(t.Text, out var res);

                    if (parseResult)
                        cost += res;
                });
                ShowWaySearchResult(cost, resAStar.Value);
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        #region task 6

        public DelegateCommand Task6Command { get; }

        public void StartTask6()
        {
            try
            {
                ClearGraph();
                var checkRes = CheckGraphsLinksWithMsg(true);
                if (checkRes == false)
                    return;

                var mappedList = MainModel.CreateMapedList(Model.NodesSource.Cast<NodeModel>(),
                    Model.LinksSource.Cast<LinkModel>());

                var t6 = new Task6Logick();
                var resTask6 = t6.BeginTask6(mappedList);

                var resWindow = new Task6Window(resTask6);
                resWindow.Show();
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
        

        #region task 7 Isomorphism

        public DelegateCommand IsomorphismCommand { get; }

        public void StartIsomorphism()
        {
            try
            {
                var task7 = new IsomorphismTask7(Model, model2);
                if (task7.IsIsomorphy())
                {
                    MessageBox.Show("Graphs are isomorphic");
                }
                else
                {
                    MessageBox.Show("Graphs aren't isomorphic");
                }
               
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion


        #region Task 8 Connectivity 

        public DelegateCommand ConnectivityCommand { get; }

        private void StartConnectivity()
        {
            try
            {
                var task8 = new ConnectivityTask8(Model);
                var result = task8.CheckConnectivity();
                MessageBox.Show(result, "Done", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

        }

        #endregion


        #region Task 9
        public DelegateCommand CheckToFullCommand { get; }
        public DelegateCommand CreateFullCommand { get; }

        public void StartCheckToFull()
        {
            try
            {
                var mappedList = MainModel.CreateMapedList(Model.NodesSource.Cast<NodeModel>(),
                    Model.LinksSource.Cast<LinkModel>());

                var res = FullGraphTask9.Check(mappedList);
                if (res)
                {
                    MessageBox.Show("Graphs is full", "9(1) Result", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Graphs isn't full", "9(1) Result", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void StartCreateFull()
        {
            try
            {
                var mappedList = MainModel.CreateMapedList(Model.NodesSource.Cast<NodeModel>(),
                    Model.LinksSource.Cast<LinkModel>());



                var additionalLinks = FullGraphTask9.GetFull(mappedList);

                if (additionalLinks.Count == 0)
                {
                    MessageBox.Show("Graphs is already full", "9(1) Result", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                Model.StartTransaction("full");

                foreach (var link in additionalLinks)
                {
                    Model.AddLink(link);
                }
                Model.CommitTransaction("full");

                OnFileLoaded();
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion


        #region Task 13

        public DelegateCommand KruskalCommand { get; }

        private void StartKruskal()
        {
            var wightCheckRes = CheckGraphsLinksWithMsg();
            if (!wightCheckRes)
                return;

            var isOrientated = !CheckOnOrientated();
            if (isOrientated)
            {
                var result = MessageBox.Show("You have orienated graph, prgram can work with error. Do you want to continue?",
                    "Are you sure?", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        break;

                    case MessageBoxResult.No:
                        return;
                }
            }


            try
            {
                var graph = WeightedGraph<string, int>.GetGraph(Model.LinksSource.Cast<LinkModel>(), Model.NodesSource.Cast<NodeModel>());
                var algorithm = new KruskalTask13<string, int>();
                var resultKruskal = algorithm.StartKruskal(graph);

                model2 = new GraphLinksModel<NodeModel, string, string, LinkModel>()
                {
                    Modifiable = true,
                    HasUndoManager = true
                };

                foreach (var edge in resultKruskal)
                {
                    var fromNode = Model.FindNodeByKey(edge.Source);
                    var toNode = Model.FindNodeByKey(edge.Destination);

                    if (model2.FindNodeByKey(edge.Source) == null)
                        model2.AddNode((NodeModel)fromNode.Clone());

                    if (model2.FindNodeByKey(edge.Destination) == null)
                        model2.AddNode((NodeModel)toNode.Clone());

                    model2.AddLink(
                        new LinkModel(edge.Source, edge.Destination, edge.Weight.ToString()) { IsOriented = false });
                }

                SwitchGraph();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        #region Task 14 Cycle

        public DelegateCommand CheckCycleCommand { get; set; }

        private void CheckCycle()
        {
            var diGraph = DiGraph<string>.GetDiGraph(Model.NodesSource.Cast<NodeModel>(), Model.LinksSource.Cast<LinkModel>());

            var algorithm = new CycleDetector<string>();

            var HasCycle = algorithm.HasCycle(diGraph);

            MessageBox.Show(HasCycle.ToString());
        }

        #endregion


        #region Task 15 Colorer

        public DelegateCommand ColorerCommand { get; }

        public void StartColorer()
        {
            try
            {

                var task15 = new ColorerTask15(Model);
                var result = task15.Paint();

                if (result == null)
                {
                    MessageBox.Show($"Chromatic number: 1", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                    
                var parts = result.Partitions;
                
                MessageBox.Show($"Chromatic number: {parts.Keys.Count}", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
                foreach (var color in parts.Keys)
                {
                    foreach (var nodeKey in parts[color])
                    {
                        var node = GetNode(nodeKey);
                        node.Color = (SolidColorBrush) (new BrushConverter().ConvertFrom($"#{color}"));
                    }

                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        

        private NodeModel GetNode(string key)
        {
            try
            {
                foreach (NodeModel node in Model.NodesSource)
                    if (node.Key == key)
                        return node;

                return null;
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        #endregion



        private Action TaskStarter(Action task)
        {
            return () =>
            {
                Model.StartTransaction("Task");
                task.Invoke();
                Model.CommitTransaction("Task");
            };
        }
        private void ClearGraph()
        {
            try
            {
                foreach (LinkModel link in Model.LinksSource)
                {
                    link.IsSelected = false;
                }

                foreach (NodeModel node in Model.NodesSource)
                {
                    node.IsSelected = false;
                    node.Color = null;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CheckGraphsLinksWithMsg(bool onlyPlus = false)
        {
            try
            {
                foreach (var o in Model.LinksSource)
                {
                    var parseResult = int.TryParse(((LinkModel) o).Text, out var ignored);
                    if (parseResult == false)
                    {
                        MessageBox.Show($"Cannot start func because one of link has wrong cost [{(LinkModel) o}]",
                            "Alert",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                    if (onlyPlus && ignored < 0)
                    {
                        MessageBox.Show(
                            $"Cannot start func because one of link has wrong cost [{(LinkModel) o}] required > 0",
                            "Alert",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }


                }

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void UpdateMatrix(IEnumerable<NodeModel> nodes, IEnumerable<LinkModel> links)
        {
            Model.NodesSource = new ObservableCollection<NodeModel>(nodes);
            Model.LinksSource = new ObservableCollection<LinkModel>(links);
        }

        private string LoadFromFile()
        {
            try
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
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return "";
        }

        private void ShowWaySearchResult(int cost, string vector)
        {
            try
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
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void SaveVector(string vector)
        {
            try
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
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void SaveVectors(IEnumerable<string> vectors)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Simple text (*.txt)|*.txt";
                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        using (var sw = new StreamWriter(saveFileDialog.FileName))
                        {
                            foreach (var vector in vectors)
                            {
                                sw.WriteLine(vector);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool CheckOnOrientated()
        {
            try
            {
                foreach (LinkModel link in Model.LinksSource)
                {
                    if (link.IsOriented)
                        return false;
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        protected virtual void OnFileLoaded()
        {
            try
            {
                RaisePropertyChanged("Model");
                FileLoaded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateRoutes(object sender, DiagramEventArgs e)
        {
            try
            {
                var myDiagram = sender as Diagram;
                // just set the Route points once per Load
                myDiagram.LayoutCompleted -= UpdateRoutes;
                foreach (Link link in myDiagram.Links)
                {
                    LinkModel transition = link.Data as LinkModel;
                    if (transition != null && transition.Points != null && transition.Points.Count() > 1)
                    {
                        link.Route.Points = (IList<Point>) transition.Points;
                    }
                }
                PartManager.UpdatesRouteDataPoints = true; // OK for CustomPartManager to update Transition.Points 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Oops.. something goes wrong...\n\n" + ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}