using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemarkableSync.document.v6.SceneItems;

// TODO: Exception handling
namespace RemarkableSync.document.v6
{
    class RmConstants
    {
        public static int X_MAX = 1404;
        public static int Y_MAX = 1872;
    }

    public abstract class ByteableList
    {
        protected List<ByteableList> _objects;

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
            Logger.LogMessage(ToString());
            foreach(var child in _objects)
            {
                child.Log();
            }
        }

        public virtual void FromStream(MemoryStream buffer, List<string> layerNames)
        {
            byte[] numberBytes = new byte[4];
            buffer.Read(numberBytes, 0, numberBytes.Length);
            int numChildren = BitConverter.ToInt32(numberBytes, 0);
            for(int i = 0; i < numChildren; ++i)
            {
                ByteableList child = CreateChild();
                child.FromStream( buffer, layerNames);
                Append(child);
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

    public class RmPage: ByteableList
    {
        public static RmPage ParseStream(MemoryStream contentStream, List<string> layerNames)
        {
            RmPage page = new RmPage();
            contentStream.Seek(0, SeekOrigin.Begin);
            page.FromStream(contentStream, layerNames);
            return page;
        }

        public string Header { get; set; }
        protected int fileVersion;

        public override ByteableList CreateChild()
        {
            return new RmLayer();
        }

        public override void FromStream( MemoryStream buffer, List<string> layerNames)
        {
            byte[] headerBytes = new byte[43];
            buffer.Read(headerBytes, 0, headerBytes.Length);
            Header = Encoding.ASCII.GetString(headerBytes, 0, headerBytes.Length);

            int.TryParse(Header.Split('=').ElementAt(1), out fileVersion);
            base.FromStream(buffer, layerNames);
        }

        public override string ToString()
        {
            return $"Lines: header=\"{ Header}\", nobjs={_objects.Count}";
        }
    }

    class RmLayer:ByteableList
    {
        public Color? LayerColor { get; set; }

        public override ByteableList CreateChild()
        {
            return new RmStroke();
        }

        public override void FromStream(MemoryStream buffer, List<string> layerNames)
        {
            LayerColor = null;
            if (layerNames.Count > 0)
            {
                var color = Color.FromName(layerNames[0]);
                if (color.IsKnownColor)
                {
                    LayerColor = color;
                }
                layerNames.RemoveAt(0);
            }
            
            base.FromStream(buffer, layerNames);
        }

        public override string ToString()
        {
            return $"Layer: nobjs={_objects.Count}";
        }
    }

    class RmStroke: ByteableList
    {
        public RmPen Pen  { get; set; }
        public RmColor Colour  { get; set; }
        public float Width { get; set; }

        public override ByteableList CreateChild()
        {
            return new RmSegment();
        }

        public override void FromStream(MemoryStream buffer, List<string> layerNames)
        {
            byte[] strokeHeaderBytes = new byte[20];
            buffer.Read(strokeHeaderBytes, 0, strokeHeaderBytes.Length);

            Pen = (RmPen)BitConverter.ToInt32(strokeHeaderBytes, 0);
            Colour = (RmColor)BitConverter.ToInt32(strokeHeaderBytes, 4);
            Width = BitConverter.ToSingle(strokeHeaderBytes, 12);

            base.FromStream(buffer, layerNames);
        }

        public override string ToString()
        {
            return $"Stroke: pen={Pen.ToString()}, color={Colour.ToString()}, width={Width,-5: F4}, nobjs={_objects.Count}";
        }

        public bool IsVisible()
        {
            switch(Pen)
            {
                case RmPen.ERASER: 
                case RmPen.ERASER_AREA: 
                default:
                    return true;
            }
        }
    }

    class RmSegment: ByteableList
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

        public override void FromStream( MemoryStream buffer, List<string> layerNames)
        {
            byte[] segmentBytes = new byte[24];
            buffer.Read(segmentBytes, 0, segmentBytes.Length);

            X = BitConverter.ToSingle(segmentBytes, 0);
            Y = BitConverter.ToSingle(segmentBytes, 4);
            Speed = BitConverter.ToSingle(segmentBytes, 8);
            Tilt = BitConverter.ToSingle(segmentBytes, 12);
            Width = BitConverter.ToSingle(segmentBytes, 16);
            Pressure = BitConverter.ToSingle(segmentBytes, 20);
        }

        public override string ToString()
        {
            return $"Segment: X={X,-6: F1}, Y={Y,-6: F1}, Speed={Speed,-6: F1}, Tilt={Width,-6: F4}, Width={Width,-6: F4}, Pressure={Pressure,-6: F4}, nobjs={_objects.Count}";
        }
    }
}
