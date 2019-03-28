using System.Threading.Tasks;

namespace Graphs.Sources.Models
{
    public class BfsData
    {
        public MappedNode Node = null;
        public bool IsVisited = false;

        public LinkModel ParentLink = null;
        public MappedNode ParentMappedNode = null;
    }
}
