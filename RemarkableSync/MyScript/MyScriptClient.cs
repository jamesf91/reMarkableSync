using RemarkableSync.document;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RemarkableSync.MyScript
{
    public class MyScriptClient
    {
        static readonly string Url = "https://cloud.myscript.com/api/v4.0/iink/batch";
        static readonly string JiixContentType = "application/vnd.myscript.jiix,application/json";

        private HttpClient _client;

        private static string AppKeyName = "appkey";
        private static string HmacKeyName = "hmackey";
        private static string EmptyKey = "****";

        private string _appKey;
        private string _hmacKey;
        private IConfigStore _configStore;
        private bool _saveHwrData = false;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public MyScriptClient(IConfigStore configStore)
        {
#if DEBUG
            _saveHwrData = true;
#endif

            _appKey = "";
            _hmacKey = "";
            _configStore = configStore;
            _client = new HttpClient();
            LoadConfig();
        }

        public async Task<Tuple<int, string>> RequestHwr(RmDocument doc, int pageIndex, string language)
        {
            Tuple<int, string> resultTuple = Tuple.Create<int, string>(pageIndex, null);

            if (_appKey == "" || _hmacKey == "")
            {
                Logger.Debug("Unable to send request due to appkey or hmac kay being empty");
                return resultTuple;
            }

            string responseContentString = "";
            HwrRequestBundle requestBundle = doc.GetPageAsMyScriptHwrRequestBundle(pageIndex, language);

            try
            {
                bool _testing = false;
                string outFile = Path.Combine(Path.GetTempPath(), $"HwrResponse_{pageIndex}.json");
                if (_testing && File.Exists(outFile))
                {
                    Logger.Debug("IN TEST MODE NOT REQUESTING MyScript data, but reading from disk");
                    responseContentString = File.ReadAllText(outFile);
                }
                else
                {
                    string reqString = JsonSerializer.Serialize(requestBundle.Request);
                    if (_saveHwrData)
                    {
                        File.WriteAllText(Path.Combine(Path.GetTempPath(), "HwrRequest.json"), reqString);
                    }
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

                    HttpResponseMessage response = await _client.SendAsync(requestMessage);
                    if (!response.IsSuccessStatusCode)
                    {
                        Logger.Debug($"Request was not successful. Return status: {response.StatusCode}, {response.ReasonPhrase}");
                        return resultTuple;
                    }

                    responseContentString = await response.Content.ReadAsStringAsync();
                }
                
            }
            catch (Exception err)
            {
                Logger.Error($"HWR request exception: {err.Message}.\n {err.StackTrace}");
                return resultTuple;
            }

            try
            {
                if (_saveHwrData)
                {
                    File.WriteAllText(Path.Combine(Path.GetTempPath(), $"HwrResponse_{pageIndex}.json"), responseContentString);
                }
                MyScriptResult result = JsonSerializer.Deserialize<MyScriptResult>(responseContentString);
                return Tuple.Create(pageIndex, result.label);
            }
            catch (Exception err)
            {
                Logger.Error($"MyScriptResult json deseralizing failed with: {err.Message}.\n Content:\n{responseContentString}");
                return resultTuple;
            }
        }

        public void SetConfig(string appKey, string hmacKey)
        {
            _appKey = appKey;
            _hmacKey = hmacKey;
            WriteConfig();
        }

        private void LoadConfig()
        {
            string appKey = _configStore.GetConfig(AppKeyName);
            string hmacKey = _configStore.GetConfig(HmacKeyName);
            _appKey = appKey == EmptyKey ? "" : appKey;
            _hmacKey = hmacKey == EmptyKey ? "" : hmacKey;
        }

        private void WriteConfig()
        {
            Dictionary<string, string> mapConfigs = new Dictionary<string, string>();
            mapConfigs[AppKeyName] = _appKey?.Length > 0 ? _appKey : EmptyKey;
            mapConfigs[HmacKeyName] = _hmacKey?.Length > 0 ? _hmacKey : EmptyKey;
            _configStore.SetConfigs(mapConfigs);
        }

        private List<string> ParseResult(HwrRequestBundle requestBundle, MyScriptResult result)
        {
            List<string> resultList = new List<string>();
            var groupBounds = requestBundle.Bounds;
            Queue<Word> wordQueue = new Queue<Word>(result.words);
            foreach (var bound in groupBounds)
            {
                StringBuilder sb = new StringBuilder();
                while (true)
                {
                    if (wordQueue.Count == 0)
                    {
                        break;
                    }

                    var currWord = wordQueue.Peek();

                    // always append whitespace to current group
                    if (currWord.boundingbox is null)
                    {
                        sb.Append(wordQueue.Dequeue().label);
                        continue;
                    }

                    if (bound.Contains(currWord.boundingbox))
                    {
                        sb.Append(wordQueue.Dequeue().label);
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                resultList.Add(sb.ToString());
            }

            for (int i = 0; i < resultList.Count; ++i)
            {
                Logger.Debug($"MyScriptClient::ParseResult() - result item {i}: {resultList[i]}");
            }

            return resultList;
        }
    }
}
