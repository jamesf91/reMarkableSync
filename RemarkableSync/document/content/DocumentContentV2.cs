using System.Collections.Generic;

namespace RemarkableSync.document
{
    public class DocumentContentV2 : DocumentContent
    {
        public int customZoomCenterX { get; set; }
        public int customZoomCenterY { get; set; }
        public string customZoomOrientation { get; set; }
        public int customZoomPageHeight { get; set; }
        public int customZoomPageWidth { get; set; }
        public int customZoomScale { get; set; }
        public cPages cPages { get; set; }
        public string zoomMode { get; set; }


        public override List<string> getPages()
        {
            List<string> pages = new List<string>();
            foreach (Page item in cPages.pages)
            {
                pages.Add(item.id);
            }
            return pages;
        }
    }

    public class cPages
    {
        public TimestampedStringValue lastOpened { get; set; }
        public TimestampedIntValue original { get; set; }
        public Page[] pages { get; set; }
        public Uuid[] uuids { get; set; }

    }
    public class Uuid
    {
        public string first { get; set; }
        public int second { get; set; }
    }
    public class Page
    {
        public string id { get; set; }
        public TimestampedStringValue idx { get; set; }
        public TimestampedStringValue template { get; set; }
        public TimestampedStringValue scrollTime { get; set; }
        public TimestampedIntValue verticalScroll { get; set; }
    }

    public class TimestampedStringValue
    {
        public string timestamp { get; set; }
        public string value { get; set; }
    }

    public class TimestampedIntValue
    {
        public string timestamp { get; set; }
        public int value { get; set; }
    }
}



