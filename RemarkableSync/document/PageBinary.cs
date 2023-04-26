using RemarkableSync.document.v5;
using RemarkableSync.document.v6;
using RemarkableSync.MyScript;
using System;
using System.Drawing;
using System.IO;

namespace RemarkableSync.document
{
    enum RmFileVersion
    {
        V5 = 5,
        V6 = 6
    }
    public class PageBinary
    {
        private Stream _document;
        private TaggedBinaryReader _reader;
        private RmFileVersion _doc_version;
        private IRmPageBinary _rmPageBinary;


        public PageBinary(String path)
        {
            if (File.Exists(path))
            {
                using (Stream document = File.Open(path, FileMode.Open))
                {
                    _document = document;
                    _reader = new TaggedBinaryReader(_document);
                    ReadBinaryDocument();
                }
            }
            else
            {
                //default create empty page
                _rmPageBinary = new RmPageBinaryV6();
            }
            
        }

        public PageBinary(Stream document) 
        { 
            _document = document;
            _reader = new TaggedBinaryReader(_document);
            ReadBinaryDocument();
        }

        public Bitmap GetBitmap()
        {
            return _rmPageBinary.GetBitmap();
        }

        public Tuple<StrokeGroup, BoundingBox> GetMyScriptFormat()
        {
            return _rmPageBinary.GetMyScriptFormat();
        }
        private void ReadBinaryDocument()
        {
            ReadHeader();

            switch (_doc_version)
            {
                case RmFileVersion.V5:
                    _rmPageBinary = RmPage.ParseStream(_reader);
                    break;
                case RmFileVersion.V6:
                    _rmPageBinary = new RmPageBinaryV6(ref _reader);
                    break;
            }
            //Logger.Debug("Not processing file version " + _doc_version);
        }


        /**
         * Read header 43 bytes
         *
         * V6 "reMarkable .lines file, version=6          "
         * V5 "reMarkable .lines file, version=5          "
         */
        private void ReadHeader()
        {
            String header = new String(_reader.ReadChars(43));

            if (header.Contains("version=6"))
            {
                _doc_version = RmFileVersion.V6;
            } else if(header.Contains("version=5"))
            {
                _doc_version = RmFileVersion.V5;
            } else
            {
                throw new Exception("Unsupported file version: " + header);
            }
        }
    }
}
