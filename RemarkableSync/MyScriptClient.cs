using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using RemarkableSync.RmLine;


namespace RemarkableSync
{
    public class MyScriptClient
    {
        static readonly string Url = "https://cloud.myscript.com/api/v4.0/iink/batch";
        static readonly string JiixContentType = "application/vnd.myscript.jiix,application/json";

        private HttpClient _client;

        class HwrRequest
        {
            public int xDPI { get; set; }
            public int yDPI { get; set; }
            public string contentType { get; set; }
            public StrokeGroup[] strokeGroups { get; set; }
        }

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
            _client = new HttpClient();
            LoadConfig();
        }

        public MyScriptResult RequestHwr(List<RmPage> pages)
        {
            Console.WriteLine($"MyScriptClient::RequestHwr() -  requesting hand writing recognition for {pages.Count} pages");

            if (_appKey == "" || _hmacKey == "")
            {
                Console.WriteLine("MyScriptClient::RequestHwr() -  Unable to send request due to appkey or hmac kay being empty");
                return null;
            }

            string responseContentString = "";

            try
            {
                HwrRequest request = CreateHwrRequest(pages);
                string reqString = JsonSerializer.Serialize(request);
                byte[] requestContent = Encoding.Unicode.GetBytes(reqString);

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, Url);
                requestMessage.Content = new ByteArrayContent(requestContent);
                requestMessage.Content.Headers.Add("Content-Type", "application/json");
                requestMessage.Headers.Add("Accept", JiixContentType);
                requestMessage.Headers.Add("applicationKey", _appKey);

                using (HMACSHA512 hmac = new HMACSHA512(Encoding.ASCII.GetBytes(_appKey + _hmacKey)))
                {
                    byte[] hash = hmac.ComputeHash(requestContent);
                    StringBuilder sBuilder = new StringBuilder();

                    for (int i = 0; i < hash.Length; i++)
                        sBuilder.Append(hash[i].ToString("x2"));

                    requestMessage.Headers.Add("hmac", sBuilder.ToString());
                }

                HttpResponseMessage response = _client.SendAsync(requestMessage).Result;
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"MyScriptClient::RequestHwr() - Request was not successful. Return status: {response.StatusCode}, {response.ReasonPhrase}");
                    return null;
                }

                responseContentString = response.Content.ReadAsStringAsync().Result;
            }
            catch(Exception err)
            {
                Console.WriteLine($"MyScriptClient::RequestHwr() - HWR request exception: {err.Message}.\n {err.StackTrace}");
                return null;
            }

            try
            {
                MyScriptResult result = JsonSerializer.Deserialize<MyScriptResult>(responseContentString);
                return result;
            }
            catch (Exception err)
            {
                Console.WriteLine($"MyScriptClient::RequestHwr() - MyScriptResult json deseralizing failed with: {err.Message}.\n Content:\n{responseContentString}");
                return null;
            }
        }

        private HwrRequest CreateHwrRequest(List<RmPage> pages)
        {
            HwrRequest request = new HwrRequest();
            request.xDPI = request.yDPI = 226;
            request.contentType = "Text";
            request.strokeGroups = new StrokeGroup[pages.Count];
            for (int i = 0; i < pages.Count; ++i)
            {
                request.strokeGroups[i] = PageToStrokeGroup(pages[i], i);
            }
            return request;
        }

        private StrokeGroup PageToStrokeGroup(RmPage page, int pageNum)
        {
            int yOffset = pageNum * RmConstants.X_MAX;

            List<Stroke> strokes = new List<Stroke>();

            foreach(RmLayer rmLayer in page.Objects)
            {
                foreach (RmStroke rmStroke in rmLayer.Objects)
                {
                    if ((rmStroke.Pen == PenEnum.RUBBER) || (rmStroke.Pen == PenEnum.RUBBER_AREA) || (rmStroke.Pen == PenEnum.ERASE_ALL))
                    {
                        continue;
                    }

                    Stroke stroke = new Stroke();
                    int count = rmStroke.Objects.Count;
                    stroke.x = new int[count];
                    stroke.y = new int[count];
                    int i = 0;
                    foreach (RmSegment rmSegment in rmStroke.Objects)
                    {
                        stroke.x[i] = (int) Math.Round(rmSegment.X);
                        stroke.y[i] = (int) Math.Round(rmSegment.Y) + yOffset;
                        i++;
                    }
                    strokes.Add(stroke);
                }
            }

            StrokeGroup strokeGroup = new StrokeGroup();
            strokeGroup.strokes = strokes.ToArray();
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
