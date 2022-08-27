using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace RemarkableSync
{ 
    class V2HttpHelper
    {
        private static string BlobHost = "https://rm-blob-storage-prod.appspot.com";
        private static string DownloadUrl = BlobHost + "/api/v1/signed-urls/downloads";
        private static string HeaderGeneration = "x-goog-generation";

        private HttpClient _client;

        public V2HttpHelper(HttpClient client)
        {
            _client = client;
        }

        private async Task<string> GetUrlAsync(string hash)
        {
            try
            {
                var requestContent = new BlobStorageRequest
                {
                    http_method = "GET",
                    relative_path = hash
                };
                HttpResponseMessage response = await HttpClientJsonExtensions.PostAsJsonAsync(_client, new Uri(DownloadUrl), requestContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Request failed with status code {response.StatusCode}");
                }

                BlobStorageResponse blobResponse = await HttpContentJsonExtensions.ReadFromJsonAsync<BlobStorageResponse>(response.Content);
                return blobResponse.url;
            }
            catch (Exception err)
            {
                Logger.LogMessage($"Failed to get url for hash: {hash}. err: {err.ToString()} ");
                return "";
            }
        }

        public async Task<BlobStream> GetBlobStreamFromHashAsync(string hash)
        {
            Logger.LogMessage($"Entering: ..  hash = {hash}");
            try
            {
                string url = await GetUrlAsync(hash);
                if (url == "")
                {
                    throw new Exception($"Failed to determine GET url");
                }

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Get
                };

                HttpResponseMessage response = await _client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Request failed with status code {response.StatusCode}");
                }

                BlobStream blobStream = new BlobStream
                {
                    Generation = long.Parse(response.Headers.GetValues(HeaderGeneration).First()),
                    Blob = await response.Content.ReadAsStringAsync()
                };

                return blobStream;
            }
            catch (Exception err)
            {
                Logger.LogMessage($"Failed to complete GET for hash: {hash}. err: {err.ToString()} ");
                return null;
            }
        }

        public async Task<Stream> GetStreamFromHashAsync(string hash)
        {
            Logger.LogMessage($"Entering: ..  hash = {hash}");
            try
            {
                string url = await GetUrlAsync(hash);
                if (url == "")
                {
                    throw new Exception($"Failed to determine GET url");
                }

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Get
                };

                HttpResponseMessage response = await _client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Request failed with status code {response.StatusCode}");
                }

                return await response.Content.ReadAsStreamAsync();
            }
            catch (Exception err)
            {
                Logger.LogMessage($"Failed to complete GET for hash: {hash}. err: {err.ToString()} ");
                return null;
            }
        }
    }

    class BlobStream
    {
        public string Blob { get; set; }
        public long Generation { get; set; }
    }

    class BlobStorageRequest
    {
        public string http_method { get; set; }
        public string relative_path { get; set; }
    }

    class BlobStorageResponse
    {
        public string relative_path { get; set; }
        public string url { get; set; }
        public string expires { get; set; }
        public string method { get; set; }
    }
}
