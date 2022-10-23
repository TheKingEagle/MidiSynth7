using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiSynth7.components.seqparams
{
    internal class S : ISeqParamPlayer
    {
        public char Mark => 'S';

        
        public void Play(SeqData passthroughData, MidiEngine engine, TrackerSequence Sequence)
        {
            // D0-DF: Values >= 208 <= 223 - Set Note delay
            // 
            // NB: SDX values are technically limited to the sequence's TPR (Ticks per row).
            //  For example, If the TPR is 3, and you set SD3 for a note, that note should not play.

            // F0-FF: Values >= 240 <= 255
            throw new NotImplementedException();
        }
    }
}
