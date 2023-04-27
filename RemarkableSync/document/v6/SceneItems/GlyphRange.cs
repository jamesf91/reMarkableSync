using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace RemarkableSync.document.v6.SceneItems
{
    internal class GlyphRange
    {
        public int start;
        public int length;
        public String text;
        public RmPenColor color;
        public List<RmRectangle> rectangles = new List<RmRectangle>();
    }
}
