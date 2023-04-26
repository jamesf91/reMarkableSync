using RemarkableSync.MyScript;
using System;
using System.Drawing;

namespace RemarkableSync
{
    public interface IRmPageBinary
    {
        Bitmap GetBitmap();

        Tuple<StrokeGroup, BoundingBox> GetMyScriptFormat();
    }
}
