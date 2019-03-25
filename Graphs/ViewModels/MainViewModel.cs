using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using Graphs.Models;
using Northwoods.GoXam.Model;


namespace Graphs.ViewModels
{
    class MainViewModel:ViewModelBase
    {
        public GraphLinksModel<NodeModel, String, String, LinkModel> Model { get; set; }


        public MainViewModel()
        {
            Model = new GraphLinksModel<NodeModel, String, String, LinkModel>();
            Model.Modifiable = true;  // let the user modify the graph
            Model.HasUndoManager = true;  // support undo/redo
        }
    }
}
