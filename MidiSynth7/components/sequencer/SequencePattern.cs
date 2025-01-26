using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiSynth7.components.sequencer
{
    public class SequencePattern
    {
        public List<PatternStep> Steps { get; set; }

        public static List<PatternStep> GetEmptySequencePattern()
        {
            PatternStep[] s = new PatternStep[64];
            return s.ToList();
        }

    }
}
