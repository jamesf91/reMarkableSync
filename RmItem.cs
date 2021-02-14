using System;
using System.Collections.Generic;

namespace RemarkableSync
{
    public class RmItem
    {
        public static string CollectionType = "CollectionType";
        public static string DocumentType = "DocumentType";

        public RmItem()
        {
            Children = new List<RmItem>();
        }

        public string ID { get; set; }
        public int Version { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public string BlobURLGet { get; set; }
        public DateTime BlobURLGetExpires { get; set; }
        public DateTime ModifiedClient { get; set; }
        public string Type { get; set; }
        public string VissibleName { get; set; }
        public int CurrentPage { get; set; }
        public bool Bookmarked { get; set; }
        public string Parent { get; set; }

        public List<RmItem> Children { get; set; }
    }
}



