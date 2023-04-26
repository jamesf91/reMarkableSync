using RemarkableSync.document;
using System.IO;
using System.Text;
using System.Text.Json;

namespace RemarkableSync
{
    public partial class RmSftpDataSource : IRmDataSource
    {
        public class NotebookMetadata
        {
            public static NotebookMetadata FromStream(Stream stream)
            {
                string content = "";
                stream.Position = 0;
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    content = reader.ReadToEnd();
                }
                return JsonSerializer.Deserialize<NotebookMetadata>(content);
            }

            public RmItem ToRmItem(string filename)
            {
                RmItem item = new RmItem();

                // strip extension from filename to use as ID
                int pos = filename.IndexOf('.');
                item.ID = pos == -1 ? filename : filename.Substring(0, pos);
                item.Version = version;
                item.Type = type;
                item.VissibleName = visibleName;
                item.Parent = parent;
                return item;
            }

            public bool deleted { get; set; }
            public string lastModified { get; set; }
            public int lastOpenedPage { get; set; }
            public bool metadatamodified { get; set; }
            public bool modified { get; set; }
            public string parent { get; set; }
            public bool pinned { get; set; }
            public bool synced { get; set; }
            public string type { get; set; }
            public int version { get; set; }
            public string visibleName { get; set; }
        }

        public class NotebookContent
        {
            public static NotebookContent FromStream(Stream stream)
            {
                string content = "";
                stream.Position = 0;
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    content = reader.ReadToEnd();
                }
                return JsonSerializer.Deserialize<NotebookContent>(content);
            }

            public int coverPageNumber { get; set; }
            public bool dummyDocument { get; set; }
            public Extrametadata extraMetadata { get; set; }
            public string fileType { get; set; }
            public string fontName { get; set; }
            public int lineHeight { get; set; }
            public int margins { get; set; }
            public string orientation { get; set; }
            public int pageCount { get; set; }
            public string[] pages { get; set; }
            public string textAlignment { get; set; }
            public int textScale { get; set; }
            public Transform transform { get; set; }
        }

        public class Extrametadata
        {
            public string LastBallpointColor { get; set; }
            public string LastBallpointSize { get; set; }
            public string LastBallpointv2Color { get; set; }
            public string LastBallpointv2Size { get; set; }
            public string LastCalligraphyColor { get; set; }
            public string LastCalligraphySize { get; set; }
            public string LastClearPageColor { get; set; }
            public string LastClearPageSize { get; set; }
            public string LastEraseSectionColor { get; set; }
            public string LastEraseSectionSize { get; set; }
            public string LastEraserColor { get; set; }
            public string LastEraserSize { get; set; }
            public string LastEraserTool { get; set; }
            public string LastFinelinerColor { get; set; }
            public string LastFinelinerSize { get; set; }
            public string LastFinelinerv2Color { get; set; }
            public string LastFinelinerv2Size { get; set; }
            public string LastHighlighterColor { get; set; }
            public string LastHighlighterSize { get; set; }
            public string LastHighlighterv2Color { get; set; }
            public string LastHighlighterv2Size { get; set; }
            public string LastMarkerColor { get; set; }
            public string LastMarkerSize { get; set; }
            public string LastMarkerv2Color { get; set; }
            public string LastMarkerv2Size { get; set; }
            public string LastPaintbrushColor { get; set; }
            public string LastPaintbrushSize { get; set; }
            public string LastPaintbrushv2Color { get; set; }
            public string LastPaintbrushv2Size { get; set; }
            public string LastPen { get; set; }
            public string LastPencilColor { get; set; }
            public string LastPencilSize { get; set; }
            public string LastPencilv2Color { get; set; }
            public string LastPencilv2Size { get; set; }
            public string LastReservedPenColor { get; set; }
            public string LastReservedPenSize { get; set; }
            public string LastSelectionToolColor { get; set; }
            public string LastSelectionToolSize { get; set; }
            public string LastSharpPencilColor { get; set; }
            public string LastSharpPencilSize { get; set; }
            public string LastSharpPencilv2Color { get; set; }
            public string LastSharpPencilv2Size { get; set; }
            public string LastSolidPenColor { get; set; }
            public string LastSolidPenSize { get; set; }
            public string LastTool { get; set; }
            public string LastZoomToolColor { get; set; }
            public string LastZoomToolSize { get; set; }
        }

        public class Transform
        {
            public int m11 { get; set; }
            public int m12 { get; set; }
            public int m13 { get; set; }
            public int m21 { get; set; }
            public int m22 { get; set; }
            public int m23 { get; set; }
            public int m31 { get; set; }
            public int m32 { get; set; }
            public int m33 { get; set; }
        }

    }
}

