using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiSynth7.components
{
    /// <summary>
    /// Scaffolding for a JSON based instrument definition file
    /// </summary>
    public class InstrumentDefinition
    {
        public List<NumberedEntry> Banks { get; set; }
        public List<NumberedEntry> Instruments { get; set; }
        public List<NumberedEntry> Drumkits { get; set; }
    }
}
