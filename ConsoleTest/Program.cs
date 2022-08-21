using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using RemarkableSync.RmLine;
using RemarkableSync.OnenoteAddin;
using Application = Microsoft.Office.Interop.OneNote.Application;
using System.Threading;

namespace RemarkableSync
{
    class Program
    {
        static void Main(string[] args)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            Progress<string> progress = new Progress<string>(str => Console.WriteLine($"Progress: {str}"));

            RmCloudDataSource datasource = new RmCloudDataSource(new WinRegistryConfigStore(@"Software\Microsoft\Office\OneNote\AddInsData\RemarkableSync.OnenoteAddin"));
            var items = datasource.GetItemHierarchy(source.Token, progress).Result;

            Console.ReadKey();
        }
    }
}
