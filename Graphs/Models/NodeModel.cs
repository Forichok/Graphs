using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphs.Helpers;
using Northwoods.GoXam.Model;

namespace Graphs.Models
{
    // the data for each node; the predefined data class is enough

    [Serializable]
    public class NodeModel : GraphLinksModelNodeData<String>
    {
        private static Boolean isExampleCreated;

        public Boolean IsSelected { get; set; }

        private int _figureId;
        public String Figure { get; set; } 
       
        public NodeModel()
        {
            _figureId = 0;
            Figure = NodeFigureCreator.GetFigure(ref _figureId);
            IsSelected = false;
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

        public void ChangeFigure()
        {
            Figure = NodeFigureCreator.GetFigure(ref _figureId);
        }

        // note that adding properties here means also overriding MakeXElement and LoadFromXElement
    }

}
