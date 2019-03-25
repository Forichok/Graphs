using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Northwoods.GoXam.Model;

namespace Graphs.Models
{
    // the data for each link
    [Serializable]
    public class LinkModel : GraphLinksModelLinkData<String, String>
    {
        // this property remembers the curviness;
        // Double.NaN means let it use a default calculated value
        // default value of NaN causes Route to calculate it

        public Boolean IsOriented { get; set; }

        public Boolean IsSelected { get; set; }

        public double Curviness { get; set; }

        public Point Offset { get; set; } 

        public LinkModel()
        {
            Text = "0";
            Offset = new Point(0, 0);
            Curviness = Double.NaN;
            IsOriented = true;
            IsSelected = false;
        }

        // write the extra property on the link data
        public override XElement MakeXElement(XName n)
        {
            XElement e = base.MakeXElement(n);
            e.Add(XHelper.Attribute("Curviness", this.Curviness, Double.NaN));
            e.Add(XHelper.Attribute("Offset", this.Offset, new Point(0, 0)));
            return e;
        }

        // read the extra property on the link data
        public override void LoadFromXElement(XElement e)
        {
            base.LoadFromXElement(e);
            this.Curviness = XHelper.Read("Curviness", e, Double.NaN);
            this.Offset = XHelper.Read("Offset", e, new Point(0, 0));
        }
    }

}
