using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Office.Interop.OneNote;

namespace RemarkableSync.OnenoteAddin
{
    class OneNoteHelper
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

        private Application _application;
        private XNamespace _ns;

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
                Console.WriteLine("OneNoteHelper::GetCurrentNotebookId() - No notebook found as current");
                return null;
            }
        }

        public string GetCurrentSectionId()
        {
            string currentNoteBookId = GetCurrentNotebookId();
            if(currentNoteBookId == null)
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
                Console.WriteLine("OneNoteHelper::GetCurrentSectionId() - No section found as current");
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

            foreach(string contentLine in contentLines)
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

        private void GetNamespace()
        {
            string xml;
            _application.GetHierarchy(null, HierarchyScope.hsNotebooks, out xml);

            var doc = XDocument.Parse(xml);
            _ns = doc.Root.Name.Namespace;
        }

    }
}
