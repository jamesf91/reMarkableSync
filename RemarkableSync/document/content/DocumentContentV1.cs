using System.Collections.Generic;
using System.Linq;

namespace RemarkableSync.document
{
    public class DocumentContentV1 : DocumentContent
    {
        public string[] pages { get; set; }

        public override List<string> getPages()
        {
            return pages.ToList();
        }
    }
}



