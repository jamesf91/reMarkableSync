using System;
using System.Collections.Generic;

namespace RemarkableSync.document
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

        public static List<RmItem> SortItems(List<RmItem> collection)
        {
            foreach (RmItem item in collection)
            {
                if (item.Children.Count > 0)
                {
                    item.Children = SortItems(item.Children);
                }
            }

            collection.Sort(
                delegate (RmItem p1, RmItem p2)
                {
                    int compareType = p1.Type.CompareTo(p2.Type);
                    if (compareType == 0)
                    {
                        return p1.VissibleName.CompareTo(p2.VissibleName);
                    }
                    return compareType;
                }
            );
            return collection;
        }
    }
}



