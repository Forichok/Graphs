using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Graphs.Models;
using Northwoods.GoXam;

namespace Graphs.Tools
{
    public class CustomPartManager : PartManager
    {
        public CustomPartManager()
        {
            this.UpdatesRouteDataPoints = true;  // call UpdateRouteDataPoints when Link.Route.Points has changed
        }

        // this supports undo/redo of link route reshaping
        protected override void UpdateRouteDataPoints(Link link)
        {
            if (!this.UpdatesRouteDataPoints) return;   // in coordination with Load_Click and UpdateRoutes, above
            var data = link.Data as LinkModel;
            if (data != null)
            {
                data.Points = new List<Point>(link.Route.Points);
            }
        }
    }
}
