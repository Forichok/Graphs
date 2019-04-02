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
        // this property remembers the curviness;
        // Double.NaN means let it use a default calculated value
        // default value of NaN causes Route to calculate it
        
        public GraphLinksModel<NodeModel, string, string, LinkModel> model;

        public bool IsOriented { get; set; }

        public bool IsSelected { get; set; }

        public double Curviness { get; set; }

        public Point Offset { get; set; }

        public string Weight
        {
            get
            {
                return Text;
            }
            set
            {
              //  RaisePropertyChanged("Weight",Text,value);
                Text = value;
            }
        }

        public LinkModel()
        {
            Offset = new Point(0, 0);
            Curviness = double.NaN;
            IsOriented = true;
            IsSelected = false;
            this.PropertyChanged += LinkModel_PropertyChanged;
        }

        public LinkModel(string from, string to, string text) : base(from, to)
        {
            
            Offset = new Point(0, 0);
            Curviness = double.NaN;
            IsOriented = true;
            IsSelected = false;
            this.PropertyChanged += LinkModel_PropertyChanged;
            Weight = text;
        }

        private void LinkModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                bool isContains = model.LinksSource.Cast<LinkModel>().Contains(this);
                var n = 0;
                if (Weight.Length != 0 && !isContains && int.TryParse(Weight, out n)&&n>=0)
                {
                    model.StartTransaction("Add LinkModel");
                    model.AddLink(this);
                    model.DoLinkAdded(this);
                    model.CommitTransaction("Add LinkModel");

                }
                else if (isContains && !int.TryParse(Weight, out n) && n <= 0)
                {
                    model.StartTransaction("Remove LinkModel");
                    model.RemoveLink(this);
                    model.DoLinkRemoved(this);
                    model.CommitTransaction("Remove LinkModel");
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
            e.Add(XHelper.Attribute("X", this.Offset.X, 0));
            e.Add(XHelper.Attribute("Y", this.Offset.Y, 0));
            e.Add(XHelper.Attribute("IsOriented", this.IsOriented, false));
            return e;
        }

        // read the extra property on the link data
        public override void LoadFromXElement(XElement e)
        {
            base.LoadFromXElement(e);
            this.Curviness = XHelper.Read("Curviness", e, double.NaN);
            var x = XHelper.Read("X", e, 0);
            var y = XHelper.Read("Y", e, 0);
            this.Offset=new Point(x, y);
            //this.Offset = XHelper.Read("Offset", e, new Point(0, 0));
            this.IsOriented = XHelper.Read("IsOriented", e, false);
        }

        public string GetTo(string startNode)
        {
            if (IsOriented)
                return To;

            return startNode == From ? To : From;
        }

        public string GetFrom(string startNode)
        {
            if (IsOriented)
                return From;

            return startNode == To ? From : To;
        }

    }

}
