using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphs.Sources.Models;
using Northwoods.GoXam.Model;

namespace Graphs.Sources.Helpers
{
    static class ModelExtension
    {
        public static bool IsOriented(this GraphLinksModel<NodeModel, string, string, LinkModel> model)
        {
            foreach (LinkModel link in model.LinksSource)
            {
                if (!link.IsOriented)
                    return false;
            }

            return true;
        }
    }
}
