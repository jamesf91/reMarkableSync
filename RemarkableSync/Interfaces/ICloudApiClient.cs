using RemarkableSync.document;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RemarkableSync
{
    interface ICloudApiClient : IDisposable
    {
        Task<List<RmItem>> GetAllItems(CancellationToken cancellationToken, IProgress<string> progress);

        Task<RmDocument> DownloadDocument(string ID, CancellationToken cancellationToken, IProgress<string> progress);

    }
}
