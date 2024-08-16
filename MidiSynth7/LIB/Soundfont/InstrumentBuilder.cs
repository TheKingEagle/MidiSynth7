using System;
using System.IO;
using System.Text;
/*
 Copyright 2020 Mark Heath

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

 */
namespace NAudio.SoundFont
{
    /// <summary>
    /// Instrument Builder
    /// </summary>
    internal class InstrumentBuilder : StructureBuilder<Instrument>
    {
        private Instrument lastInstrument = null;

        public override Instrument Read(BinaryReader br)
        {
            Instrument i = new Instrument();
            string s = Encoding.UTF8.GetString(br.ReadBytes(20), 0, 20);
            if (s.IndexOf('\0') >= 0)
            {
                s = s.Substring(0, s.IndexOf('\0'));
            }
            i.Name = s;
            i.startInstrumentZoneIndex = br.ReadUInt16();
            if (lastInstrument != null)
            {
                lastInstrument.endInstrumentZoneIndex = (ushort)(i.startInstrumentZoneIndex - 1);
            }
            data.Add(i);
            lastInstrument = i;
            return i;
        }

        public override void Write(BinaryWriter bw, Instrument instrument)
        {
        }

        public override int Length => 22;

        public void LoadZones(Zone[] zones)
        {
            // don't do the last preset, which is simply EOP
            for (int instrument = 0; instrument < data.Count - 1; instrument++)
            {
                Instrument i = data[instrument];
                i.Zones = new Zone[i.endInstrumentZoneIndex - i.startInstrumentZoneIndex + 1];
                Array.Copy(zones, i.startInstrumentZoneIndex, i.Zones, 0, i.Zones.Length);
            }
            // we can get rid of the EOP record now
            data.RemoveAt(data.Count - 1);
        }

        public Instrument[] Instruments => data.ToArray();
    }
}