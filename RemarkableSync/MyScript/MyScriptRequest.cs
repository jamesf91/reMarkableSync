using System.Collections.Generic;

namespace RemarkableSync.MyScript
{

    public class HwrRequestBundle
    {
        public HwrRequest Request { get; set; }

        public List<BoundingBox> Bounds { get; set; }

    }

    public class HwrRequest
    {
        public int xDPI { get; set; }
        public int yDPI { get; set; }
        public string contentType { get; set; }
        public Configuration configuration { get; set; }
        public StrokeGroup[] strokeGroups { get; set; }
    }

    public class StrokeGroup
    {
        public Stroke[] strokes { get; set; }
    }

    public class Stroke
    {
        public int[] x { get; set; }
        public int[] y { get; set; }
    }

    public class Configuration
    {
        public string lang { get; set; }
    }
}
