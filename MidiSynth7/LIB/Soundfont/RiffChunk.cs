using System;
using System.IO;
using NAudio.Utils;
/*
 Copyright 2020 Mark Heath

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

 */
namespace NAudio.SoundFont
{
    internal class RiffChunk
    {
        private string chunkID;
        private BinaryReader riffFile;

        public static RiffChunk GetTopLevelChunk(BinaryReader file)
        {
            RiffChunk r = new RiffChunk(file);
            r.ReadChunk();
            return r;
        }

        private RiffChunk(BinaryReader file)
        {
            riffFile = file;
            chunkID = "????";
            ChunkSize = 0;
            DataOffset = 0;
        }

        /// <summary>
        /// just reads a chunk ID at the current position
        /// </summary>
        /// <returns>chunk ID</returns>
        public string ReadChunkID()
        {
            byte[] cid = riffFile.ReadBytes(4);
            if (cid.Length != 4)
            {
                throw new InvalidDataException("Couldn't read Chunk ID");
            }
            return ByteEncoding.Instance.GetString(cid, 0, cid.Length);
        }

        /// <summary>
        /// reads a chunk at the current position
        /// </summary>
        private void ReadChunk()
        {
            this.chunkID = ReadChunkID();
            this.ChunkSize = riffFile.ReadUInt32(); //(uint) IPAddress.NetworkToHostOrder(riffFile.ReadUInt32());
            this.DataOffset = riffFile.BaseStream.Position;
        }

        /// <summary>
        /// creates a new riffchunk from current position checking that we're not
        /// at the end of this chunk first
        /// </summary>
        /// <returns>the new chunk</returns>
        public RiffChunk GetNextSubChunk()
        {
            if (riffFile.BaseStream.Position + 8 < DataOffset + ChunkSize)
            {
                RiffChunk chunk = new RiffChunk(riffFile);
                chunk.ReadChunk();
                return chunk;
            }
            //Console.WriteLine("DEBUG Failed to GetNextSubChunk because Position is {0}, dataOffset{1}, chunkSize {2}",riffFile.BaseStream.Position,dataOffset,chunkSize);
            return null;
        }

        public byte[] GetData()
        {
            riffFile.BaseStream.Position = DataOffset;

            // Make sure that ChunkSize can handle very large values
            long chunkSize = ChunkSize;

            // If the chunk size is larger than int.MaxValue, we need to handle it differently
            if (chunkSize > int.MaxValue)
            {
                // TKE: Since I am only interested in the presets, I can skip this
                riffFile.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                return new byte[] { (byte)0};
            }
            else
            {
                // For smaller chunks, read all at once
                byte[] data = riffFile.ReadBytes((int)chunkSize);

                if (data.Length != chunkSize)
                {
                    throw new InvalidDataException(String.Format("Data length mismatch: Expected {0}, read {1}", chunkSize, data.Length));
                }

                return data;
            }
        }


        /// <summary>
        /// useful for chunks that just contain a string
        /// </summary>
        /// <returns>chunk as string</returns>
        public string GetDataAsString()
        {
            byte[] data = GetData();
            if (data == null)
                return null;
            return ByteEncoding.Instance.GetString(data, 0, data.Length);
        }

        public T GetDataAsStructure<T>(StructureBuilder<T> s)
        {
            riffFile.BaseStream.Position = DataOffset;
            if (s.Length != ChunkSize)
            {
                throw new InvalidDataException(String.Format("Chunk size is: {0} so can't read structure of: {1}", ChunkSize, s.Length));
            }
            return s.Read(riffFile);
        }

        public T[] GetDataAsStructureArray<T>(StructureBuilder<T> s)
        {
            riffFile.BaseStream.Position = DataOffset;
            if (ChunkSize % s.Length != 0)
            {
                throw new InvalidDataException(String.Format("Chunk size is: {0} not a multiple of structure size: {1}", ChunkSize, s.Length));
            }
            int structuresToRead = (int)(ChunkSize / s.Length);
            T[] a = new T[structuresToRead];
            for (int n = 0; n < structuresToRead; n++)
            {
                a[n] = s.Read(riffFile);
            }
            return a;
        }

        public string ChunkID
        {
            get
            {
                return chunkID;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("ChunkID may not be null");
                }
                if (value.Length != 4)
                {
                    throw new ArgumentException("ChunkID must be four characters");
                }
                chunkID = value;
            }
        }

        public uint ChunkSize { get; private set; }

        public long DataOffset { get; private set; }

        public override string ToString()
        {
            return String.Format("RiffChunk ID: {0} Size: {1} Data Offset: {2}", ChunkID, ChunkSize, DataOffset);
        }

    }

}
