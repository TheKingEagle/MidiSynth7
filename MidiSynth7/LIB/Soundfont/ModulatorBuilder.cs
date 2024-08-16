using System.IO;
/*
 Copyright 2020 Mark Heath

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

 */
namespace NAudio.SoundFont
{
    class ModulatorBuilder : StructureBuilder<Modulator>
    {
        public override Modulator Read(BinaryReader br)
        {
            Modulator m = new Modulator();
            m.SourceModulationData = new ModulatorType(br.ReadUInt16());
            m.DestinationGenerator = (GeneratorEnum)br.ReadUInt16();
            m.Amount = br.ReadInt16();
            m.SourceModulationAmount = new ModulatorType(br.ReadUInt16());
            m.SourceTransform = (TransformEnum)br.ReadUInt16();
            data.Add(m);
            return m;
        }

        public override void Write(BinaryWriter bw, Modulator o)
        {
            //Zone z = (Zone) o;
            //bw.Write(p.---);
        }

        public override int Length => 10;

        public Modulator[] Modulators => data.ToArray();
    }
}