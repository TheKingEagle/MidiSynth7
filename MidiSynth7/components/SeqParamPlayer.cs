using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiSynth7.components
{
    /*
     * Planned to support: Axx - Set Ticks per row
     *                     Bxx - Jump to row 0 of pattern X
     *                     Cxx - Break to ROW X of next pattern
     *                     SBx - Loop Pattern X times between two loop points (SB0 -> Start loop at position; SBX 1-F, 1 to 15 times)
     *                     SDx - Delay note X ticks (NB: Limited to TPR defined by AXX)
     *                     SFx - Set sequence's active Zxx Macro
     *                     Zxx - Set active macro's value Range: 00-7F (0-127)
     *
     * Eventually include: Jxx - Note Arpegio; IF I can find a good mech
     *                     SEx - Pattern delay (in rows), IF I can be mature
     *
     */
    public interface ISeqParamPlayer
    {
        
         
        char Mark { get; }

        /// <summary>
        /// Play through the parameter, allowing processing of parameter data
        /// </summary>
        /// <param name="passthroughData">data to pass through to this parameter</param>
        /// <param name="engine">the midi engine for all the things</param>
        /// <param name="Sequence">The calling sequence</param>
        void Play(SeqData passthroughData, MidiEngine engine, TrackerSequence Sequence);
    }
}
