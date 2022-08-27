using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemarkableSync
{
    class CloudApiV2Client : ICloudApiClient
    {
        private HttpClient _client;
        private V2DocTreeProcessor _docTreeProcessor;

        public CloudApiV2Client(HttpClient client)
        {
            _client = client;
            _docTreeProcessor = new V2DocTreeProcessor(client);
        }

        public void Dispose()
        {
        }

        public async Task<RmDownloadedDoc> DownloadDocument(RmItem item, CancellationToken cancellationToken, IProgress<string> progress)
        {
            var fileList = _docTreeProcessor.GetFileListForDocument(item.ID);

            return await Task.Run(() =>
            {
                return new RmCloudV2DownloadedDoc(item.ID, fileList, _client, cancellationToken, progress);
            });
        }

        public async Task<List<RmItem>> GetAllItems(CancellationToken cancellationToken, IProgress<string> progress)
        {
            progress.Report("Checking document list for changes");
            await _docTreeProcessor.SyncTreeAsync(cancellationToken, progress);
            return _docTreeProcessor.GetAllItems();
        }
    }
}
