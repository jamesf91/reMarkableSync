using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemarkableSync
{
    public interface IConfigStore : IDisposable
    {
        string GetConfig(string key);

        bool SetConfigs(Dictionary<string, string> configs);
    }
}
