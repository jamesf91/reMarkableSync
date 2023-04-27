using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace RemarkableSync.document.v5
{
    public class RmLinesDrawer
    {
        static public List<Bitmap> DrawPages(List<RmPage> pages)
        {
            List<Bitmap> images = (from page in pages
                                   select DrawPage(page)).ToList();
            return images;
        }

        static public Bitmap DrawPage(RmPage page)
        {
            Bitmap image = new Bitmap(RmConstants.X_MAX, RmConstants.Y_MAX);

            Graphics graphics = Graphics.FromImage(image);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.White);

            foreach (RmLayer layer in page.Objects)
            {
                DrawLayer(layer, ref graphics);
            }

            return image;
        }

        static private void DrawLayer(RmLayer layer, ref Graphics graphics)
        {
            foreach (RmStroke stroke in layer.Objects)
            {
                if (stroke.IsVisible())
                {
                    DrawStroke(stroke, ref graphics);
                }
            }
        }

        static private void DrawStroke(RmStroke stroke, ref Graphics graphics)
        {
            Color color;

            switch (stroke.Colour)
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

            Pen pen = new Pen(color, stroke.Width);

            GraphicsPath path = new GraphicsPath();
            Point[] points = new Point[stroke.Objects.Count];
            for (int i = 0; i < stroke.Objects.Count; ++i)
            {
                RmSegment segment = (RmSegment)stroke.Objects[i];
                points[i] = new Point((int)segment.X, (int)segment.Y);
            }
            path.AddLines(points);
            graphics.DrawPath(pen, path);

            pen.Dispose();
        }

    }
}
