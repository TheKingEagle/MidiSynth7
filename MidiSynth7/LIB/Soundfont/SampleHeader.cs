/*
 Copyright 2020 Mark Heath

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

 */
namespace NAudio.SoundFont
{
    /// <summary>
    /// A SoundFont Sample Header
    /// </summary>
    public class SampleHeader
    {
        /// <summary>
        /// The sample name
        /// </summary>
        public string SampleName;
        /// <summary>
        /// Start offset
        /// </summary>
        public uint Start;
        /// <summary>
        /// End offset
        /// </summary>
        public uint End;
        /// <summary>
        /// Start loop point
        /// </summary>
        public uint StartLoop;
        /// <summary>
        /// End loop point
        /// </summary>
        public uint EndLoop;
        /// <summary>
        /// Sample Rate
        /// </summary>
        public uint SampleRate;
        /// <summary>
        /// Original pitch
        /// </summary>
        public byte OriginalPitch;
        /// <summary>
        /// Pitch correction
        /// </summary>
        public sbyte PitchCorrection;
        /// <summary>
        /// Sample Link
        /// </summary>
        public ushort SampleLink;
        /// <summary>
        /// SoundFont Sample Link Type
        /// </summary>
        public SFSampleLink SFSampleLink;

        /// <summary>
        /// <see cref="object.ToString"/>
        /// </summary>
        public override string ToString() => SampleName;

    }
}

