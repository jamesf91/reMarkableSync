using System;
using System.Collections.Generic;

namespace RemarkableSync.document.v6.SceneItems
{
    /**
     * Block of text.
     * 
     * `items` are a CRDT sequence of strings. The `item_id` for each string refers
     * to its first character; subsequent characters implicitly have sequential
     * ids.
     * 
     * When formatting is present, some of `items` have a value of an integer
     * formatting code instead of a string.
     * 
     * `styles` are LWW values representing a mapping of character IDs to
     * `ParagraphStyle` values. These formats apply to each line of text (until the
     * next newline).
     * 
     * `pos_x`, `pos_y` and `width` are dimensions for the text block.
     *
     */
    internal class RmText
    {
        public CrdtSequence<String> items = new CrdtSequence<string>();
        public Dictionary<CrdtId, LwwValue<ParagraphStyle>> styles = new Dictionary<CrdtId, LwwValue<ParagraphStyle>>();
        public double pos_x;
        public double pos_y;
        public float width;
    }
}
