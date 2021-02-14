﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemarkableSync.RmLine
{
    public enum PenEnum
    {
        // see https://github.com/ax3l/lines-are-beautiful/blob/develop/include/rmlab/Line.hpp
        BRUSH = 0,
        PENCIL_TILT = 1,
        BALLPOINT_PEN_1 = 2,
        MARKER_1 = 3,
        FINELINER_1 = 4,
        HIGHLIGHTER = 5,
        RUBBER = 6,  // used in version 5
        PENCIL_SHARP = 7,
        RUBBER_AREA = 8,
        ERASE_ALL = 9,
        SELECTION_BRUSH_1 = 10,
        SELECTION_BRUSH_2 = 11,
        // below used for version 5
        PAINT_BRUSH_1 = 12,
        MECHANICAL_PENCIL_1 = 13,
        PENCIL_2 = 14,
        BALLPOINT_PEN_2 = 15,
        MARKER_2 = 16,
        FINELINER_2 = 17,
        HIGHLIGHTER_2 = 18,
        DEFAULT = FINELINER_2
    }

    public enum ColourEnum
    {
        BLACK = 0,
        GREY = 1,
        WHITE = 2
    }

    abstract class ByteableList
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
            Console.WriteLine(ToString());
            foreach(var child in _objects)
            {
                child.Log();
            }
        }

        public virtual void FromStream(ref MemoryStream buffer)
        {
            byte[] numberBytes = new byte[4];
            buffer.Read(numberBytes, 0, numberBytes.Length);
            int numChildren = BitConverter.ToInt32(numberBytes, 0);
            for(int i = 0; i < numChildren; ++i)
            {
                ByteableList child = CreateChild();
                child.FromStream(ref buffer);
                Append(child);
            }
        }

        public override string ToString()
        {
            return $"Unimplemented base type";
        }
    }

    class Page: ByteableList
    {
        public string Header { get; set; }

        public override ByteableList CreateChild()
        {
            return new Layer();
        }

        public override void FromStream(ref MemoryStream buffer)
        {
            byte[] headerBytes = new byte[43];
            buffer.Read(headerBytes, 0, headerBytes.Length);
            Header = BitConverter.ToString(headerBytes);
            base.FromStream(ref buffer);
        }

        public override string ToString()
        {
            return $"Lines: header=\"{ Header}\", nobjs={_objects.Count}";
        }
    }

    class Layer:ByteableList
    {
        public override ByteableList CreateChild()
        {
            return new Stroke();
        }

        public override string ToString()
        {
            return $"Layer: nobjs={_objects.Count}";
        }
    }

    class Stroke: ByteableList
    {
        public PenEnum Pen  { get; set; }
        public ColourEnum Colour  { get; set; }
        public float Width { get; set; }

        public override ByteableList CreateChild()
        {
            return new Segment();
        }

        public override void FromStream(ref MemoryStream buffer)
        {
            byte[] strokeHeaderBytes = new byte[20];
            buffer.Read(strokeHeaderBytes, 0, strokeHeaderBytes.Length);

            Pen = (PenEnum) BitConverter.ToInt32(strokeHeaderBytes, 0);
            Colour = (ColourEnum)BitConverter.ToInt32(strokeHeaderBytes, 4);
            Width = BitConverter.ToSingle(strokeHeaderBytes, 12);

            base.FromStream(ref buffer);
        }

        public override string ToString()
        {
            return $"Stroke: pen={Pen.ToString()}, color={Colour.ToString()}, width={Width,-5: F4}, nobjs={_objects.Count}";
        }
    }

    class Segment: ByteableList
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

        public override void FromStream(ref MemoryStream buffer)
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
