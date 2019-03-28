using System;
using Graphs.Sources.Helpers;
using Northwoods.GoXam.Model;

namespace Graphs.Sources.Models
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
            //string key = NodeNameCreator.GetNodeName();
            this.Key = "0";  // be sure to provide an initial non-null value for the Key
            this.Text = "test";
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
