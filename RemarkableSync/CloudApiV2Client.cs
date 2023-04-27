using RemarkableSync.document;
using System;
using System.Collections.Generic;
using System.Net.Http;
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

        public async Task<RmDocument> DownloadDocument(string ID, CancellationToken cancellationToken, IProgress<string> progress)
        {
            var fileList = _docTreeProcessor.GetFileListForDocument(ID);

            return await Task.Run(() =>
            {
                return new RmCloudV2DownloadedDoc(ID, fileList, _client, cancellationToken, progress);
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
