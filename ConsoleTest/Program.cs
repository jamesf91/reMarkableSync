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
            string settingsRegPath = @"Software\Microsoft\Office\OneNote\AddInsData\RemarkableSync.OnenoteAddin";
            IConfigStore _configStore = new WinRegistryConfigStore(settingsRegPath);

            // setup
            Dictionary<string, string> mapConfigs = new Dictionary<string, string>();
            mapConfigs["sshHost"] = "10.11.99.1";
            mapConfigs["SshPassword"] = "ABvontxEol";
            _configStore.SetConfigs(mapConfigs);

            // end setup

            IRmDataSource dataSource = new RmSftpDataSource(_configStore);
            //IRmDataSource dataSource = new RmCloudDataSource(_configStore);

            List<RmItem> rootItems = dataSource.GetItemHierarchy().Result;
            RmItem item = (from root in rootItems
                where root.Type == RmItem.DocumentType
                select root).ToArray()[0];

            List<RmPage> pages = new List<RmPage>();

            using (RmDownloadedDoc doc = dataSource.DownloadDocument(item).Result)
            {
                for (int i = 0; i < doc.PageCount; ++i)
                {
                    pages.Add(doc.GetPageContent(i));
                }
            }

            MyScriptClient hwrClient = new MyScriptClient(_configStore);
            MyScriptResult result = hwrClient.RequestHwr(pages).Result;
            if (result != null)
            {
                Console.WriteLine($"HWR result: {result.label}");
            }
            Console.ReadKey();
        }
    }
}
