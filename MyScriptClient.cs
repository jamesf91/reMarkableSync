using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RemarkableSync.RmLine;

namespace RemarkableSync
{
    class MyScriptClient
    {
        class StrokeGroup
        {
            public Stroke[] strokes { get; set; }
        }

        class Stroke
        {
            public int[] x { get; set; }
            public int[] y { get; set; }
        }

        private static string ConfigFile = ".myscript";
        private static string AppKeyName = "appkey";
        private static string HmacKeyName = "hmackey";

        private string _appKey;
        private string _hmacKey;

        public MyScriptClient()
        {
            _appKey = "";
            _hmacKey = "";
            LoadConfig();
        }

        private StrokeGroup PageToStrokeGroup(RmPage page, int pageNum)
        {
            int yOffset = pageNum * RmConstants.X_MAX;

            // get all applicable strokes
            List<RmStroke> rmStrokes = new List<RmStroke>();
            foreach(var rmLayer in page.Objects)
            {
                foreach (RmStroke rmStroke in rmLayer.Objects)
                {
                    if ((rmStroke.Pen != PenEnum.RUBBER) && (rmStroke.Pen != PenEnum.RUBBER_AREA) && (rmStroke.Pen != PenEnum.ERASE_ALL))
                    {
                        rmStrokes.Add(rmStroke);
                    }
                }
            }

            StrokeGroup strokeGroup = new StrokeGroup();
            strokeGroup.strokes = new Stroke[rmStrokes.Count];
            return strokeGroup;
        }

        private void LoadConfig()
        {
            Dictionary<string, string> config = new Dictionary<string, string>();
            try
            {
                string configFilePath = GetConfigPath();
                foreach (string line in File.ReadLines(configFilePath))
                {
                    int delimPos = line.IndexOf(':');
                    if (delimPos == -1)
                        continue;
                    config.Add(line.Substring(0, delimPos).Trim(), line.Substring(delimPos + 1).Trim());
                }
            }
            catch (Exception err)
            {
                Console.WriteLine($"Unable to load token from config file. Err = {err.Message}");
            }

            if (config.ContainsKey(AppKeyName))
                _appKey = config[AppKeyName];

            if (config.ContainsKey(HmacKeyName))
                _hmacKey = config[HmacKeyName];
        }

        private void WriteConfig()
        {
            StreamWriter file = new StreamWriter(GetConfigPath());
            if (_appKey?.Length > 0)
            {
                file.WriteLine(String.Format("{0}: {1}", AppKeyName, _appKey));
            }
            if (_hmacKey?.Length > 0)
            {
                file.WriteLine(String.Format("{0}: {1}", HmacKeyName, _hmacKey));
            }
            file.Close();
        }

        private string GetConfigPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\" + ConfigFile;
        }

    }
}
