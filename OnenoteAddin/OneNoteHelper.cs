using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace RemarkableSync.OnenoteAddin
{
    public class OneNoteHelper
    {
        static readonly List<string> PageObjectNames = new List<string>()
        {
            "Outline",
            "Image",
            "InkDrawing",
            "InsertedFile",
            "MediaFile",
            "FutureObject"
        };

        static private int PageXOffset = 36;
        static private int PageYOffset = 86;
        static private int ImageGap = 50;
        static private string PositionElementName = "Position";
        static private string SizeElementName = "Size";

        private Application _application;
        private XNamespace _ns;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public OneNoteHelper(Application application)
        {
            _application = application;
            GetNamespace();
        }

        public string GetCurrentNotebookId()
        {
            string xmlHierarchy;
            _application.GetHierarchy(null, HierarchyScope.hsNotebooks, out xmlHierarchy, XMLSchema.xs2013);

            XDocument hierachyDocument = XDocument.Parse(xmlHierarchy);
            var currentNoteBooks = from notebookNode in hierachyDocument.Descendants(_ns + "Notebook")
                                   where notebookNode.Attribute("isCurrentlyViewed")?.Value == "true"
                                   select notebookNode;

            if (currentNoteBooks.Count() > 0)
            {
                return currentNoteBooks.ElementAt(0).Attribute("ID")?.Value ?? null;
            }
            else
            {
                Logger.Debug("No notebook found as current");
                return null;
            }
        }

        public string GetCurrentSectionId()
        {
            string currentNoteBookId = GetCurrentNotebookId();
            if (currentNoteBookId == null)
            {
                return null;
            }

            string xmlHierarchy;
            _application.GetHierarchy(currentNoteBookId, HierarchyScope.hsSections, out xmlHierarchy, XMLSchema.xs2013);

            XDocument hierachyDocument = XDocument.Parse(xmlHierarchy);
            var currentSection = from sectionNode in hierachyDocument.Descendants(_ns + "Section")
                                 where sectionNode.Attribute("isCurrentlyViewed")?.Value == "true"
                                 select sectionNode;

            if (currentSection.Count() > 0)
            {
                return currentSection.ElementAt(0).Attribute("ID")?.Value ?? null;
            }
            else
            {
                Logger.Debug("No section found as current");
                return null;
            }
        }

        public string CreatePage(string sectionId, string pageName)
        {
            // Create the new page
            string pageId;
            _application.CreateNewPage(sectionId, out pageId, NewPageStyle.npsBlankPageWithTitle);

            string xml;
            _application.GetPageContent(pageId, out xml, PageInfo.piAll, XMLSchema.xs2013);
            var doc = XDocument.Parse(xml);
            var title = doc.Descendants(_ns + "T").First();
            title.Value = pageName;

            // Update the page
            _application.UpdatePageContent(doc.ToString(), DateTime.MinValue, XMLSchema.xs2013);
            return pageId;
        }

        public void AddPageContent(string pageId, string content)
        {
            string xml;
            _application.GetPageContent(pageId, out xml, PageInfo.piAll, XMLSchema.xs2013);
            var doc = XDocument.Parse(xml);
            var ns = doc.Root.Name.Namespace;

            var contentLines = content.Split('\n').ToList();
            XElement newOutline = new XElement(ns + "Outline");
            XElement oeChildren = new XElement(ns + "OEChildren");

            foreach (string contentLine in contentLines)
            {
                XElement oe = new XElement(ns + "OE");
                XElement t = new XElement(ns + "T");
                t.Add(new XCData(contentLine));
                oe.Add(t);
                oeChildren.Add(oe);
            }

            newOutline.Add(oeChildren);
            doc.Root.Add(newOutline);

            // Update the page
            _application.UpdatePageContent(doc.ToString(), DateTime.MinValue, XMLSchema.xs2013);
        }

        public void AppendPageImages(string pageId, List<Bitmap> images, double zoom = 1.0)
        {
            string xml;
            _application.GetPageContent(pageId, out xml, PageInfo.piAll, XMLSchema.xs2013);
            var pageDoc = XDocument.Parse(xml);

            int yPos = GetBottomContentYPos(pageDoc);

            foreach (var image in images)
            {
                yPos = AppendImage(pageDoc, image, zoom, yPos) + ImageGap;
            }

            _application.UpdatePageContent(pageDoc.ToString(), DateTime.MinValue, XMLSchema.xs2013);
        }

        public void AppendPageImage(string pageId, Bitmap image, double zoom = 1.0)
        {
            string xml;
            _application.GetPageContent(pageId, out xml, PageInfo.piAll, XMLSchema.xs2013);
            var pageDoc = XDocument.Parse(xml);

            int yPos = GetBottomContentYPos(pageDoc);
            AppendImage(pageDoc, image, zoom, yPos);

            _application.UpdatePageContent(pageDoc.ToString(), DateTime.MinValue, XMLSchema.xs2013);
        }

        private int AppendImage(XDocument pageDoc, Bitmap bitmap, double zoom, int yPos)
        {
            int height = (int)Math.Round(bitmap.Height * zoom);
            int width = (int)Math.Round(bitmap.Width * zoom);

            var ns = pageDoc.Root.Name.Namespace;
            XElement imageEl = new XElement(ns + "Image");

            XElement positionEl = new XElement(ns + "Position");
            positionEl.Add(new XAttribute("x", PageXOffset));
            positionEl.Add(new XAttribute("y", yPos));

            XElement sizeEl = new XElement(ns + "Size");
            sizeEl.Add(new XAttribute("width", width));
            sizeEl.Add(new XAttribute("height", height));

            XElement dataEl = new XElement(ns + "Data");
            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            dataEl.Value = Convert.ToBase64String(stream.ToArray());

            imageEl.Add(positionEl);
            imageEl.Add(sizeEl);
            imageEl.Add(dataEl);

            pageDoc.Root.Add(imageEl);
            return (yPos + height);
        }

        private void GetNamespace()
        {
            string xml;
            _application.GetHierarchy(null, HierarchyScope.hsNotebooks, out xml);

            var doc = XDocument.Parse(xml);
            _ns = doc.Root.Name.Namespace;
        }

        private int GetBottomContentYPos(XDocument pageDoc)
        {
            var ns = pageDoc.Root.Name.Namespace;
            int lowestYPos = PageYOffset;

            foreach (var child in pageDoc.Root.Elements())
            {
                var posEl = child.Element(ns + PositionElementName);
                var sizeEl = child.Element(ns + SizeElementName);
                if (posEl == null || sizeEl == null)
                {
                    continue;
                }

                try
                {
                    int yPos = 0;
                    int height = 0;
                    string yAttribValue = posEl.Attribute("y")?.Value;
                    if (yAttribValue != null)
                    {
                        yPos = (int)double.Parse(yAttribValue, CultureInfo.InvariantCulture);
                    }
                    string heightAttribValue = sizeEl.Attribute("height")?.Value;
                    if (heightAttribValue != null)
                    {
                        height = (int)double.Parse(heightAttribValue, CultureInfo.InvariantCulture);
                    }

                    lowestYPos = Math.Max(lowestYPos, (yPos + height));
                }
                catch (Exception err)
                {
                    Logger.Error($"error: {err.Message}");
                    continue;
                }
            }
            return lowestYPos;
        }
    }
}
