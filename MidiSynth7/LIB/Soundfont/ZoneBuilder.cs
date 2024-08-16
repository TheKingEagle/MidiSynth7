using System;
using System.IO;
/*
 Copyright 2020 Mark Heath

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

 */
namespace NAudio.SoundFont
{
    class ZoneBuilder : StructureBuilder<Zone>
    {
        private Zone lastZone = null;

        public override Zone Read(BinaryReader br)
        {
            Zone z = new Zone();
            z.generatorIndex = br.ReadUInt16();
            z.modulatorIndex = br.ReadUInt16();
            if (lastZone != null)
            {
                lastZone.generatorCount = (ushort)(z.generatorIndex - lastZone.generatorIndex);
                lastZone.modulatorCount = (ushort)(z.modulatorIndex - lastZone.modulatorIndex);
            }
            data.Add(z);
            lastZone = z;
            return z;
        }

        public override void Write(BinaryWriter bw, Zone zone)
        {
            //bw.Write(p.---);
        }

        public void Load(Modulator[] modulators, Generator[] generators)
        {
            // don't do the last zone, which is simply EOZ
            for (int zone = 0; zone < data.Count - 1; zone++)
            {
                Zone z = (Zone)data[zone];
                z.Generators = new Generator[z.generatorCount];
                Array.Copy(generators, z.generatorIndex, z.Generators, 0, z.generatorCount);
                z.Modulators = new Modulator[z.modulatorCount];
                Array.Copy(modulators, z.modulatorIndex, z.Modulators, 0, z.modulatorCount);
            }
            // we can get rid of the EOP record now
            data.RemoveAt(data.Count - 1);
        }

        public Zone[] Zones => data.ToArray();

        public override int Length => 4;
    }
}