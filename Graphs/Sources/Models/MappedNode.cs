using System.Collections.Generic;

namespace Graphs.Sources.Models
{
    public class MappedNode
    {
        public NodeModel Node { get; set; }

        public List<LinkModel> Links { get; } = new List<LinkModel>();
    }
}
