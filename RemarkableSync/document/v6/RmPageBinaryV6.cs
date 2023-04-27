using RemarkableSync.document.v5;
using RemarkableSync.document.v6.SceneItems;
using RemarkableSync.MyScript;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Xml.Linq;


namespace RemarkableSync.document.v6
{
    internal class RmPageBinaryV6 : IRmPageBinary
    {

        public static int X_MAX = 1404;
        public static int X_SHIFT = X_MAX / 2;
        public static int Y_MAX = 1872;

        private TaggedBinaryReader _reader;
        private List<BlockList> _blocks = new List<BlockList>();

        private float _x_min = 0,
                _x_max = 0,
                _y_min = 0,
                _y_max = 0;
        private int _width = 0, _height = 0, _xpos_delta = 0, _ypos_delta = 0;
        private bool _documentLoaded = false;
        private bool _isBlankDocument = false;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public RmPageBinaryV6(ref TaggedBinaryReader reader)
        {
            _reader = reader;
            ReadBlocks();
        }

        public RmPageBinaryV6() {
            _isBlankDocument = true;
        }

        public List<BlockList> GetBlockLists()
        {
            return this._blocks;
        }

        private void ReadBlocks()
        {
            try
            {
                while (_reader.BaseStream.Position < _reader.BaseStream.Length)
                {
                    _blocks.Add(BlockList.ReadNextBlock(ref _reader));
                }
                CalculatePageDimensions();
                _documentLoaded = true;
                Logger.Debug("End of file!");
            }
            catch (Exception e)
            {
                Logger.Error("Error? " + e.Message);
            }
        }


        private void CalculatePageDimensions()
        {
            /**
             * {xpos,ypos} coordinates are based on the top-center point
             * of the doc **if there are no text boxes**. When you add
             * text boxes, the xpos/ypos values change.
             */

            foreach (var block in _blocks)
            {
                (float block_xmin, float block_xmax, float block_ymin, float block_ymax) = block.GetDimensions();
                _x_min = _x_min > block_xmin ? block_xmin : _x_min;
                _x_max = _x_max < block_xmax ? block_xmax : _x_max;
                _y_min = _y_min > block_ymin ? block_ymin : _y_min;
                _y_max = _y_max < block_ymax ? block_ymax : _y_max;
            }

            _xpos_delta = X_SHIFT;
            if ((_x_min + X_SHIFT) < 0)
            {
                //make sure there are no negative xpos
                _xpos_delta += (int)-(_x_min + X_SHIFT);
            }
            //_ypos_delta = 0;

            //adjust dimensions if needed
            _width = Convert.ToInt32(Math.Ceiling(Math.Max(X_MAX, _x_max - _x_min)));
            _height = Convert.ToInt32(Math.Ceiling(Math.Max(Y_MAX, _y_max - _y_min)));
        }
        public int GetPageWidth()
        {
            return _width;
        }
        public int GetPageHeight()
        {
            return _height;
        }

        public Bitmap GetBitmap()
        {
            if (!_documentLoaded)
            {
                Logger.Debug("Page not loaded, returning blank image");
                return new Bitmap(X_MAX, Y_MAX);
            }

            Bitmap image = new Bitmap(_width, _height);

            Graphics graphics = Graphics.FromImage(image);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.White);

            foreach (BlockList block in _blocks)
            {
                if (block is SceneLineItemBlock)
                {
                    RmLine line = ((SceneLineItemBlock)block).GetLine();
                    if (line != null)
                    {
                        DrawStroke(line, ref graphics);
                    }

                }
            }

            return image;
        }

