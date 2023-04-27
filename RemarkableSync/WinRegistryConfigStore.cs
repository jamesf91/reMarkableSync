using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RemarkableSync
{
    public class WinRegistryConfigStore : IConfigStore
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private string _regKeyPath;
        private bool _encrypt;

        public WinRegistryConfigStore(string regKeyPath, bool encrypt = true)
        {
            _regKeyPath = regKeyPath;
            _encrypt = encrypt;
        }

        public string GetConfig(string configName)
        {
            string regValue = null; ;
            try
            {
                var settingsKey = Registry.CurrentUser.OpenSubKey(_regKeyPath);
                regValue = (string)settingsKey.GetValue(configName, null);
            }
            catch (Exception err)
            {
                Logger.Error($"Failed to access registry for config \"{configName}\". Error: {err.Message}");
                return null;
            }

            if (regValue == null)
            {
                Logger.Error($"Getting value from registry for config \"{configName}\" failed");
                return null;
            }

            try
            {
                if (_encrypt)
                {
                    return Encoding.UTF8.GetString(DecryptData(Convert.FromBase64String(regValue)));
                }
                else
                {
                    return regValue;
                }
            }
            catch (Exception err)
            {
                Logger.Error($"Failed to get config \"{configName}\". Error: {err.Message}");
                return null;
            }           
        }

        public bool SetConfigs(Dictionary<string, string> configs)
        {
            List<KeyValuePair<string, string>> processedConfigs = null;
            if (_encrypt)
            {
                try
                {
                    // to byte[], encrypt, then to base 64 string
                    var rawData = from config in configs
                                  select new KeyValuePair<string, string>(
                                      config.Key,
                                      Convert.ToBase64String(EncryptData(Encoding.UTF8.GetBytes(config.Value))));
                    processedConfigs = rawData.ToList();
                }
                catch (Exception err)
                {
                    Logger.Error($"Failed to encrypt all configs. Error: {err.Message}");
                    return false;
                }
            }
            else
            {
                processedConfigs = (from config in configs
                                    select new KeyValuePair<string, string>(config.Key, config.Value)).ToList();
            }

            try
            {
                Registry.CurrentUser.CreateSubKey(_regKeyPath);
                var settingsKey = Registry.CurrentUser.OpenSubKey(_regKeyPath, true);
                foreach (var processedConfig in processedConfigs)
                {
                    settingsKey.SetValue(processedConfig.Key, processedConfig.Value);
                }
            }
            catch (Exception err)
            {
                Logger.Error($"Failed to write to registry. Error: {err.Message}");
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            return;
        }

        private byte[] EncryptData(byte[] rawData)
        {
            try
            {
                return ProtectedData.Protect(rawData, null, DataProtectionScope.LocalMachine);
            }
            catch (Exception err)
            {
                Logger.Error("Encrypt failed with err: " + err.Message);
                throw err;
            }
        }

        private byte[] DecryptData(byte[] encryptedData)
        {
            try
            {
                return ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.LocalMachine);
            }
            catch (Exception err)
            {
                Logger.Error("Encrypt failed with err: " + err.Message);
                throw err;
            }
        }
    }
}
