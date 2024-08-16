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
    class PresetBuilder : StructureBuilder<Preset>
    {
        private Preset lastPreset = null;

        public override Preset Read(BinaryReader br)
        {
            Preset p = new Preset();
            string s = Encoding.UTF8.GetString(br.ReadBytes(20), 0, 20);
            if (s.IndexOf('\0') >= 0)
            {
                s = s.Substring(0, s.IndexOf('\0'));
            }
            p.Name = s;
            p.PatchNumber = br.ReadUInt16();
            p.Bank = br.ReadUInt16();
            p.startPresetZoneIndex = br.ReadUInt16();
            p.library = br.ReadUInt32();
            p.genre = br.ReadUInt32();
            p.morphology = br.ReadUInt32();
            if (lastPreset != null)
                lastPreset.endPresetZoneIndex = (ushort)(p.startPresetZoneIndex - 1);
            data.Add(p);
            lastPreset = p;
            return p;
        }

        public override void Write(BinaryWriter bw, Preset preset)
        {
        }

        public override int Length => 38;

        public void LoadZones(Zone[] presetZones)
        {
            // don't do the last preset, which is simply EOP
            for (int preset = 0; preset < data.Count - 1; preset++)
            {
                Preset p = data[preset];
                p.Zones = new Zone[p.endPresetZoneIndex - p.startPresetZoneIndex + 1];
                Array.Copy(presetZones, p.startPresetZoneIndex, p.Zones, 0, p.Zones.Length);
            }
            // we can get rid of the EOP record now
            data.RemoveAt(data.Count - 1);
        }

        public Preset[] Presets => data.ToArray();
    }
}