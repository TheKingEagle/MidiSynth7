using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiSynth7.components.sequencer
{
    public class Sequence
    {
        public string SequenceName { get; set; }
        public int Tempo { get; set; }
        public int Divisions { get; set; }
        public int Measures { get; set; }
        public int NotesPerMeasure { get; set; }

        public List<SequencePattern> Patterns { get; set; }

        public override string ToString()
        {
            return SequenceName;
        }
    }
}
