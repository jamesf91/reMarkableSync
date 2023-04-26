using RemarkableSync.document;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;

namespace RemarkableSync
{
    class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static string startPath = @"C:\dev\reMarkableSync\backup";
        private static string outputPath = @"C:\dev\reMarkableSync\backup";
        private static string SftpIP = "192.168.0.204";
        private static string SftPass = "***";

        static void Main(string[] args)
        {
            //ProcessSource(0);
            doSomethingDifferent();

            Logger.Debug("Done! Press key to exit");
            Console.ReadKey();
        }

        static void doSomethingDifferent()
        {
            string ID = "c06ef5e1-76aa-40fa-97fd-5533253b1d02";
            int pageNr = 3;
            CancellationToken _cancellationToken = new CancellationToken();
            //IRmDataSource source = new LocalFolderDataSource(startPath);
            IRmDataSource source = new RmSftpDataSource(SftpIP, SftPass);
            RmDocument doc = source.DownloadDocument(ID, _cancellationToken, null).Result;
            doc.GetPageAsImage(pageNr).Save(Path.Combine(outputPath, $"{ID}_{pageNr + 1}.png"), ImageFormat.Png);
        }

        static void ProcessSource(int source, string ID = null)
        {
            IRmDataSource _rmDataSource = null;
            switch (source)
            {
                case 0:
                    Logger.Debug("Using local data source");
                    _rmDataSource = new LocalFolderDataSource(startPath);
                    break;
                case 1:
                    Logger.Debug("Using SFTP data source");
                    _rmDataSource = new RmSftpDataSource(SftpIP, SftPass);
                    break;
                case 2:
                    Logger.Debug("Using Online data source");
                    _rmDataSource = new RmCloudDataSource(new WinRegistryConfigStore(@"Software\Microsoft\Office\OneNote\AddInsData\RemarkableSync.OnenoteAddin"));
                    break;
                default:
                    return;
            }
            if (ID != null)
            {
                SavePageImages(_rmDataSource, ID);
            }
            else
            {
                NavigateRootItems(_rmDataSource);
            }
        }

        static void SavePageImages(IRmDataSource source, string ID)
        {
            Progress<string> progress = new Progress<string>((string updateText) =>
            {
                Logger.Debug($"Progressing:\n{updateText}");
            });
            CancellationToken _cancellationToken = new CancellationToken();

            RmDocument doc = source.DownloadDocument(ID, _cancellationToken, progress).Result;
            for (int i = 0; i < doc.PageCount; i++)
            {
                doc.GetPageAsImage(i).Save(Path.Combine(outputPath, $"{ID}_{i + 1}.png"), ImageFormat.Png);
            }
        }

        static void NavigateRootItems(IRmDataSource source)
        {
            Progress<string> progress = new Progress<string>((string updateText) =>
            {
                Logger.Debug($"Getting document list:\n{updateText}");
            });
            CancellationToken _cancellationToken = new CancellationToken();
            var rootItems = source.GetItemHierarchy(_cancellationToken, progress).Result;
            Logger.Debug($"Found {rootItems.Count} items");

            rootItems = RmItem.SortItems(rootItems);
            var currentSelection = rootItems;

            PrintDirectory(rootItems);
            string currentPath = "root";
            Console.WriteLine("Current Directory: {0}", currentPath);

            while (true)
            {
                Console.WriteLine("Enter a number to navigate to a subdirectory (or q to quit, r to restart):");
                string input = Console.ReadLine();
                int selection = 0;

                if (input == "q")
                {
                    break;
                }
                if (input == "r")
                {
                    currentSelection = rootItems;
                    currentPath = "root";
                    Console.Clear();
                    Console.WriteLine("Current Directory: {0}", currentPath);
                    PrintDirectory(currentSelection);
                    continue;

                }
                if (!int.TryParse(input, out selection))
                {
                    Console.WriteLine("Invalid input!");
                    continue;
                }

                if (selection < 1 || selection > currentSelection.Count)
                {
                    Console.WriteLine("Invalid selection!");
                    continue;
                }

                RmItem selected = currentSelection[selection - 1];
                if (selected.Type == RmItem.CollectionType)
                {
                    currentPath = Path.Combine(currentPath, selected.VissibleName);
                    currentSelection = selected.Children;
                }
                else
                {
                    SavePageImages(source, selected.ID);
                }

                Console.Clear();
                Console.WriteLine("Current Directory: {0}", currentPath);
                PrintDirectory(currentSelection);
            }
        }

        static void PrintDirectory(List<RmItem> collection)
        {
            Console.WriteLine("Folder:");
            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i].Type == RmItem.CollectionType)
                {
                    Console.WriteLine("\t{0}. {1}/", i + 1, collection[i].VissibleName);
                }
                else
                {
                    Console.WriteLine("\t{0}. {1}", i + 1, collection[i].VissibleName);
                }

            }
        }
    }
}
