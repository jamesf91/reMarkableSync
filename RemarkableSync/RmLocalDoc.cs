using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RemarkableSync
{
    class RmLocalDoc : RmDocument
    {
        private V2HttpHelper _httpHelper;
        private object _taskProgress;

        public RmLocalDoc(string id) : base(id)
        {
            LoadDocumentContent();
        }

        public RmLocalDoc(string id, string root_path) : base(id, root_path)
        {
            LoadDocumentContent();
        }
    }
}