using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanford.Multimedia.Midi;
namespace MidiSynth7.components.sequencer
{
    public class PatternStep
    {
        public List<ChannelMessage> MidiMessages { get; set; }
    }
}
