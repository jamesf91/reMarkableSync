using RemarkableSync.MyScript;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Remoting.Messaging;

// TODO: Exception handling
namespace RemarkableSync.document.v5
{
    class RmConstants
    {
        public static int X_MAX = 1404;
        public static int Y_MAX = 1872;
    }

    public abstract class ByteableList
    {
        protected List<ByteableList> _objects;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public ByteableList()
        {
            _objects = new List<ByteableList>();
        }

        public abstract ByteableList CreateChild();

        public void Append(ByteableList child)
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

        public virtual void FromStream(TaggedBinaryReader reader)
        {
            try
            {
                int numChildren = reader.ReadInt32();
                for (int i = 0; i < numChildren; ++i)
                {
                    ByteableList child = CreateChild();
                    child.FromStream(reader);
                    Append(child);
                }
                
            }catch (Exception ex)
            {
                throw ex;
            }
            
        }

        public override string ToString()
        {
            return $"Unimplemented base type";
        }

        public List<ByteableList> Objects
        {
            get { return _objects; }
        }
    }

    public class RmPage : ByteableList, IRmPageBinary
    {
        public static RmPage ParseStream(TaggedBinaryReader reader)
        {
            RmPage page = new RmPage();
            page.FromStream(reader);
            return page;
        }

        public override ByteableList CreateChild()
        {
            return new RmLayer();
        }

        public override void FromStream(TaggedBinaryReader reader)
        {
            base.FromStream(reader);
        }

        public override string ToString()
        {
            return $"Lines: nobjs={_objects.Count}";
        }

        public Bitmap GetBitmap()
        {
            return RmLinesDrawer.DrawPage(this);
        }

        public Tuple<StrokeGroup, BoundingBox> GetMyScriptFormat()
        {
            int yOffset = 0;

            List<Stroke> strokes = new List<Stroke>();
            BoundingBox bound = new BoundingBox();

            foreach (RmLayer rmLayer in Objects)
            {
                foreach (RmStroke rmStroke in rmLayer.Objects)
                {
                    if (!rmStroke.IsVisible())
                    {
                        continue;
                    }

                    Stroke stroke = new Stroke();
                    int count = rmStroke.Objects.Count;
                    List<int> xList = new List<int>(count);
                    List<int> yList = new List<int>(count);
                    int i = 0;
                    foreach (RmSegment rmSegment in rmStroke.Objects)
                    {
                        int x = (int)Math.Round(rmSegment.X);
                        int y = (int)Math.Round(rmSegment.Y) + yOffset;
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

            StrokeGroup strokeGroup = new StrokeGroup();
            strokeGroup.strokes = strokes.ToArray();

            return Tuple.Create(strokeGroup, bound);
        }
    }

    class RmLayer : ByteableList
    {
        public override ByteableList CreateChild()
        {
            return new RmStroke();
        }

        public override void FromStream(TaggedBinaryReader reader)
        {
            base.FromStream(reader);
        }

        public override string ToString()
        {
            return $"Layer: nobjs={_objects.Count}";
        }
    }

    class RmStroke : ByteableList
    {
        public RmPen Pen { get; set; }
        public RmPenColor Colour { get; set; }
        public float Width { get; set; }

        public override ByteableList CreateChild()
        {
            return new RmSegment();
        }

        public override void FromStream(TaggedBinaryReader reader)
        {
            Pen = (RmPen)reader.ReadInt32();
            Colour = (RmPenColor)reader.ReadInt32();
            reader.BaseStream.Position += 4; //advance 4 bytes
            Width = reader.ReadSingle();
            reader.BaseStream.Position += 4; //advance 4 bytes

            base.FromStream(reader);
        }

        public override string ToString()
        {
            return $"Stroke: pen={Pen}, color={Colour}, width={Width,-5: F4}, nobjs={_objects.Count}";
        }

        public bool IsVisible()
        {
            switch (Pen)
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

    class RmSegment : ByteableList
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Speed { get; set; }
        public float Tilt { get; set; }
        public float Width { get; set; }
        public float Pressure { get; set; }
        public override ByteableList CreateChild()
        {
            throw (new Exception("Segment has no children type"));
        }

        public override void FromStream(TaggedBinaryReader reader)
        {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Speed = reader.ReadSingle();
            Tilt = reader.ReadSingle();
            Width = reader.ReadSingle();
            Pressure = reader.ReadSingle();
        }

        public override string ToString()
        {
            return $"Segment: X={X,-6: F1}, Y={Y,-6: F1}, Speed={Speed,-6: F1}, Tilt={Width,-6: F4}, Width={Width,-6: F4}, Pressure={Pressure,-6: F4}, nobjs={_objects.Count}";
        }
    }
}
