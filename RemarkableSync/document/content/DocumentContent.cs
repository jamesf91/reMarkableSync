using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RemarkableSync.document
{
    public enum RmContentFormat
    {
        v1 = 1, 
        v2 = 2
    }
    public class DocumentContent : IDocumentContent
    {
        public int coverPageNumber { get; set; }
        public DocumentMetadata documentMetadata { get; set; }
        public bool dummyDocument { get; set; }
        public Extrametadata extraMetadata { get; set; }
        public string fileType { get; set; }
        public string fontName { get; set; }
        public RmContentFormat formatVersion { get; set; }
        public int lineHeight { get; set; }
        public int margins { get; set; }
        public string orientation { get; set; }
        public int originalPageCount { get; set; }
        public int pageCount { get; set; }
        public object[] pageTags { get; set; }
        public string sizeInBytes { get; set; }
        public object[] tags { get; set; }
        public string textAlignment { get; set; }
        public float textScale { get; set; }

        public virtual List<string> getPages()
        {
            return new List<string>();
        }

        public static DocumentContent GetDocumentContentFromJson(string contentJsonString)
        {
            try
            {
                JsonNode versionCheck = JsonNode.Parse(contentJsonString);                
                RmContentFormat docVersion = RmContentFormat.v1; //Default to v1
                if (versionCheck != null && versionCheck["formatVersion"] != null)
                {
                    docVersion = (RmContentFormat)versionCheck["formatVersion"].GetValue<int>();
                }
            
                switch (docVersion)
                {
                    case RmContentFormat.v1:
                        return JsonSerializer.Deserialize<DocumentContentV1>(contentJsonString);
                    case RmContentFormat.v2:
                    default:
                        return JsonSerializer.Deserialize<DocumentContentV2>(contentJsonString);
                }

            }

            catch (Exception)
            {
                throw new Exception("Unsupported content version");
            }
        }
    }
}



