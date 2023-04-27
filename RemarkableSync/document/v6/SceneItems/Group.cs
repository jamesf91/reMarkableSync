using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RemarkableSync.document.v6.RmPageBinaryV6;

namespace RemarkableSync.document.v6.SceneItems
{
    /****
     * 
     * A Group represents a group of nested items.
     * 
     * Groups are used to represent layers.
     * 
     * node_id is the id that this sub-tree is stored as a "SceneTreeBlock".
     * 
     * children is a sequence of other SceneItems.
     * 
     * `anchor_id` refers to a text character which provides the anchor y-position
     * for this group. There are two values that seem to be special:
     * - `0xfffffffffffe` seems to be used for lines right at the top of the page?
     * - `0xffffffffffff` seems to be used for lines right at the bottom of the page?
     * 
     */
    internal class Group : SceneItem
    {
        public CrdtId node_id;
        public CrdtSequence<SceneItem> children;
        public LwwValue<String> label = new LwwValue<string>(new CrdtId(0, 0), "");
        public LwwValue<bool> visible = new LwwValue<bool>(new CrdtId(0, 0), true);

        public LwwValue<CrdtId> anchor_id;
        public LwwValue<int> anchor_type;
        public LwwValue<float> anchor_threshold;
        public LwwValue<float> anchor_origin_x;

        public Group() { 
            
        }
    }
}
