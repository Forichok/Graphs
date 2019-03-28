using System;
using System.Windows;
using System.Xml.Linq;
using Northwoods.GoXam.Model;

namespace Graphs.Sources.Models
{
    // the data for each link
    [Serializable]
    public class LinkModel : GraphLinksModelLinkData<string, string>
    {
        // this property remembers the curviness;
        // Double.NaN means let it use a default calculated value
        // default value of NaN causes Route to calculate it

        public bool IsOriented { get; set; }

        public bool IsSelected { get; set; }

        public double Curviness { get; set; }

        public Point Offset { get; set; } 

        public LinkModel()
        {
            Text = "0";
            Offset = new Point(0, 0);
            Curviness = double.NaN;
            IsOriented = true;
            IsSelected = false;
        }

        public LinkModel(string from, string to, string text) : base(from, to)
        {
            this.Text = text;
            Offset = new Point(0, 0);
            Curviness = double.NaN;
            IsOriented = true;
            IsSelected = false;
        }

        //TODO:: fix this creating and saving
        // write the extra property on the link data
        public override XElement MakeXElement(XName n)
        {
            XElement e = base.MakeXElement(n);
            e.Add(XHelper.Attribute("Curviness", this.Curviness, double.NaN));
            e.Add(XHelper.Attribute("Offset", this.Offset, new Point(0, 0)));
            return e;
        }

        // read the extra property on the link data
        public override void LoadFromXElement(XElement e)
        {
            base.LoadFromXElement(e);
            this.Curviness = XHelper.Read("Curviness", e, double.NaN);
            this.Offset = XHelper.Read("Offset", e, new Point(0, 0));
        }

        public string GetTo(string startNode)
        {
            if (IsOriented)
                return To;

            return startNode == From ? To : From;
        }
    }

}
