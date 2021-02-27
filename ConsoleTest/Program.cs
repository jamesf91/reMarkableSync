using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemarkableSync.RmLine;

namespace RemarkableSync
{
    class Program
    {
        static void Main(string[] args)
        {
            RmCloud cloud = new RmCloud();

            List<RmItem> rootItems = cloud.GetItemHierarchy();
            RmItem item = (from root in rootItems
                where root.Type == RmItem.DocumentType
                select root).ToArray()[0];

            List<RmPage> pages = new List<RmPage>();

            using (RmDownloadedDoc doc = cloud.DownloadDocument(item))
            {
                for (int i = 0; i < doc.PageCount; ++i)
                {
                    pages.Add(doc.GetPageContent(i));
                }
            }

            MyScriptClient hwrClient = new MyScriptClient();
            MyScriptResult result = hwrClient.RequestHwr(pages);
            if (result != null)
            {
                Console.WriteLine($"HWR result: {result.label}");
            }
            Console.ReadKey();
        }
    }
}
