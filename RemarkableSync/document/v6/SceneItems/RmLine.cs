using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace RemarkableSync.document.v6.SceneItems
{
    internal class RmLine
    {
        public RmPenColor penColor;
        public RmPen pen;
        public List<RmPoint> points = new List<RmPoint>();
        public double thickness_scale;
        public float starting_length;

        public bool IsVisible()
        {
            switch (pen)
            {
                case RmPen.ERASER:
                case RmPen.ERASER_AREA:
                case RmPen.ERASER_ALL:
                    return false;
                default:
                    return true;
            }
        }
    }
}
