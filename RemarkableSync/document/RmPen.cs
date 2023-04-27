using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemarkableSync.document
{
    public enum RmPen
    {
     /**
      * """
      * Stroke pen id representing reMarkable tablet tools.
      * Tool examples: ballpoint, fineliner, highlighter or eraser.
      *
      * XXX this list is from remt pre-v6
      **/
        BALLPOINT_1 = 2,
        BALLPOINT_2 = 15,
        CALIGRAPHY = 21,
        ERASER = 6,
        ERASER_AREA = 8,
        ERASER_ALL = 9,
        FINELINER_1 = 4,
        FINELINER_2 = 17,
        HIGHLIGHTER_1 = 5,
        HIGHLIGHTER_2 = 18,
        MARKER_1 = 3,
        MARKER_2 = 16,
        MECHANICAL_PENCIL_1 = 7,
        MECHANICAL_PENCIL_2 = 13,
        PAINTBRUSH_1 = 0,
        PAINTBRUSH_2 = 12,
        PENCIL_1 = 1,
        PENCIL_2 = 14,
        SELECTION_BRUSH_1 = 10,
        SELECTION_BRUSH_2 = 11,
    }
}
