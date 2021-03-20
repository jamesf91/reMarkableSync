using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemarkableSync
{
    public interface IRmDataSource: IDisposable
    {
        Task<List<RmItem>> GetItemHierarchy();

        Task<RmDownloadedDoc> DownloadDocument(RmItem item);
    }
}