        internal void DrawStroke(RmLine line, ref Graphics graphics, Color? color = null)
        {
            if (color == null)
            {
                switch (line.penColor)
                {
                    case RmPenColor.GREY:
                        color = Color.Gray;
                        break;
                    case RmPenColor.WHITE:
                        color = Color.White;
                        break;
                    case RmPenColor.BLACK:
                    default:
                        color = Color.Black;
                        break;
                }
            }

            Pen pen = new Pen(color.GetValueOrDefault(Color.Black), (float)line.thickness_scale);

            GraphicsPath path = new GraphicsPath();
            Point[] points = new Point[line.points.Count];
            for (int i = 0; i < line.points.Count; ++i)
            {
                RmPoint point = (RmPoint)line.points[i];
                points[i] = new Point((int)point.x + _xpos_delta, (int)point.y + _ypos_delta);
            }
            path.AddLines(points);
            graphics.DrawPath(pen, path);

            pen.Dispose();
        }

        public Tuple<StrokeGroup, BoundingBox> GetMyScriptFormat()
        {
            List<Stroke> strokes = new List<Stroke>();
            BoundingBox bound = new BoundingBox();

            if (!_documentLoaded)
            {
                Logger.Debug("Page not loaded, returning blank image");
                Tuple.Create(new StrokeGroup(), bound);
            }

            foreach (BlockList block in _blocks)
            {
                if (block is SceneLineItemBlock lineBlock)
                {
                    RmLine line = lineBlock.GetLine();
                    if (line != null && line.IsVisible())
                    {
                        Stroke stroke = new Stroke();
                        List<int> xList = new List<int>();
                        List<int> yList = new List<int>();
                        int i = 0;
                        foreach (RmPoint point in line.points)
                        {
                            int x = (int)Math.Round(point.x) + _xpos_delta;
                            int y = (int)Math.Round(point.y) + _ypos_delta;
                            if ((i > 0) && (x == xList[i - 1]) && (y == yList[i - 1]))
                            {
                                continue;
                            }
                            xList.Add(x);
                            yList.Add(y);
                            bound.Expand(x, y);
                            i++;
                        }
                        xList.TrimExcess();
                        yList.TrimExcess();
                        stroke.x = xList.ToArray();
                        stroke.y = yList.ToArray();
                        strokes.Add(stroke);
                    }

                }
            }

            StrokeGroup strokeGroup = new StrokeGroup
            {
                strokes = strokes.ToArray()
            };

            return Tuple.Create(strokeGroup, bound);
        }

        public class BlockInfo
        {
            public int offset;
            public int size;
        }

        public class MainBlockInfo : BlockInfo
        {
            public int block_type;
            public int min_version;
            public int current_version;
        }



        public abstract class BlockList
        {
            protected const Byte BLOCK_TYPE = 0x00;
            protected List<BlockList> _objects;
            protected TaggedBinaryReader _reader;
            protected MainBlockInfo _info;

            public static BlockList ReadNextBlock(ref TaggedBinaryReader reader)
            {
                MainBlockInfo info = new MainBlockInfo();
                info.size = (int)reader.ReadUInt32();

                int unknown = (int)reader.ReadByte();
                info.min_version = (int)reader.ReadByte();
                info.current_version = (int)reader.ReadByte();
                info.block_type = (int)reader.ReadByte();

                using (MemoryStream currentBlockContent = new MemoryStream(reader.ReadBytes(info.size)))
                {
                    BlockList currentBlock;
                    //parse block
                    switch (info.block_type)
                    {
                        case 0:
                            currentBlock = new MigrationInfoBlock(currentBlockContent);
                            break;
                        case 1:
                            currentBlock = new SceneTreeBlock(currentBlockContent);
                            break;
                        case 2:
                            currentBlock = new TreeNodeBlock(currentBlockContent);
                            break;
                        case 3:
                            currentBlock = new SceneGlyphItemBlock(currentBlockContent);
                            break;
                        case 4:
                            currentBlock = new SceneGroupItemBlock(currentBlockContent);
                            break;
                        case 5:
                            currentBlock = new SceneLineItemBlock(currentBlockContent);
                            break;
                        case 6:
                            currentBlock = new SceneTextItemBlock(currentBlockContent);
                            break;
                        case 7:
                            currentBlock = new RootTextBlock(currentBlockContent);
                            break;
                        case 9:
                            currentBlock = new AuthorIdsBlock(currentBlockContent);
                            break;
                        case 10:
                            currentBlock = new PageInfoBlock(currentBlockContent);
                            break;
                        default://todo: fix default
                            currentBlock = new UnknownBlock(currentBlockContent);
                            Logger.Debug($"Unknown block found: type {info.block_type}" );
                            break;
                    }
                    currentBlock._info = info;
                    Logger.Debug("Block header: Length " + info.size + ", minVersion " + info.min_version + ", curVersion " + info.current_version + ", blockType " + info.block_type);

                    currentBlock.FromStream();

                    return currentBlock;
                }
            }

