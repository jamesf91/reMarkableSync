using System;
using System.IO;
using System.Text;

namespace RemarkableSync.document
{
    public class TaggedBinaryReader : BinaryReader
    {
        //Tag type representing the type of following data.
        public enum TagType : ushort
        {
            ID = 0xF,
            Length4 = 0xC,
            Byte8 = 0x8,
            Byte4 = 0x4,
            Byte1 = 0x1
        }
        public TaggedBinaryReader(Stream input) : base(input)
        {
        }

        public TaggedBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        public TaggedBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        public int ReadVarUint()
        {
            int shift = 0;
            int result = 0;

            while (true)
            {
                int i = Convert.ToInt32(ReadByte());
                result |= (i & 0x7F) << shift;
                shift += 7;
                if ((i & 0x80) == 0)
                {
                    break;
                }
            }

            return result;
        }

        public byte[] ReadSubBlock(int index)
        {
            ReadTag(index, TagType.Length4);
            int subblockLength = (int)ReadUInt32();

            return ReadBytes(subblockLength);
        }

        public TaggedBinaryReader GetSubBlockAsBinaryReader(int index)
        {
            byte[] subblock = ReadSubBlock(index);
            return new TaggedBinaryReader(new MemoryStream(subblock));
        }

        public CrdtId ReadTaggedId(int index)
        {
            ReadTag(index, TagType.ID);

            return ReadId();
        }

        public CrdtId ReadId()
        {
            int part1 = (int)ReadByte();
            int part2 = ReadVarUint();


            return new CrdtId(part1, part2);
        }

        public (int, TagType) ReadTag(int index, TagType expected_type)
        {
            int x = ReadVarUint();

            // First part is an index number that identifies if this is the right
            // data we're expecting
            var tagIndex = x >> 4;

            // Second part is a tag type that identifies what kind of data it is
            var tagType = (TagType)(x & 0xF);


            if (tagIndex != index)
            {
                throw new Exception("Unexpected block index");
            }
            if (tagType != expected_type)
            {
                throw new Exception("Unexpected subblock tag type");
            }

            return (tagIndex, tagType);
        }

        public bool HasSubblock(int index)
        {
            return CheckTag(index, TagType.Length4);
        }

        /**
         * Check that INDEX and TAG_TYPE are next.
         * 
         * Returns True if the expected index and tag type are found. Does not
         * advance the stream.
         */
        public bool CheckTag(int index, TagType expected_type)
        {
            long pos = BaseStream.Position;
            try
            {
                ReadTag(index, expected_type); 
                
                //throws error when not expected
                return true;
            }catch (Exception e)
            {
                return false;
            }
            finally { 
                BaseStream.Position = pos; 
            }
        }



        public int ReadTaggedUInt32(int index)
        {
            ReadTag(index, TagType.Byte4);
            return (int)ReadUInt32();
        }
        public bool ReadTaggedBool(int index)
        {
            ReadTag(index, TagType.Byte1);
            return ReadBoolean();
        }

        public byte ReadTaggedByte(int index)
        {
            ReadTag(index, TagType.Byte1);
            return ReadByte();
        }

        public float ReadTaggedSingle(int index)
        {
            ReadTag(index, TagType.Byte4);
            return ReadSingle();
        }

        public double ReadTaggedDouble(int index)
        {
            ReadTag(index, TagType.Byte8);
            return ReadDouble();
        }

        public String ReadString(int index)
        {
            using (TaggedBinaryReader subBlock = GetSubBlockAsBinaryReader(index))
            {
                int length = subBlock.ReadVarUint();
                bool is_ascii = subBlock.ReadBoolean();

                return new String(subBlock.ReadChars(length));
            }
        }

        public (String, int) ReadStringWithFormat(int index)
        {
            using(TaggedBinaryReader subBlock = GetSubBlockAsBinaryReader(index)){
                int length = subBlock.ReadVarUint();
                bool is_ascii = subBlock.ReadBoolean();

                String s = new String(subBlock.ReadChars(length));
                int format = -1;
                try
                {
                    format = subBlock.ReadTaggedUInt32(2);
                }catch (Exception e){
                    
                }
                
                return (s, format);
            }            
        }

        public LwwValue<bool> ReadLwwBool(int index)
        {
            using (TaggedBinaryReader subBlock = GetSubBlockAsBinaryReader(index))
            {
                return new LwwValue<bool>(
                    subBlock.ReadTaggedId(1),
                    subBlock.ReadTaggedBool(2)
                );
            }
        }

        public LwwValue<byte> ReadLwwByte(int index)
        {
            using (TaggedBinaryReader subBlock = GetSubBlockAsBinaryReader(index))
            {
                return new LwwValue<byte>(
                    subBlock.ReadTaggedId(1),
                    subBlock.ReadTaggedByte(2)
                );
            }
        }

        public LwwValue<int> ReadLwwInt(int index)
        {
            using (TaggedBinaryReader subBlock = GetSubBlockAsBinaryReader(index))
            {
                return new LwwValue<int>(
                    subBlock.ReadTaggedId(1),
                    subBlock.ReadTaggedByte(2)
                );
            }
        }

        public LwwValue<float> ReadLwwFloat(int index)
        {
            using (TaggedBinaryReader subBlock = GetSubBlockAsBinaryReader(index))
            {
                return new LwwValue<float>(
                    subBlock.ReadTaggedId(1),
                    subBlock.ReadTaggedSingle(2)
                );
            }
        }
        public LwwValue<double> ReadLwwDouble(int index)
        {
            using (TaggedBinaryReader subBlock = GetSubBlockAsBinaryReader(index))
            {
                return new LwwValue<double>(
                    subBlock.ReadTaggedId(1),
                    subBlock.ReadTaggedDouble(2)
                );
            }
        }

        public LwwValue<CrdtId> ReadLwwId(int index)
        {
            using (TaggedBinaryReader subBlock = GetSubBlockAsBinaryReader(index))
            {
                return new LwwValue<CrdtId>(
                    subBlock.ReadTaggedId(1),
                    subBlock.ReadTaggedId(2)
                );
            }
        }
        public LwwValue<string> ReadLwwString(int index)
        {
            using (TaggedBinaryReader subBlock = GetSubBlockAsBinaryReader(index))
            {
                return new LwwValue<string>(
                    subBlock.ReadTaggedId(1),
                    subBlock.ReadString(2)
                );
            }
        }
    }
}
