using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiSynth7.components
{
    public class NFXDelayProfile
    {
        public string ProfileName { get; set; }
        /// <summary>
        /// Echo delay in ms
        /// </summary>
        public int Delay { get; set; }
        /// <summary>
        /// Property used to determine both octave, and note count.
        /// </summary>
        public int[] OffsetMap { get; set; }
        /// <summary>
        /// Decrease relative volume per note by x%
        /// </summary>
        public int FalloffPercentage { get; set; }
        /// <summary>
        /// Reverse Offset map
        /// </summary>
        public bool Reverse { get; set; }

    }
}
