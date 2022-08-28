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
        /// Property used to determine pitch and velocity at step.
        /// </summary>
        public (int pitch,int decay)[] OffsetMap { get; set; }
        /// <summary>
        /// Reverse Offset map
        /// </summary>
        public bool Reverse { get; set; }

    }
}
