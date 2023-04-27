using RemarkableSync.document;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// TODO: Exception handling
namespace RemarkableSync
{
    public class RmCloudDataSource : IRmDataSource
    {
        private static string DeviceTokenName = "rmdevicetoken";
        private static string UserTokenName = "rmusertoken";
        private static string EmptyToken = "****";
        private static string UserAgent = "rmapi";
        private static string Device = "desktop-windows";
        private static string DefaultDeviceTokenUrl = "https://webapp-prod.cloud.remarkable.engineering/token/json/2/device/new";
        private static string DefaultUserTokenUrl = "https://webapp-prod.cloud.remarkable.engineering/token/json/2/user/new";
        private static string CustomDeviceTokenUrlName = "CustomDeviceTokenUrl";
        private static string CustomUserTokenUrlName = "CustomUserTokenUrl";


        private string _deviceTokenUrl;
        private string _userTokenUrl;
        private string _devicetoken;
        private string _usertoken;
        private bool _initialized;

        private HttpClient _client;
        private IConfigStore _configStore;
        private IConfigStore _hiddenConfigStore;
        private ICloudApiClient _apiClient;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public RmCloudDataSource(IConfigStore configStore, IConfigStore hiddenConfigStore = null)
        {
            _usertoken = null;
            _devicetoken = null;
            _initialized = false;
            _configStore = configStore;
            _hiddenConfigStore = hiddenConfigStore;
            _apiClient = null;

            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("user-agent", UserAgent);

            // check for hidden configs
            _deviceTokenUrl = hiddenConfigStore?.GetConfig(CustomDeviceTokenUrlName) ?? DefaultDeviceTokenUrl;
            _userTokenUrl = hiddenConfigStore?.GetConfig(CustomUserTokenUrlName) ?? DefaultUserTokenUrl;

        }

        public async Task<bool> RegisterWithOneTimeCode(string oneTimeCode)
        {
            string uuid = Guid.NewGuid().ToString();
            string requestString = $@"{{
                ""code"": ""{oneTimeCode}"",
                ""deviceDesc"": ""{Device}"",
                ""deviceID"": ""{uuid}""
            }}";

            try
            {
                Logger.Debug($"registring with code: {oneTimeCode}");
                HttpResponseMessage response = await Request(
                    HttpMethod.Post,
                    _deviceTokenUrl,
                    null,
                    new ByteArrayContent(Encoding.ASCII.GetBytes(requestString)));

                if (response.IsSuccessStatusCode)
                {
                    byte[] responseContent = response.Content.ReadAsByteArrayAsync().Result;
                    _devicetoken = Encoding.ASCII.GetString(responseContent);
                    WriteConfig();
                    return true;
                }
                else
                {
                    Logger.Debug($"response code: {response.StatusCode}");
                }
            }
            catch (Exception err)
            {
                Logger.Error("Error: " + err.Message);
            }
            return false;
        }

        public async Task<List<RmItem>> GetItemHierarchy(CancellationToken cancellationToken, IProgress<string> progress)
        {
            List<RmItem> collection = await GetAllItems(cancellationToken, progress);
            return getChildItemsRecursive("", ref collection);
        }

        private async Task<List<RmItem>> GetAllItems(CancellationToken cancellationToken, IProgress<string> progress)
        {
            if (!_initialized)
            {
                await Initialize();
            }

            if (_apiClient == null)
            {
                string errMsg = "Enable to create cloud API client.";
                Logger.Error(errMsg);
                throw new Exception(errMsg);
            }

            return await _apiClient.GetAllItems(cancellationToken, progress);
        }

        public async Task<RmDocument> DownloadDocument(string ID, CancellationToken cancellationToken, IProgress<string> progress)
        {
            if (!_initialized)
            {
                await Initialize();
            }

            if (_apiClient == null)
            {
                string errMsg = "Enable to create cloud API client.";
                Logger.Error(errMsg);
                throw new Exception(errMsg);
            }

            return await _apiClient.DownloadDocument(ID, cancellationToken, progress);
        }

        public void Dispose()
        {
            _client?.Dispose();
            _configStore?.Dispose();
            _hiddenConfigStore?.Dispose();
            _apiClient?.Dispose();
        }

        private async Task<bool> Initialize()
        {
            LoadConfig();
            if (_devicetoken != null)
            {
                Logger.Debug("device token loaded from config file");
                _initialized = await RenewToken();
            }
            return _initialized;
        }

        private void LoadConfig()
        {
            string devicetoken = _configStore.GetConfig(DeviceTokenName);
            string usertoken = _configStore.GetConfig(UserTokenName);
            _devicetoken = devicetoken == EmptyToken ? "" : devicetoken;
            _usertoken = usertoken == EmptyToken ? "" : usertoken;
        }

        private void WriteConfig()
        {
            Dictionary<string, string> mapConfigs = new Dictionary<string, string>();
            mapConfigs[DeviceTokenName] = _devicetoken?.Length > 0 ? _devicetoken : EmptyToken;
            mapConfigs[UserTokenName] = _usertoken?.Length > 0 ? _usertoken : EmptyToken;
            _configStore.SetConfigs(mapConfigs);
        }

        private async Task<HttpResponseMessage> Request(HttpMethod method, string url, Dictionary<string, string> header, HttpContent content)
        {
            Logger.Debug($"url is: {url}");
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

            HttpResponseMessage response = await _client.SendAsync(request);
            return response;
        }

        private async Task<bool> RenewToken()
        {
            if (_devicetoken == null || _devicetoken.Length == 0)
            {
                throw new Exception("Please register a device first");
            }

            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Authorization", $"Bearer {_devicetoken}");

            HttpResponseMessage response = await Request(HttpMethod.Post, _userTokenUrl, header, null);
            if (response.IsSuccessStatusCode)
            {
                byte[] responseContent = response.Content.ReadAsByteArrayAsync().Result;
                _usertoken = Encoding.ASCII.GetString(responseContent);
                _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_usertoken}");
                WriteConfig();
                InitializeApiClient();
                Logger.Debug("user token renewed");
                return true;
            }
            else
            {
                Logger.Debug($"Renew session token failed with response code {response.StatusCode}");
                throw new Exception("Can't renew sesion token");
            }
        }

        private List<RmItem> getChildItemsRecursive(string parentId, ref List<RmItem> items)
        {
            var children = (from item in items where item.Parent == parentId select item).ToList();
            foreach (var child in children)
            {
                child.Children = getChildItemsRecursive(child.ID, ref items);
            }
            return children;
        }

        private void InitializeApiClient()
        {
            if (_apiClient != null)
            {
                _apiClient.Dispose();
            }

            try
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jsonToken = handler.ReadToken(_usertoken) as JwtSecurityToken;
                string scopeValue = jsonToken.Claims.First(claim => claim.Type == "scopes").Value;
                var scopeFields = scopeValue.Split(' ');
                var v2ScopeFieldCount = scopeFields.Where(field => (field == "sync:fox" || field == "sync:tortoise" || field == "sync:hare")).ToList().Count;
                if (v2ScopeFieldCount == 0)
                {
                    Logger.Debug("Creating V1 api client");
                    _apiClient = new CloudApiV1Client(_client, _hiddenConfigStore);
                }
                else
                {
                    Logger.Debug("Creating V2 api client");
                    _apiClient = new CloudApiV2Client(_client);
                }
            }
            catch (Exception)
            {
                Logger.Debug($"Unable to determine api version from user token");
                throw new Exception("Unable to determine cloud API version");
            }
        }
    }
}