            public BlockList(MemoryStream buffer)
            {
                _objects = new List<BlockList>();
                _reader = new TaggedBinaryReader(buffer);
            }

            public void Append(BlockList child)
            {
                _objects.Add(child);
            }

            public void Log()
            {
                Logger.Debug(ToString());
                foreach (var child in _objects)
                {
                    child.Log();
                }
            }

            public abstract void FromStream();

            /**
             * If this block updated the document dimensions
             * it should return values > 0
             */
            public virtual (float, float, float, float) GetDimensions()
            {
                return (0, 0, 0, 0);
            }

            public override string ToString()
            {
                return $"Unimplemented base type";
            }

            public List<BlockList> Objects
            {
                get { return _objects; }
            }

        }

        public class AuthorIdsBlock : BlockList
        {
            protected new const int BLOCK_TYPE = 9;
            private Dictionary<int, Guid> AuthorUuids = new Dictionary<int, Guid>();

            public AuthorIdsBlock(MemoryStream buffer) : base(buffer)
            {
            }

            public override void FromStream()
            {
                int subBlocks = _reader.ReadVarUint();
                for (int i = 0; i < subBlocks; i++)
                {
                    using (TaggedBinaryReader subblockReader = _reader.GetSubBlockAsBinaryReader(0))
                    {
                        int uuid_length = (int)subblockReader.ReadVarUint();

                        int author_id = (int)subblockReader.ReadUInt16();
                        AuthorUuids.Add(author_id, new Guid(subblockReader.ReadBytes(uuid_length)));
                    }

                }
            }
        }


        public class MigrationInfoBlock : BlockList
        {
            protected new const int BLOCK_TYPE = 0;
            private CrdtId _crtd_id;
            private bool _is_device = false;
            private bool _unknown = false;

            public MigrationInfoBlock(MemoryStream buffer) : base(buffer)
            {
            }

            public override void FromStream()
            {
                _crtd_id = _reader.ReadTaggedId(1);
                _is_device = _reader.ReadTaggedBool(2);
                //There might be remaining data in this block, discard for now
            }
        }

        public class TreeNodeBlock : BlockList
        {
            protected new const int BLOCK_TYPE = 2;
            private Group _group = new Group();

            public TreeNodeBlock(MemoryStream buffer) : base(buffer)
            {
            }

            public override void FromStream()
            {
                _group.node_id = _reader.ReadTaggedId(1);
                _group.label = _reader.ReadLwwString(2);
                _group.visible = _reader.ReadLwwBool(3);

                if (_reader.BaseStream.Position < _reader.BaseStream.Length)
                {
                    _group.anchor_id = _reader.ReadLwwId(7);
                    _group.anchor_type = _reader.ReadLwwInt(8);
                    _group.anchor_threshold = _reader.ReadLwwFloat(9);
                    _group.anchor_origin_x = _reader.ReadLwwFloat(10);
                }
            }
        }

        public class PageInfoBlock : BlockList
        {
            protected new const int BLOCK_TYPE = 10;
            private int _loads_count;
            private int _merges_count;
            private int _text_chars_count;
            private int _text_lines_count;

            public PageInfoBlock(MemoryStream buffer) : base(buffer)
            {
            }

