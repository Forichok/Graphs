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
        public String Figure { get; set; } = "Cube";
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

}
