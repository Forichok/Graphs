﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using DevExpress.Mvvm;
using Graphs.Models;
using Graphs.Tools;
using Microsoft.Win32;
using Northwoods.GoXam;
using Northwoods.GoXam.Model;


namespace Graphs.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        public GraphLinksModel<NodeModel, String, String, LinkModel> Model { get; set; }
        public CustomPartManager PartManager { get; set; }

        public MainViewModel()
        {
            Model = new GraphLinksModel<NodeModel, String, String, LinkModel>();
            PartManager = new CustomPartManager();
            Model.Modifiable = true; // let the user modify the graph
            Model.HasUndoManager = true; // support undo/redo
        }

        public ICommand ReverseMenuCommand
        {
            get
            {
                return new DelegateCommand<Object>((sender) =>
                {
                    var link = (sender as PartManager.PartBinding).Data as LinkModel;
                    var tmpStr = link.From;
                    link.From = link.To;
                    link.To = tmpStr;
                });
            }
        }

        public ICommand ChangeFigureMenuCommand
        {
            get
            {
                return new DelegateCommand<Object>((sender) =>
                {
                    var b = (sender as PartManager.PartBinding).Data as NodeModel;
                    b.ChangeFigure();
                });
            }
        }

        public ICommand ChangeLinkDirectionMenuCommand
        {
            get
            {
                return new DelegateCommand<Object>((sender) =>
                {
                    var link = (sender as PartManager.PartBinding).Data as LinkModel;
                    link.IsOriented = !link.IsOriented;
                    Model.RemoveLink(link);
                    Model.AddLink(link); //?? better way to update??
                });
            }
        }

        public ICommand AddNewNodeCommand
        {
            get
            {
                return new DelegateCommand<PartManager.PartBinding>((sender) =>
                {
                    Diagram myDiagram = sender.Part.Diagram;
                    Adornment ad = Part.FindAncestor<Adornment>(sender.Part.SelectionElement); //e.OriginalSource as UIElement
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
                });
            }
        }

        public ICommand SaveAsImageCommand
        {
            get
            {
                return new DelegateCommand<Diagram>((diagram) =>
                {
                    Rect b = diagram.Panel.DiagramBounds;

                    SaveFileDialog l_dialog = new SaveFileDialog();


                    bool? dialogResult = l_dialog.ShowDialog();

                    if (dialogResult == true)
                    {
                        var image = diagram.Panel.MakeBitmap(new Size(b.Width, b.Height), 96, new Point(b.X, b.Y), 1,
                            bmp =>
                            {
                                var pos = diagram.Panel.Position;
                                diagram.Panel.Position = new Point(pos.X, pos.Y + 1);
                                diagram.Panel.Position = pos;

                                FileStream stream = new FileStream(l_dialog.FileName, FileMode.Create);
                                PngBitmapEncoder encoder = new PngBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(bmp));
                                encoder.Save(stream);
                                stream.Close();
                            });
                    }

                });
            }
        }

        public ICommand LoadCommand
        {
            get
            {
                return new DelegateCommand<Object>((sender) =>
                {
                    if (Model == null) return;
                    try
                    {
                        XElement root = XElement.Parse(LoadFromFile());
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

                });
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return new DelegateCommand<Diagram>((myDiagram) =>
                {
                    if (Model == null) return;
                    // copy the Route.Points into each LinkModel data
                    foreach (Link link in myDiagram.Links)
                    {
                        LinkModel linkModel = link.Data as LinkModel;
                        if (linkModel != null)
                        {
                            linkModel.Points = new List<Point>(link.Route.Points);
                        }
                    }
                    XElement root = Model.Save<NodeModel, LinkModel>("StateChart", "NodeModel", "LinkModel");
                    SaveFileDialog saveFileDialog1 = new SaveFileDialog();

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

                });
            }
        }

        private String LoadFromFile()
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

                using (StreamReader reader = new StreamReader(fileStream))
                {
                    fileContent = reader.ReadToEnd();
                }
            }

            return fileContent;
        }
    }
}
