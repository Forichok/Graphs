using System;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Northwoods.GoXam;
using Northwoods.GoXam.Model;

namespace Graphs.Sources.Models
{
    // the data for each link
    [Serializable]
    public class LinkModel : GraphLinksModelLinkData<string, string>
    {
        private string _weight;
        // this property remembers the curviness;
        // Double.NaN means let it use a default calculated value
        // default value of NaN causes Route to calculate it
        public Diagram DiagramModel;
        public GraphLinksModel<NodeModel, string, string, LinkModel> model;

        public bool IsOriented { get; set; }

        public bool IsSelected { get; set; }

        public double Curviness { get; set; }

        public Point Offset { get; set; }

        public String Weight
        {
            get => _weight;
            set => _weight = value;
        }

        public EventHandler LinkChangedHandler;

        public LinkModel()
        {
            Text = "0";
            Offset = new Point(0, 0);
            Curviness = double.NaN;
            IsOriented = true;
            IsSelected = false;
            Weight = Text;
        }

        public LinkModel(string from, string to, string text) : base(from, to)
        {
            this.Text = text;
            Offset = new Point(0, 0);
            Curviness = double.NaN;
            IsOriented = true;
            IsSelected = false;
            this.PropertyChanged += LinkModel_PropertyChanged;
            Weight = Text;
        }

        private void LinkModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                bool isContains = model.LinksSource.Cast<LinkModel>().Contains(this);
                var n = 0;
                if (Weight.Length != 0 && !isContains && int.TryParse(Weight, out n)&&n>0)
                {
                    DiagramModel.StartTransaction("Add LinkModel");
                    model.AddLink(this);
                    model.DoLinkAdded(this);
                    DiagramModel.CommitTransaction("Add LinkModel");

                }
                else if (isContains && !int.TryParse(Weight, out n) && n > 0)
                {
                    DiagramModel.StartTransaction("Remove LinkModel");
                    model.RemoveLink(this);
                    model.DoLinkRemoved(this);
                    DiagramModel.CommitTransaction("Remove LinkModel");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            
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

        
    }

}
