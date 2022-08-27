using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RemarkableSync
{
    interface ICloudApiClient : IDisposable
    {
        Task<List<RmItem>> GetAllItems(CancellationToken cancellationToken, IProgress<string> progress);

        Task<RmDownloadedDoc> DownloadDocument(RmItem item, CancellationToken cancellationToken, IProgress<string> progress);

    }
}