            //private int _unknown;
            public override void FromStream()
            {
                _loads_count = _reader.ReadTaggedUInt32(1);
                _merges_count = _reader.ReadTaggedUInt32(2);
                _text_chars_count = _reader.ReadTaggedUInt32(3);
                _text_chars_count = _reader.ReadTaggedUInt32(4);

                //There might be remaining data in this block, discard for now
            }
        }

        public class UnknownBlock : BlockList
        {
            protected new const int BLOCK_TYPE = 13;

            public UnknownBlock(MemoryStream buffer) : base(buffer)
            {
            }

            public override void FromStream()
            {
                //No idea how to process yet
            }
        }

        public class SceneTreeBlock : BlockList
        {
            protected new const int BLOCK_TYPE = 1;
            private CrdtId _tree_id;
            private CrdtId _node_id;
            private bool _is_update;
            private CrdtId _parent_id;

            public SceneTreeBlock(MemoryStream buffer) : base(buffer)
            {
            }

            public override void FromStream()
            {
                _tree_id = _reader.ReadTaggedId(1);
                _node_id = _reader.ReadTaggedId(2);
                _is_update = _reader.ReadTaggedBool(3);

                TaggedBinaryReader subblockReader = _reader.GetSubBlockAsBinaryReader(4);
                _parent_id = subblockReader.ReadTaggedId(1);
                subblockReader.Close();
            }
        }



        //Group item block?
        public class SceneItemBlock : BlockList
        {
            protected const int ITEM_TYPE = 0;
            protected CrdtId parent_id;
            protected CrdtId item_id;
            protected CrdtId left_id;
            protected CrdtId right_id;
            protected int deleted_length;
            //private CrdtSequenceItem item;

            public SceneItemBlock(MemoryStream buffer) : base(buffer)
            {
                parent_id = _reader.ReadTaggedId(1);
                item_id = _reader.ReadTaggedId(2);
                left_id = _reader.ReadTaggedId(3);
                right_id = _reader.ReadTaggedId(4);
                deleted_length = _reader.ReadTaggedUInt32(5);
            }

            public override void FromStream()
            {
                throw new NotImplementedException();
            }
        }


        public class SceneGlyphItemBlock : SceneItemBlock
        {
            protected new const int BLOCK_TYPE = 3;
            protected new const int ITEM_TYPE = 1;

            private GlyphRange _value;

            public SceneGlyphItemBlock(MemoryStream buffer) : base(buffer)
            {
            }

            public override void FromStream()
            {
                _value = GlyphRangeFromStream();
            }

            public GlyphRange GlyphRangeFromStream()
            {
                GlyphRange glyphRange = new GlyphRange();
                glyphRange.start = _reader.ReadTaggedUInt32(2);
                glyphRange.length = _reader.ReadTaggedUInt32(3);

                glyphRange.color = (RmPenColor)_reader.ReadTaggedUInt32(4);
                glyphRange.text = _reader.ReadString(5);

                using (TaggedBinaryReader subblock = _reader.GetSubBlockAsBinaryReader(6))
                {
                    int num_rects = subblock.ReadVarUint();
                    for (int i = 0; i < num_rects; i++)
                    {
                        RmRectangle r = new RmRectangle();
                        r.x = _reader.ReadSingle();
                        r.y = _reader.ReadSingle();
                        r.w = _reader.ReadSingle();
                        r.h = _reader.ReadSingle();
                        glyphRange.rectangles.Add(r);
                    }
                }

                return glyphRange;
            }
        }


        public class SceneGroupItemBlock : SceneItemBlock
        {
            protected new const int BLOCK_TYPE = 4;
            protected new const int ITEM_TYPE = 2;

            public CrdtId value;

            public SceneGroupItemBlock(MemoryStream buffer) : base(buffer)
            {

            }

