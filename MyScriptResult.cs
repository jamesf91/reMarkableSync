namespace RemarkableSync
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
        public float x { get; set; }
        public float y { get; set; }
        public float width { get; set; }
        public float height { get; set; }
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
