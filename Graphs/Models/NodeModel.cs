using System;
using Graphs.Helpers;
using Northwoods.GoXam.Model;

namespace Graphs.Models
{
    // the data for each node; the predefined data class is enough

    [Serializable]
    public class NodeModel : GraphLinksModelNodeData<string>
    {

        private int _figureId;

        public bool IsSelected { get; set; }
        public string Figure { get; set; } 
       

        public NodeModel()
        {
            _figureId = 0;
            Figure = NodeFigureCreator.GetFigure(ref _figureId);
            IsSelected = false;
            string key = NodeNameCreator.GetNodeName();
            this.Key = key;  // be sure to provide an initial non-null value for the Key
            this.Text = key;
        }

        public NodeModel(string key, string text)
        {
            this.Key = key;  
            this.Text = text;
            _figureId = 0;
            Figure = NodeFigureCreator.GetFigure(ref _figureId);
            IsSelected = false;
        }

        public void ChangeFigure()
        {
            Figure = NodeFigureCreator.GetFigure(ref _figureId);
        }


        //TODO new read and write overwrite 
    }

}
