using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemarkableSync
{
    public partial class MyScriptClient
    {
        class HwrRequestBundle
        {
            public HwrRequest Request { get; set; }

            public List<BoundingBox> Bounds { get; set; }

        }

        class HwrRequest
        {
            public int xDPI { get; set; }
            public int yDPI { get; set; }
            public string contentType { get; set; }
            public Configuration configuration { get; set; }
            public StrokeGroup[] strokeGroups { get; set; }
        }

        class StrokeGroup
        {
            public Stroke[] strokes { get; set; }
        }

        class Stroke
        {
            public int[] x { get; set; }
            public int[] y { get; set; }
        }

        class Configuration
        {
            public string lang { get; set; }
        }
    }
}