            public override void FromStream()
            {
                if (_reader.HasSubblock(6))
                {
                    using (TaggedBinaryReader subBlock = _reader.GetSubBlockAsBinaryReader(6))
                    {
                        int item_type = subBlock.ReadByte();
                        if (ITEM_TYPE != item_type)
                        {
                            Logger.Debug("Item type mismatach");
                        }
                        else
                        {
                            value = subBlock.ReadTaggedId(2);
                        }
                    }
                }
            }
        }


        public class SceneLineItemBlock : SceneItemBlock
        {
            protected new const int BLOCK_TYPE = 5;
            protected new const int ITEM_TYPE = 3;

            private RmLine _line;

            public RmLine GetLine() { return _line; }
            public SceneLineItemBlock(MemoryStream buffer) : base(buffer)
            {
            }

            public override void FromStream()
            {
                if (_reader.HasSubblock(6))
                {
                    using (TaggedBinaryReader subBlock = _reader.GetSubBlockAsBinaryReader(6))
                    {
                        int item_type = subBlock.ReadByte();
                        if (ITEM_TYPE != item_type)
                        {
                            Logger.Debug("Item type mismatach");
                        }
                        else
                        {
                            _line = LineFromStream(subBlock);
                        }
                    }
                }
            }

            public RmLine LineFromStream(TaggedBinaryReader reader)
            {
                RmLine l = new RmLine();
                l.pen = (RmPen)reader.ReadTaggedUInt32(1);
                l.penColor = (RmPenColor)reader.ReadTaggedUInt32(2);
                l.thickness_scale = reader.ReadTaggedDouble(3);
                l.starting_length = reader.ReadTaggedSingle(4);

                int point_size = _info.current_version == 1 ? 24 : 14;
                using (TaggedBinaryReader subblock = reader.GetSubBlockAsBinaryReader(5))
                {
                    int num_points = (int)subblock.BaseStream.Length / point_size;
                    for (int i = 0; i < num_points; i++)
                    {
                        l.points.Add(PointFromStream(subblock));
                    }
                }

                return l;
            }

            private RmPoint PointFromStream(TaggedBinaryReader reader)
            {
                RmPoint newPoint = new RmPoint();
                newPoint.x = reader.ReadSingle();
                newPoint.y = reader.ReadSingle();

                if (_info.current_version == 1)
                {
                    // calculation based on ddvk's reader
                    // XXX removed rounding so that can round-trip correctly?
                    newPoint.speed = (int)(reader.ReadSingle() * 4);
                    newPoint.direction = (int)(255 * reader.ReadSingle() / (Math.PI * 2));
                    newPoint.width = (int)Math.Round(reader.ReadSingle() * 4);
                    newPoint.pressure = (int)(reader.ReadSingle() * 255);
                }
                else
                {
                    newPoint.speed = reader.ReadUInt16();
                    newPoint.direction = reader.ReadUInt16();
                    newPoint.width = reader.ReadByte();
                    newPoint.pressure = reader.ReadByte();
                }

                return newPoint;
            }

            public override (float, float, float, float) GetDimensions()
            {
                float x_min = 0,
                    x_max = 0,
                    y_min = 0,
                    y_max = 0;

                if (_line != null)
                {
                    foreach (RmPoint point in _line.points)
                    {
                        x_min = x_min > point.x ? point.x : x_min;
                        x_max = x_max < point.x ? point.x : x_max;
                        y_min = y_min > point.y ? point.y : y_min;
                        y_max = y_max < point.y ? point.y : y_max;
                    }
                }

                return (x_min, x_max, y_min, y_max);
            }
        }


        public class SceneTextItemBlock : SceneItemBlock
        {
            protected new const int BLOCK_TYPE = 6;
            protected new const int ITEM_TYPE = 5;

            public SceneTextItemBlock(MemoryStream buffer) : base(buffer)
            {
            }

            public override void FromStream()
            {

            }
        }

        public class RootTextBlock : BlockList
        {
            protected new const int BLOCK_TYPE = 7;
            private CrdtId _block_id;
            private RmText _value = new RmText();

            public RootTextBlock(MemoryStream buffer) : base(buffer)
            {
            }

