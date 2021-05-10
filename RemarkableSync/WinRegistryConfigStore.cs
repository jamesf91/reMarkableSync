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
        private string _regKeyPath;

        public WinRegistryConfigStore(string regKeyPath)
        {
            _regKeyPath = regKeyPath;
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
                Console.WriteLine($"WinRegistryConfigStore::GetConfig() - Failed to access registry for config \"{configName}\". Error: {err.Message}");
                return null;
            }

            if (regValue == null)
            {
                Console.WriteLine($"WinRegistryConfigStore::GetConfig() - Getting value from registry for config \"{configName}\" failed");
                return null;
            }

            try
            {
                return Encoding.UTF8.GetString(DecryptData(Convert.FromBase64String(regValue)));
            }
            catch (Exception err)
            {
                Console.WriteLine($"WinRegistryConfigStore::GetConfig() - Failed to get config \"{configName}\". Error: {err.Message}");
                return null;
            }           
        }

        public bool SetConfigs(Dictionary<string, string> configs)
        {
            List<KeyValuePair<string, string>> encryptedConfigs = null;
            try
            {
                // to byte[], encrypt, then to base 64 string
                var rawData = from config in configs
                              select new KeyValuePair<string, string>(
                                  config.Key,
                                  Convert.ToBase64String(EncryptData(Encoding.UTF8.GetBytes(config.Value))));
                encryptedConfigs = rawData.ToList();
            }
            catch (Exception err)
            {
                Console.WriteLine($"WinRegistryConfigStore::SetConfigs() - Failed to encrypt all configs. Error: {err.Message}");
                return false;
            }

            try
            {
                Registry.CurrentUser.CreateSubKey(_regKeyPath);
                var settingsKey = Registry.CurrentUser.OpenSubKey(_regKeyPath, true);
                foreach (var encryptedConfig in encryptedConfigs)
                {
                    settingsKey.SetValue(encryptedConfig.Key, encryptedConfig.Value);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine($"WinRegistryConfigStore::SetConfigs() - Failed to write to registry. Error: {err.Message}");
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
                Console.WriteLine("WinRegistryConfigStore::EncryptData() - Encrupt failed with err: " + err.Message);
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
                Console.WriteLine("WinRegistryConfigStore::DecryptData() - Encrupt failed with err: " + err.Message);
                throw err;
            }
        }
    }
}
