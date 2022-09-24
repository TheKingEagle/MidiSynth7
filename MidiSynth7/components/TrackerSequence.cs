using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanford.Multimedia.Midi;
namespace MidiSynth7.components
{
   /// <summary>
   /// GOALS: 
   ///  - Compatibility with OpenMPT's standard (At least mostly)
   ///      - Ability to copy-paste track data
   ///      - Ability to understand controller macros
   ///  - figure out how to display the tracker
   ///  
   /// </summary>

    public class TrackerSequence
    {
        public string SequenceName { get; set; }
        public int ChannelCount { get; set; }
        public List<TrackerPattern> Patterns { get; set; }
    }

    public class TrackerPattern
    {
        public int TicksPerRow { get; set; }
        public string PatternName { get; set; }
        public List<SequenceRow> Rows { get; set; }
    }

    public class SequenceRow
    {
        public List<(int midiChannel,int col, SeqData data)> Channels { get; set; }
    }

    public class SeqData
    {
        

        private int? _pitch = 0;
        private byte? _velocity = 0;
        private TrackerInstrument _trackerInstrument;
        private SeqParam _seqParam;

        public int? Pitch
        {
            get => _pitch;
            set
            {
                _pitch = value;
            }
        }

        public byte? Velocity
        {
            get => _velocity;
            set
            {
                _velocity = value;
            }
        }
        public TrackerInstrument Instrument
        {
            get => _trackerInstrument; 
            set
            {
                _trackerInstrument = value;
            }
        }
        public SeqParam Parameter
        {
            get => _seqParam; 
            set
            {
                _seqParam = value;
            }
        }

        public override string ToString()
        {
            string note = "...";
            if(Pitch.HasValue)
            {

                var n = MidiEngine.GetNote(Pitch.Value, "-");
                note = n.noteLabel + n.octave;
                if(Pitch.Value == -1)
                {
                    note = "== ";
                }
                if (Pitch.Value == -2)
                {
                    note = "~~ ";
                }
                if (Pitch.Value == -3)
                {
                    note = "^^ ";
                }
            }
            string instindex = "..";
            if(Instrument != null)
            {
                instindex = Instrument.Index.ToString();//It's not hex here since OpenMPT does not use hex for instruments
            }
            string velocity = " ..";
            if (Velocity.HasValue)
            {
                velocity = "v" + Velocity.Value;
            }
            string param = ".";
            if(Parameter != null)
            {
                param = Parameter.Mark.ToString() + Parameter.Value.ToString("X:2");
            }
            return "|"+note + instindex + velocity + param;
        }
    }

    public class TrackerInstrument
    {
        public byte Index { get; set; }
        public NumberedEntry Bank { get; set; }
        public NumberedEntry Instrument { get; set; }
    }

    public class SeqParam
    {
        public char Mark { get; set; }

        public byte Value { get; set; }
    }
}