            public override void FromStream()
            {
                _block_id = _reader.ReadTaggedId(1);
                using (TaggedBinaryReader subBlock = _reader.GetSubBlockAsBinaryReader(2))
                {
                    //Text items
                    using (TaggedBinaryReader secondSubBlock = subBlock.GetSubBlockAsBinaryReader(1))
                    {
                        using (TaggedBinaryReader thirdSubBlock = secondSubBlock.GetSubBlockAsBinaryReader(1))
                        {
                            int num_subblocks = thirdSubBlock.ReadVarUint();
                            for (int i = 0; i < num_subblocks; i++)
                            {
                                _value.items.Add(TextItemFromStream(thirdSubBlock));
                            }
                        }
                    }

                    //Formatting
                    using (TaggedBinaryReader secondSubBlock = subBlock.GetSubBlockAsBinaryReader(2))
                    {
                        using (TaggedBinaryReader thirdSubBlock = secondSubBlock.GetSubBlockAsBinaryReader(1))
                        {
                            int num_subblocks = thirdSubBlock.ReadVarUint();
                            for (int i = 0; i < num_subblocks; i++)
                            {
                                (CrdtId id, LwwValue<ParagraphStyle> style) = TextFormatFromStream(thirdSubBlock);
                                _value.styles.Add(id, style);
                            }
                        }
                    }
                }

                //Last section
                using (TaggedBinaryReader subBlock = _reader.GetSubBlockAsBinaryReader(3))
                {
                    // "pos_x" and "pos_y" from ddvk? Gives negative number -- possibly could
                    // be bounding box?
                    _value.pos_x = subBlock.ReadDouble();
                    _value.pos_y = subBlock.ReadDouble();
                }

                _value.width = _reader.ReadTaggedSingle(4);
            }

            private CrdtSequenceItem<String> TextItemFromStream(TaggedBinaryReader reader)
            {
                CrdtSequenceItem<String> text = new CrdtSequenceItem<string>();

                using (TaggedBinaryReader subBlock = reader.GetSubBlockAsBinaryReader(0))
                {
                    text.item_id = subBlock.ReadTaggedId(2);
                    text.left_id = subBlock.ReadTaggedId(3);
                    text.right_id = subBlock.ReadTaggedId(4);
                    text.deleted_length = subBlock.ReadTaggedUInt32(5);

                    if (_reader.HasSubblock(6))
                    {
                        (String subBlockText, int subBlockFormat) = subBlock.ReadStringWithFormat(6);

                        //It seems that formats are stored on empty strings, so it's one or the other
                        if (subBlockFormat > -1)
                        {
                            if (subBlockText.Length > 0)
                            {
                                text.value = subBlockFormat.ToString();
                            }
                            else
                            {
                                text.value = subBlockText;
                            }
                        }
                    }
                    else
                    {
                        text.value = "";
                    }
                }

                return text;
            }

            private (CrdtId, LwwValue<ParagraphStyle>) TextFormatFromStream(TaggedBinaryReader reader)
            {
                //These are character ids, but not with an initial tag like other ids have.
                CrdtId char_id = reader.ReadId();

                /**
                 * This seems to be the item ID for this format data ? It doesn't appear
                 * elsewhere in the file. Sometimes coincides with a character id but I don't
                 * think it is referring to it.
                 */
                CrdtId timestamp = reader.ReadTaggedId(1);

                using (TaggedBinaryReader subBlock = reader.GetSubBlockAsBinaryReader(2))
                {
                    //XXX not sure what this is format? (should be 17)
                    int c = subBlock.ReadByte();
                    int format_code = subBlock.ReadByte();
                    ParagraphStyle format_type;
                    try
                    {
                        format_type = (ParagraphStyle)format_code;

                    }
                    catch (Exception ex)
                    {
                        format_type = ParagraphStyle.PLAIN; //fallback
                    }

                    return (char_id, new LwwValue<ParagraphStyle>(timestamp, format_type));
                }
            }
        }
    }
}
