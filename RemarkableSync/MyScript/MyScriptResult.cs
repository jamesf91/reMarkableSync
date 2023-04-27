namespace RemarkableSync.MyScript
{
    public class MyScriptResult
    {
        public string type { get; set; }
        public BoundingBox boundingbox { get; set; }
        public string label { get; set; }
        public Word[] words { get; set; }
        public string version { get; set; }
        public string id { get; set; }
    }

    public class BoundingBox
    {
        public BoundingBox()
        {
            x = y = float.NaN;
            width = height = (float) 0.0;
        }

        public float x { get; set; }
        public float y { get; set; }
        public float width { get; set; }
        public float height { get; set; }

        public void Expand(float X, float Y)
        {
            if (x == float.NaN)
            {
                x = X;
            }
            else if (X < x)
            {
                width += (x - X);
                x = X;
            }
            else if (X > x + width)
            {
                width = X - x;
            }

            if (y == float.NaN)
            {
                y = Y;
            }
            else if (Y < y)
            {
                height += (y - Y);
                y = Y;
            }
            else if (Y > y + height)
            {
                height = Y - y;
            }
        }

        public bool Contains(float X, float Y)
        {
            if ((x - 1 > X) || (x + width + 1 < X))
            {
                return false;
            }
            if ((y - 1 > Y) || (y + height + 1 < Y))
            {
                return false;
            }

            return true;
        }

        public bool Contains(BoundingBox bound)
        {
            if (x - 1 > bound.x)
            {
                return false;
            }
            if ((x + width + 1) <  (bound.x + bound.width))
            {
                return false;
            }
            if (y - 1 > bound.y)
            {
                return false;
            }
            if ((y + height + 1) < (bound.y + bound.height))
            {
                return false;
            }
            return true;
        }
    }

    public class Word
    {
        public string label { get; set; }
        public string[] candidates { get; set; }
        public BoundingBox boundingbox { get; set; }
        public Item[] items { get; set; }
    }

    public class Item
    {
        public string timestamp { get; set; }
        public float[] X { get; set; }
        public float[] Y { get; set; }
        public int[] F { get; set; }
        public int[] T { get; set; }
        public string type { get; set; }
        public string id { get; set; }
    }

}
