using System;
/*
 Copyright 2020 Mark Heath

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

 */
namespace NAudio.SoundFont
{
    /// <summary>
    /// A SoundFont Preset
    /// </summary>
    public class Preset
    {
        internal ushort startPresetZoneIndex;
        internal ushort endPresetZoneIndex;
        internal uint library;
        internal uint genre;
        internal uint morphology;

        /// <summary>
        /// Preset name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Patch Number
        /// </summary>
        public ushort PatchNumber { get; set; }

        /// <summary>
        /// Bank number
        /// 0 - 127, GM percussion bank is 128
        /// </summary>
        public ushort Bank { get; set; }

        /// <summary>
        /// Zones
        /// </summary>
        public Zone[] Zones { get; set; }

        /// <summary>
        /// <see cref="Object.ToString"/>
        /// </summary>
        public override string ToString()
        {
            return $"{Bank}-{PatchNumber} {Name}";
        }
    }
}