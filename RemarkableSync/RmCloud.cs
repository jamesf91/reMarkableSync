using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Net;
using System.IO.Compression;

// TODO: Exception handling
namespace RemarkableSync
{
    public class RmCloud
    {
        private static string ConfigFile = ".rmapi";
        private static string DeviceTokenName = "devicetoken";
        private static string UserTokenName = "usertoken";
        private static string UserAgent = "rmapi";
        private static string Device = "desktop-windows";
        private static string DeviceTokenUrl = "https://my.remarkable.com/token/json/2/device/new";
        private static string UserTokenUrl = "https://my.remarkable.com/token/json/2/user/new";
        private static string BaseUrl = "https://document-storage-production-dot-remarkable-production.appspot.com"; 

        private string _devicetoken;
        private string _usertoken;
        private bool _initialized;

        private HttpClient _client;

        public RmCloud()
        {
            _usertoken = null;
            _devicetoken = null;
            _initialized = false;

            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("user-agent", UserAgent);  
        }

        public bool RegisterWithOneTimeCode(string oneTimeCode)
        {
            if (!_initialized)
            {
                Initialize();
            }

            string uuid = Guid.NewGuid().ToString();
            string requestString = $@"{{
                ""code"": {oneTimeCode},
                ""deviceDesc"": {Device},
                ""deviceID"": {uuid}
            }}";

            HttpResponseMessage response = Request(
                HttpMethod.Post, 
                DeviceTokenUrl, 
                null, 
                new ByteArrayContent(Encoding.ASCII.GetBytes(requestString)));

            if (response.IsSuccessStatusCode)
            {
                byte[] responseContent = response.Content.ReadAsByteArrayAsync().Result;
                _devicetoken = Encoding.ASCII.GetString(responseContent);
                WriteConfig();
                return true;
            }

            throw new Exception("Can't register device");
        }

        public List<RmItem> GetItemHierarchy()
        {
            List<RmItem> collection = GetAllItems();
            return getChildItemsRecursive("", ref collection);
        }

        public List<RmItem> GetAllItems()
        {
            if (!_initialized)
            {
                Initialize();
            }

            HttpResponseMessage response = Request(
                HttpMethod.Get,
                "/document-storage/json/2/docs",
                null,
                null);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("GetAllItems request failed with status code " + response.StatusCode.ToString());
                return null;
            }

            string responseContent = response.Content.ReadAsStringAsync().Result;
            List<RmItem> collection = JsonSerializer.Deserialize<List<RmItem>>(responseContent);
            return collection;
        }

        public RmDownloadedDoc DownloadDocument(RmItem item)
        {
            if (!_initialized)
            {
                Initialize();
            }

            if (item.Type != RmItem.DocumentType)
            {
                Console.WriteLine($"RmCloud::DownloadDocument() - item with id {item.ID} is not document type");
                return null;

            }

            try
            {
                // first get the blob url
                string url = $"/document-storage/json/2/docs?doc={WebUtility.UrlEncode(item.ID)}&withBlob=true";
                HttpResponseMessage response = Request(HttpMethod.Get, url, null, null);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("RmCloud::DownloadDocument() -  request failed with status code " + response.StatusCode.ToString());
                    return null;
                }
                List<RmItem> items = JsonSerializer.Deserialize<List<RmItem>>(response.Content.ReadAsStringAsync().Result);
                if (items.Count == 0)
                {
                    Console.WriteLine("RmCloud::DownloadDocument() - Failed to find document with id: " + item.ID);
                    return null;
                }
                string blobUrl = items[0].BlobURLGet;
                Stream stream = _client.GetStreamAsync(blobUrl).Result;
                ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read);

                return new RmDownloadedDoc(archive, item.ID);
            }
            catch (Exception err)
            {
                Console.WriteLine($"RmCloud::DownloadDocument() - failed for id {item.ID}. Error: {err.Message}");
                return null;
            }

        }

        private void Initialize()
        {
            LoadConfig();
            if (_devicetoken != null)
            {
                Console.WriteLine("device token loaded from config file");
                RenewToken();
            }
            _initialized = true;
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

            if (config.ContainsKey(DeviceTokenName))
                _devicetoken = config[DeviceTokenName];

            if (config.ContainsKey(UserTokenName))
                _usertoken = config[UserTokenName];
        }

        private void WriteConfig()
        {
            StreamWriter file = new StreamWriter(GetConfigPath());
            if (_devicetoken?.Length > 0)
            {
                file.WriteLine(String.Format("{0}: {1}", DeviceTokenName, _devicetoken));
            }
            if (_usertoken?.Length > 0)
            {
                file.WriteLine(String.Format("{0}: {1}", UserTokenName, _usertoken));
                _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_usertoken}");

            }
            file.Close();
        }

        private string GetConfigPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\" + ConfigFile;
        }

        private HttpResponseMessage Request(HttpMethod method, string url, Dictionary<string, string> header, HttpContent content)
        {
            if (!url.StartsWith("http"))
            {
                if (!url.StartsWith("/"))
                    url = "/" + url;
                url = BaseUrl + url;
            }

            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(url);
            request.Method = method;
            if (content != null)
            {
                request.Content = content;
            }

            // add/replace the supplied headers
            if (header != null)
            {
                foreach (var key in header.Keys)
                {
                    request.Headers.Add(key, header[key]);
                }
            }

            HttpResponseMessage response = _client.SendAsync(request).Result;
            return response;
        }

        private void RenewToken()
        {
            if (_devicetoken == null || _devicetoken.Length == 0)
            {
                throw new Exception("Please register a device first");
            }

            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Authorization", $"Bearer {_devicetoken}");

            HttpResponseMessage response = Request(HttpMethod.Post, UserTokenUrl, header, null);
            if (response.IsSuccessStatusCode)
            {
                byte[] responseContent = response.Content.ReadAsByteArrayAsync().Result;
                _usertoken = Encoding.ASCII.GetString(responseContent);
                WriteConfig();
                Console.WriteLine("user token renewed");
            }
            else
            {
                throw new Exception("Can't register device");
            }
        }

        private bool IsAuthenticated()
        {
            return _devicetoken != null && _devicetoken.Length > 0 && _usertoken != null && _usertoken.Length > 0;
        }

        private List<RmItem> getChildItemsRecursive(string parentId, ref List<RmItem> items)
        {
            var children = (from item in items where item.Parent == parentId select item).ToList(); 
            foreach(var child in children)
            {
                child.Children = getChildItemsRecursive(child.ID, ref items);
            }
            return children;
        }
    }
}
