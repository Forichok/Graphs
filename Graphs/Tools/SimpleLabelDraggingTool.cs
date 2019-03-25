using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Northwoods.GoXam;
using Northwoods.GoXam.Tool;

namespace Graphs.Tools
{
    // This tool only works when a Link has a LinkPanel with a single child element
    // that is positioned at the Route.MidPoint plus some Offset.

    public class SimpleLabelDraggingTool : DiagramTool
    {
        public override bool CanStart()
        {
            if (!base.CanStart()) return false;
            Diagram diagram = this.Diagram;
            if (diagram == null) return false;
            // require left button & that it has moved far enough away from the mouse down point, so it isn't a click
            if (!IsLeftButtonDown()) return false;
            if (!IsBeyondDragSize()) return false;
            return FindLabel() != null;
        }

        private FrameworkElement FindLabel()
        {
            var elt = this.Diagram.Panel.FindElementAt<System.Windows.Media.Visual>(this.Diagram.LastMousePointInModel, e => e, null, SearchLayers.Links);
            if (elt == null) return null;
            Link link = Part.FindAncestor<Link>(elt);
            if (link == null) return null;
            var parent = System.Windows.Media.VisualTreeHelper.GetParent(elt) as System.Windows.Media.Visual;
            while (parent != null && parent != link && !(parent is LinkPanel))
            {
                elt = parent;
                parent = System.Windows.Media.VisualTreeHelper.GetParent(elt) as System.Windows.Media.Visual;
            }
            if (parent is LinkPanel)
            {
                FrameworkElement lab = elt as FrameworkElement;
                if (lab == null) return null;
                // needs to be positioned relative to the MidPoint
                if (LinkPanel.GetIndex(lab) != Int32.MinValue) return null;
                // also check for movable-ness?
                return lab;
            }
            return null;
        }

        public override void DoActivate()
        {
            StartTransaction("Shifted Label");
            this.Label = FindLabel();
            if (this.Label != null)
            {
                this.OriginalOffset = LinkPanel.GetOffset(this.Label);
            }
            base.DoActivate();
        }

        public override void DoDeactivate()
        {
            base.DoDeactivate();
            StopTransaction();
        }

        private FrameworkElement Label { get; set; }
        private Point OriginalOffset { get; set; }

        public override void DoStop()
        {
            this.Label = null;
            base.DoStop();
        }

        public override void DoCancel()
        {
            if (this.Label != null)
            {
                LinkPanel.SetOffset(this.Label, this.OriginalOffset);
            }
            base.DoCancel();
        }

        public override void DoMouseMove()
        {
            if (!this.Active) return;
            UpdateLinkPanelProperties();
        }

        public override void DoMouseUp()
        {
            if (!this.Active) return;
            UpdateLinkPanelProperties();
            this.TransactionResult = "Shifted Label";
            StopTool();
        }

        public string TransactionResult { get; set; }

        private void UpdateLinkPanelProperties()
        {
            if (this.Label == null) return;
            Link link = Part.FindAncestor<Link>(this.Label);
            if (link == null) return;
            Point last = this.Diagram.LastMousePointInModel;
            Point mid = link.Route.MidPoint;
            // need to rotate this point to account for angle of middle segment
            Point p = new Point(last.X - mid.X, last.Y - mid.Y);
            LinkPanel.SetOffset(this.Label, RotatePoint(p, -link.Route.MidAngle));
        }

        private static Point RotatePoint(Point p, double angle)
        {
            if (angle == 0 || (p.X == 0 && p.Y == 0))
                return p;
            double rad = angle * Math.PI / 180;
            double cosine = Math.Cos(rad);
            double sine = Math.Sin(rad);
            return new Point((cosine * p.X - sine * p.Y),
                             (sine * p.X + cosine * p.Y));
        }
    }
}
