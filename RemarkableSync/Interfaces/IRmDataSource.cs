using RemarkableSync.document;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RemarkableSync
{
    public interface IRmDataSource: IDisposable
    {
        Task<List<RmItem>> GetItemHierarchy(CancellationToken cancellationToken, IProgress<string> progress);

        Task<RmDocument> DownloadDocument(string ID, CancellationToken cancellationToken, IProgress<string> progress);
    }
}
