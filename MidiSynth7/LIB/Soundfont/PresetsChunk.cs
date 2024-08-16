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
	/// Class to read the SoundFont file presets chunk
	/// </summary>
	public class PresetsChunk 
	{
		private PresetBuilder presetHeaders = new PresetBuilder();
		private ZoneBuilder presetZones = new ZoneBuilder();
		private ModulatorBuilder presetZoneModulators = new ModulatorBuilder();
		private GeneratorBuilder presetZoneGenerators = new GeneratorBuilder();
		private InstrumentBuilder instruments = new InstrumentBuilder();
		private ZoneBuilder instrumentZones = new ZoneBuilder();
		private ModulatorBuilder instrumentZoneModulators = new ModulatorBuilder();
		private GeneratorBuilder instrumentZoneGenerators = new GeneratorBuilder();
		private SampleHeaderBuilder sampleHeaders = new SampleHeaderBuilder();

        internal PresetsChunk(RiffChunk chunk)
        {
            string header = chunk.ReadChunkID();

            if (header != "pdta")
            {
                throw new InvalidDataException($"Not a presets data chunk ({header})");
            }

            RiffChunk c;
            while ((c = chunk.GetNextSubChunk()) != null)
            {
                // Log chunk IDs to diagnose why it takes so long and where it fails
                Console.WriteLine($"Processing chunk: {c.ChunkID}");

                switch (c.ChunkID.ToLower())
                {
                    case "phdr":
                        c.GetDataAsStructureArray(presetHeaders);
                        break;
                    case "pbag":
                        c.GetDataAsStructureArray(presetZones);
                        break;
                    case "pmod":
                        c.GetDataAsStructureArray(presetZoneModulators);
                        break;
                    case "pgen":
                        c.GetDataAsStructureArray(presetZoneGenerators);
                        break;
                    case "inst":
                        c.GetDataAsStructureArray(instruments);
                        break;
                    case "ibag":
                        c.GetDataAsStructureArray(instrumentZones);
                        break;
                    case "imod":
                        c.GetDataAsStructureArray(instrumentZoneModulators);
                        break;
                    case "igen":
                        c.GetDataAsStructureArray(instrumentZoneGenerators);
                        break;
                    case "shdr":
                        c.GetDataAsStructureArray(sampleHeaders);
                        break;
                    default:
                        throw new InvalidDataException($"Unknown chunk type {c.ChunkID}");
                }
            }

            // now link things up
            instrumentZoneGenerators.Load(sampleHeaders.SampleHeaders);
            instrumentZones.Load(instrumentZoneModulators.Modulators, instrumentZoneGenerators.Generators);
            instruments.LoadZones(instrumentZones.Zones);
            presetZoneGenerators.Load(instruments.Instruments);
            presetZones.Load(presetZoneModulators.Modulators, presetZoneGenerators.Generators);
            presetHeaders.LoadZones(presetZones.Zones);
            sampleHeaders.RemoveEOS();
        }


        /// <summary>
        /// The Presets contained in this chunk
        /// </summary>
        public Preset[] Presets => presetHeaders.Presets;

        /// <summary>
        /// The instruments contained in this chunk
        /// </summary>
        public Instrument[] Instruments => instruments.Instruments;

        /// <summary>
        /// The sample headers contained in this chunk
        /// </summary>
        public SampleHeader[] SampleHeaders => sampleHeaders.SampleHeaders;

        /// <summary>
        /// <see cref="Object.ToString"/>
        /// </summary>
        public override string ToString() 
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Preset Headers:\r\n");
			foreach(Preset p in presetHeaders.Presets) {
				sb.AppendFormat("{0}\r\n",p);
			}
			sb.Append("Instruments:\r\n");
			foreach(Instrument i in instruments.Instruments) {
				sb.AppendFormat("{0}\r\n",i);
			}
			return sb.ToString();
		}
	}

}