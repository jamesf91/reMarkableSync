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

        private Application _application;
        private XDocument _hierarchy;

        public OneNoteHelper(Application application)
        {
            _application = application;
            UpdateHierarchy();
        }

        public XElement GetCurrentNotebook()
        {
            XElement element = _hierarchy.Root;
            XNamespace ns = element.Name.Namespace;

            var currentNoteBooks = from notebookNode in element.Descendants(ns + "Notebook")
                                 where notebookNode.Attribute("isCurrentlyViewed")?.Value == "true"
                                 select notebookNode;

            if (currentNoteBooks.Count() > 0)
            {
                return currentNoteBooks.ElementAt(0);
            }
            else
            {
                // no notebook found as current
                return null;
            }
        }

        public XElement GetCurrentSection()
        {
            XElement element = _hierarchy.Root;
            XNamespace ns = element.Name.Namespace;

            var currentSection = from sectionNode in element.Descendants(ns + "Section")
                                 where sectionNode.Attribute("isCurrentlyViewed")?.Value == "true"
                                 select sectionNode;

            if (currentSection.Count() > 0)
            {
                return currentSection.ElementAt(0);
            }
            else
            {
                // no section found as current
                return null;
            }
        }

        private void UpdateHierarchy()
        {
            string xmlHierarchy;
            _application.GetHierarchy(null, HierarchyScope.hsPages, out xmlHierarchy);
            try
            {
                _hierarchy = XDocument.Parse(xmlHierarchy);
            }
            catch (Exception err)
            {
                Console.WriteLine("Error parsing hierarchy xml. Error:" + err.Message);
                _hierarchy = null;
            }
        }


    }
}
