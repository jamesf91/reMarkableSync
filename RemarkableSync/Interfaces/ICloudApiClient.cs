using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemarkableSync
{
    interface ICloudApiClient : IDisposable
    {
        Task<List<RmItem>> GetAllItems();

        Task<RmDownloadedDoc> DownloadDocument(RmItem item);

    }
}
