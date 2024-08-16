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
    class SampleHeaderBuilder : StructureBuilder<SampleHeader>
    {
        public override SampleHeader Read(BinaryReader br)
        {
            SampleHeader sh = new SampleHeader();
            var s = br.ReadBytes(20);

            sh.SampleName = ByteEncoding.Instance.GetString(s, 0, s.Length);
            sh.Start = br.ReadUInt32();
            sh.End = br.ReadUInt32();
            sh.StartLoop = br.ReadUInt32();
            sh.EndLoop = br.ReadUInt32();
            sh.SampleRate = br.ReadUInt32();
            sh.OriginalPitch = br.ReadByte();
            sh.PitchCorrection = br.ReadSByte();
            sh.SampleLink = br.ReadUInt16();
            sh.SFSampleLink = (SFSampleLink)br.ReadUInt16();
            data.Add(sh);
            return sh;
        }

        public override void Write(BinaryWriter bw, SampleHeader sampleHeader)
        {
        }

        public override int Length => 46;

        internal void RemoveEOS()
        {
            data.RemoveAt(data.Count - 1);
        }

        public SampleHeader[] SampleHeaders => data.ToArray();
    }
}