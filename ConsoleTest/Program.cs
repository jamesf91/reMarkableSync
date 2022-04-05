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


namespace RemarkableSync
{
    class Program
    {
        static void Main(string[] args)
        {
            const string testFilePath = @"D:\WIP\remarkable\8920031f-06fe-4f41-90f6-d985c12718d9\5d476d18-7d06-480d-84d8-71686ba91d19.rm";
            const string outImagePath = @"D:\WIP\remarkable\image.png";
            RmPage page = null;

            using (FileStream fileStream = File.OpenRead(testFilePath))
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    fileStream.CopyTo(stream);
                    page = RmPage.ParseStream(stream, new List<string>());
                }
            }

            Bitmap image = RmLinesDrawer.DrawPage(page);
            image.Save(outImagePath, ImageFormat.Png);

            Logger.LogMessage("Done exporting image");

            Application oneNoteApp = new Application();
            string thisPage = oneNoteApp.Windows.CurrentWindow.CurrentPageId;
            OneNoteHelper helper = new OneNoteHelper(oneNoteApp);
            List<Bitmap> images = new List<Bitmap> { image };
            helper.AppendPageImages(thisPage, images, 0.5);

            Logger.LogMessage("Done inserting image");
            Console.ReadKey();
        }
    }
}
